using System;
using System.IO;

namespace ConsoleFileManager
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            * Alexander Fakhrudinov = Александр Фахрудинов
            * asbuka@gmail.com
            */

            //string str = @"c:\temp\5 erw yr tyrtey\";
            //string str1 = ".";
            //string str2 = "..";
            //string str3 = "\\";
            //string str4 = "\\..";
            ////str = Path.Combine(str, str1);
            //bool a1 = EXIST(Path.Combine(str, str1));
            //bool a2 = EXIST(Path.Combine(str, str2));
            //bool a3 = EXIST(Path.Combine(str, str3));
            //bool a4 = EXIST(Path.Combine(str, str4));


            Console.Title = "Console File Manager, Alexander Fakhrudinov";
            FileManager fm = new FileManager();
        }

        //private static bool EXIST(string v)
        //{
        //    DirectoryInfo dirSource = new DirectoryInfo(v);
        //    if (dirSource.Exists)
        //    {
        //        return true;
        //    }

        //    return false;
        //}
    }
}
