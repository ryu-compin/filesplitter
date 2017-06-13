using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Filesplitter
{
    class Program
    {
        static void Main(string[] args)
        {
            printHeader();
            string sourceFilePath = "";
            ulong maxFileSize = 1000000000;
            if(args.Length < 1)
            {
                printHelp("パラメーターに誤りがあります。引数を指定して下さい。",1);
            }
            foreach(string itm in args)
            {
                if((itm == "/?")||(itm == "-h") || (itm == "--h") || (itm == "-help") || (itm == "--help"))
                {
                    printHelp();
                }
            }
            if (args.Length == 2)
            {
                sourceFilePath = args[0];
                try
                {
                    maxFileSize = ulong.Parse(args[1]);
                }
                catch (Exception ex)
                {
                    errorExit(@"第二引数は分割ファイルの最大値を数値（byte）で指定してください。" + "\n" + ex.ToString());
                }
                if (System.IO.File.Exists(sourceFilePath))
                {
                    // 分割実行
                    Console.WriteLine("--- 分割開始 ---");
                    Console.WriteLine("ファイル ： " + sourceFilePath);
                    Console.WriteLine("分割サイズ（byte） : " + maxFileSize.ToString());

                    if (SplitFile(sourceFilePath, maxFileSize))
                    {
                        Console.WriteLine("ファイルの分割が完了しました。");
                    }
                }
                else
                {
                    errorExit(@"指定されたファイルは存在しません。" + "\n[ " + sourceFilePath + " ]");

                }

            }
        }
        // ファイル分割
        private static Boolean SplitFile(string sourceFilePath, ulong maxFileSize)
        {
            string fileDirectory = System.IO.Path.GetDirectoryName(sourceFilePath);
            string fileName = System.IO.Path.GetFileName(sourceFilePath);
            // ファイル読み込みバッファサイズ
            ulong bufferSize = 10000000;
            ulong writeCount = maxFileSize / bufferSize;

            using (FileStream inputFileStream = new FileStream(sourceFilePath, FileMode.Open))
            {
                int readByte = 0;
                int divCount = 0;
                long leftByte = inputFileStream.Length;
                byte[] readBuf = new byte[bufferSize];
                List<ulong> readMap = new List<ulong>();
                for (ulong i = 0; i < writeCount; i++)
                {
                    readMap.Add(bufferSize);
                }
                if (maxFileSize % bufferSize > 0)
                {
                    readMap.Add(maxFileSize % bufferSize);
                }
                while (leftByte > 0)
                {
                    // 分割ファイル名
                    string divFileName = sourceFilePath + "." + (++divCount).ToString();
                    Console.WriteLine("分割ファイル - " + divCount.ToString() + " : " + divFileName);
                    // 分割ファイルを開く
                    using (FileStream outputFileStream = new FileStream(divFileName, FileMode.Create, FileAccess.Write))
                    {
                        foreach (int mapByte in readMap)
                        {
                            // 分割元ファイルから読み込む
                            readByte = inputFileStream.Read(readBuf, 0, (int)Math.Min(mapByte, leftByte));
                            // 分割ファイルに書きこむ
                            outputFileStream.Write(readBuf, 0, readByte);
                            // 読込情報の設定
                            leftByte -= readByte;
                            if (leftByte == 0) break;
                        }
                    }
                }

            }
            return true;
        }
        // ヘッダメッセージ
        static void printHeader()
        {
            // バージョン
            System.Diagnostics.FileVersionInfo ver = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string headerString = @"+++++++++++++++++++++++++++++++++++++++++++++++++
    FileSplitter - 巨大ファイル分割機能
        Version : "
+ ver.FileVersion
+ @"
        Auther : R.Shindo
+++++++++++++++++++++++++++++++++++++++++++++++++";
            Console.WriteLine(headerString);
        }
        // Helpメッセージを出力してexit
        static void printHelp(string messageString = "", int exitCode = 0)
        {
            System.Diagnostics.FileVersionInfo ver = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string path = Process.GetCurrentProcess().MainModule.FileName;
            string helpString = @"引数の指定
第一引数：分割元ファイル名
第二引数：分割ファイルのサイズ

コマンド実行例
"
+ ver.InternalName + @" [file name] [max file size]"
;
            if(messageString != "")
            {
                Console.WriteLine(messageString);
            }
            Console.WriteLine(helpString);
            Environment.Exit(exitCode);

        }
        // Errorメッセージを出力してexit
        static void errorExit(string errorMessage, int exitCode = 1)
        {
            Console.WriteLine("--- エラーが発生しました。 ---");
            Console.WriteLine(errorMessage);
            Console.WriteLine("------------------------------");
            Environment.Exit(exitCode);
        }
    }
}
