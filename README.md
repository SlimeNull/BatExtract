# BatExtract

创建自解压 bat 脚本. 通过这个文件, 你可以将多个文件, 甚至整个目录树嵌入进一个 bat 脚本中, 当运行脚本时, 脚本可以按照原有的顺序释放文件.

> 哈哈, 我知道这是个很无聊的程序, 不过, 玩玩而已啦~ (原理是使用 base64 编码以及自带的 certutil 工具进行解码)

## 使用:

语法: 
```sh
BatExtract 文件或目录 ...
```

实例, 它会在当前目录下创建 `hello.txt.bat` :
```sh
BatExtract C:\Users\MyName\Desktop\hello.txt
```

打开 bat 你会看到:
```txt
// BAT EXTRACT V1.0.0beta //

File(s) to extract:
  hello.txt

Input directory path for extraction target. (empty for current directory)
:
```

此时, 输入你要释放的目录位置, 他就会将 hello.txt 释放到指定的目录了