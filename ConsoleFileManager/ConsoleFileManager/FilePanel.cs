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
        public int PanelHeight { get; set; }
        //public int ItemNameSize { get; set; }

        public FilePanel(string StartDir)
        {
            StartDirectory = StartDir;

            //ShowDirectoryContent(StartDirectory);            
        }

        public void ShowDirectoryContent(string startDirectory)
        {
            Console.Clear();
            string dirName = startDirectory;

            Console.WriteLine("StartDir: " + startDirectory + " PanelWidth: " + PanelWidth + " PanelHeight: " + PanelHeight);
            Console.WriteLine("123456789012345678901234567890123456789012345678901234567890123456789012345=75");

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
                    //Console.WriteLine(fileInf.Name.PadRight(maxNameLenght) + fileInf.CreationTime +  " " + fileInf.Length);

                    string formatted = PrintStringToConsole(fileInf.Name, null, fileInf.CreationTime, fileInf.Length);
                }
            }
        }

        private string PrintStringToConsole(string name, string attributes, DateTime creationTime, long fileSize)
        {
            string attr = "<DIR>";
            //if (attributes == null)
            //{
            //    attr = GetFileSize(fileSize);
            //}

            if (name.Length <= PanelWidth - 28)
            {
                name = name.PadRight(PanelWidth - 28);
            }
            else if (attributes == null) // files name shortening - extension always visible
            {
                string extension = name.Substring(name.LastIndexOf('.'));
                name = name.Substring(0, PanelWidth - (30 + extension.Length));
                name = name + ".." + extension;

                attr = GetFileSize(fileSize);
            }
            else // folders name shortening
            {
                name = name.Substring(0, PanelWidth - 31);
                name = name + "...";
            }

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
