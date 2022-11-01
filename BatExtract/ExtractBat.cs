using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BatExtract
{
    public class ExtractBat
    {
        public static readonly string Version = "1.0.0beta";

        string? pathToRestore;

        public IEnumerable<string> GetFileTree(string path, int indent)
        {
            if (File.Exists(path))
            {
                FileInfo fileinfo = new FileInfo(path);
                yield return fileinfo.Name;
            }
            else if (Directory.Exists(path))
            {
                DirectoryInfo dirinfo = new DirectoryInfo(path);
                yield return dirinfo.Name;

                foreach (DirectoryInfo subdir in dirinfo.GetDirectories())
                    foreach (string node in GetFileTree(subdir.FullName, indent))
                        yield return $"{new string(' ', indent)}{node}";

                foreach (FileInfo subfile in dirinfo.GetFiles())
                    yield return $"{new string(' ', indent)}{subfile.Name}";
            }
        }

        public IEnumerable<string> GetAllDirectories(string dir)
        {
            if (Directory.Exists(dir))
            {
                DirectoryInfo dirinfo = new DirectoryInfo(dir);

                yield return dirinfo.Name;
                foreach (DirectoryInfo subdir in dirinfo.GetDirectories())
                    foreach (string node in GetAllDirectories(subdir.FullName))
                        yield return $"{dirinfo.Name}/{node}";
            }
        }

        public IEnumerable<(string origin, string related)> GetAllFiles(string path)
        {
            if (Directory.Exists(path))
            {
                DirectoryInfo dirinfo = new DirectoryInfo(path);
                
                foreach (DirectoryInfo subdir in dirinfo.GetDirectories())
                    foreach ((string origin, string related) node in GetAllFiles(subdir.FullName))
                        yield return  (node.origin, $"{dirinfo.Name}/{node.related}");
                foreach (FileInfo subfile in dirinfo.GetFiles())
                    yield return (subfile.FullName, $"{dirinfo.Name}/{subfile.Name}");
            }
            else if (File.Exists(path))
            {
                FileInfo fileinfo = new FileInfo(path);
                yield return (fileinfo.FullName, fileinfo.Name);
            }
        }

        public IEnumerable<string> SplitStringByBlockLength(string source, int len)
        {
            int start = 0, end = len;

            if (end > source.Length)
                end = source.Length;

            while (start < source.Length)
            {
                yield return source.Substring(start, end - start);
                start += len;
                end += len;

                if (end > source.Length)
                    end = source.Length;
            }
        }

        public ExtractBat SetPath(string path)
        {
            pathToRestore = path;
            return this;
        }

        public string BuildScript()
        {
            if (pathToRestore == null)
                throw new InvalidOperationException("Path is null");

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("@Echo off");

            sb.AppendLine($"Title FILE EXTRACT V{Version}");
            sb.AppendLine($"SetLocal EnableDelayedExpansion");

            sb.AppendLine($"Echo.// BAT EXTRACT V{Version} //");
            sb.AppendLine("Echo.");
            sb.AppendLine("Echo.File(s) to extract:");

            // === file list === //
            foreach (string node in GetFileTree(pathToRestore, 2))
                sb.AppendLine($"Echo.  {node}");

            sb.AppendLine("Echo.");
            sb.AppendLine("Echo.Input directory path for extraction target. (empty for current directory)");
            sb.AppendLine("Set /P target=: ");

            sb.AppendLine("Set target=%target:\"=%");                                      // clear double quote
            sb.AppendLine("( If \"!target:~-1!\"==\"/\" Set \"target=!target:~0,-1!\" )");     // clear end /
            sb.AppendLine("( If \"!target:~-1!\"==\"\\\" Set \"target=!target:~0,-1!\" )");    // clear end \
            sb.AppendLine("( If \"!target: =!\"==\"\" Set \"target=.\" )");          // clear spaces

            sb.AppendLine("( If Not Exist \"!target!\" (Echo.Path not exist & Pause) )");  // check dir

            foreach (string dir in GetAllDirectories(pathToRestore))
            {
                sb.AppendLine($"MkDir \"%target%/{dir}\" >Nul 2>Nul");                     // create directories
                sb.AppendLine($"Echo. Directory {dir} ok");
            }

            foreach ((string origin, string related) file in GetAllFiles(pathToRestore))
            {
                string base64 = Convert.ToBase64String(File.ReadAllBytes(file.origin));
                sb.AppendLine($"(");
                sb.AppendLine($"  Echo.-----BEGIN CERTIFICATE-----");
                foreach (string block in SplitStringByBlockLength(base64, 1024))
                    sb.AppendLine($"  Echo.{block}");
                sb.AppendLine($"  Echo.-----END CERTIFICATE-----");
                sb.AppendLine($") > \"%target%/{file.related}\"");


                sb.AppendLine($"CertUtil -F -Decode \"%target%/{file.related}\" \"%target%/{file.related}\"");
                sb.AppendLine($"Echo. File {file.related} ok");
            }

            sb.AppendLine("Pause");
            return sb.ToString();
        }
    }
}
