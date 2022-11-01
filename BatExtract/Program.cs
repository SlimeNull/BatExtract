using BatExtract;
using System.Text;

internal class Program
{
    private static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            foreach (string path in args)
            {
                string? outputName =
                    Directory.Exists(path) ? Path.GetFileName(path) + ".bat" :
                    File.Exists(path) ? Path.GetFileName(path) + ".bat" :
                    null;

                if (outputName == null)
                    continue;

                string script = new ExtractBat()
                    .SetPath(path)
                    .BuildScript();

                File.WriteAllText(outputName, script, Encoding.Default);
                Console.WriteLine($"{outputName} OK");
            }
        }
    }
}