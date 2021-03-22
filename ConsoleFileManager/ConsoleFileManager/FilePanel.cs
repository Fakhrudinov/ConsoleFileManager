using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ConsoleFileManager
{
    public class FilePanel
    {
        public string StartDirectory { get; set; }
        public string CurrentItem { get; set; }
        public int PanelWidth { get; set; }
        public int FromX { get; set; }
        public int UntilX { get; set; }
        public int PanelHeight { get; set; }

        //public FilePanel(string StartDir)
        //{
        //    StartDirectory = StartDir;
        //}

        public FilePanel(string StartDir, int fromX, int untilX)
        {
            StartDirectory = StartDir;
            FromX = fromX;
            UntilX = untilX;
        }

        public void ShowDirectoryContent(string startDirectory)
        {
            Console.SetCursorPosition(FromX, 0);
            Console.WriteLine("StartDir: " + startDirectory + " PanelWidth: " + PanelWidth + " PanelHeight: " + PanelHeight + "untilX: " + UntilX);
            Console.SetCursorPosition(FromX, 1);
            Console.WriteLine("123456789012345678901234567890123456789012345678901234567890123456789012345");
            string dirName = startDirectory;

            Console.CursorTop = 3;

            if (Directory.Exists(dirName))
            {
                string[] dirs = Directory.GetDirectories(dirName);
                foreach (string s in dirs)
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(s);

                    string formatted = PrintStringToConsole(dirInfo.Name, dirInfo.Attributes.ToString(), dirInfo.CreationTime, 0);
                }

                string[] files = Directory.GetFiles(dirName);
                foreach (string s in files)
                {
                    FileInfo fileInf = new FileInfo(s);

                    string formatted = PrintStringToConsole(fileInf.Name, null, fileInf.CreationTime, fileInf.Length);
                }
            }
        }

        private string PrintStringToConsole(string name, string attributes, DateTime creationTime, long fileSize)
        {
            string attr = "<DIR>";

            if (name.Length <= UntilX - (28 + FromX))
            {
                name = name.PadRight(UntilX - (28 + FromX));
            }
            else if (name.Length > UntilX - (28 + FromX) && attributes == null ) // files name shortening - extension always visible
            {
                string extension = name.Substring(name.LastIndexOf('.'));
                name = name.Substring(0, UntilX - ((30 + FromX) + extension.Length));
                name = name + ".." + extension;

                attr = GetFileSize(fileSize);
            }
            else if (name.Length <= UntilX - (28 + FromX) && attributes == null) // files 
            {
                // продебаж это name.Length <= UntilX - (28 + FromX)

                //string extension = name.Substring(name.LastIndexOf('.'));
                //name = name.Substring(0, UntilX - ((30 + FromX) + extension.Length));
                //name = name + ".." + extension;

                attr = GetFileSize(fileSize);
            }
            else // folders name shortening
            {
                name = name.Substring(0, UntilX - (31 + FromX));
                name = name + "...";
            }

            Console.CursorLeft = FromX;
            Console.WriteLine(name + creationTime.ToString(" yy/MM/dd HH:mm:ss ") + attr.PadLeft(9)); // dt.lenght = 19 

            return name;
        }

        private string GetFileSize(long fileSize)
        {
            string[] SizeSuffixes = { " b", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

            if (fileSize < 0) { return "-" + GetFileSize(-fileSize); }

            int i = 0;
            decimal dValue = (decimal)fileSize;
            while (Math.Round(dValue / 1024) >= 1)
            {
                dValue /= 1024;
                i++;
            }

            return string.Format("{0:n2} {1}", dValue, SizeSuffixes[i]);
        }
    }
}
