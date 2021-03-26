using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ConsoleFileManager
{
    public class FilePanel
    {
        public string StartDirectory { get; set; }
        //public string CurrentItem { get; set; }
        //public int PanelWidth { get; set; }
        public int FromX { get; set; }
        public int UntilX { get; set; }
        public int PanelHeight { get; set; }

        public FilePanel(string StartDir, int fromX, int untilX)
        {
            StartDirectory = StartDir;
            FromX = fromX;
            UntilX = untilX;
        }

        public void ShowDirectoryContent(string startDirectory)
        {
            Console.SetCursorPosition(FromX, 1);
            string currDir = "Current Dir: ";
            if ((currDir + startDirectory).Length > UntilX - FromX)
                currDir = "";

            Console.Write(currDir + startDirectory);

            int dirsCount = 0;
            int filesCount = 0;
            long filesTotalSize = 0;

            DriveInfo[] drives = DriveInfo.GetDrives();

            if (Directory.Exists(startDirectory))
            {
                foreach (DriveInfo drive in drives)
                {
                    if (drive.Name.ToLower().Contains(startDirectory.ToLower().Substring(0, startDirectory.IndexOf('\\'))))
                    {
                        Console.SetCursorPosition(FromX, 2);
                        string currDisk = $"Current Disk: ";
                        if((currDisk + $"{drive.Name} Total:{GetFileSize(drive.TotalSize)} Free:{ GetFileSize(drive.TotalFreeSpace)}").Length > UntilX - FromX)
                            currDisk = "";
                        Console.Write(currDisk + $"{drive.Name} Total:{GetFileSize(drive.TotalSize)} Free:{ GetFileSize(drive.TotalFreeSpace)}");
                    }
                }

                Console.SetCursorPosition(FromX, 4);

                string[] dirs = Directory.GetDirectories(startDirectory);
                dirsCount = dirs.Length;
                foreach (string s in dirs)
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(s);

                    PrintStringToConsole(dirInfo.Name, dirInfo.Attributes.ToString(), dirInfo.CreationTime, 0);
                }

                string[] files = Directory.GetFiles(startDirectory);
                filesCount = files.Length;
                foreach (string s in files)
                {
                    FileInfo fileInf = new FileInfo(s);

                    PrintStringToConsole(fileInf.Name, null, fileInf.CreationTime, fileInf.Length);

                    filesTotalSize = filesTotalSize + fileInf.Length;
                }
            }
            else
            {
                Console.SetCursorPosition(FromX, 2);
                Console.Write("Path not found: " + startDirectory);
            }

            
            Console.SetCursorPosition(FromX, PanelHeight - 7);
            Console.Write(Console.BufferWidth + "/" + Console.WindowWidth + "//" + Console.BufferHeight + "/" + Console.WindowHeight);//$"Pagination: Shown (X dirs / Y files) from {dirsCount}/{filesCount},");
            //Console.SetCursorPosition(FromX, PanelHeight - 5);
            //Console.WriteLine($"DIRs/Files {dirsCount}/{filesCount}, Total files size = {GetFileSize(filesTotalSize)}");
            //Console.WriteLine($"DIRs/Files {dirsCount}/{filesCount}, Total files size = {GetFileSize(filesTotalSize)}");


        }

        private string PrintStringToConsole(string name, string attributes, DateTime creationTime, long fileSize)
        {
            string attr = "<DIR>";

            if (name.Length <= UntilX - (27 + FromX) && attributes == null) // files 
            {
                name = name.PadRight(UntilX - (27 + FromX));
                attr = GetFileSize(fileSize);
            }
            else if (name.Length <= UntilX - (27 + FromX))
            {
                name = name.PadRight(UntilX - (27 + FromX));
            }
            else if (name.Length > UntilX - (27 + FromX) && attributes == null ) // files name shortening - extension always visible
            {
                string extension = name.Substring(name.LastIndexOf('.'));
                name = name.Substring(0, UntilX - ((29 + FromX) + extension.Length));
                name = name + ".." + extension;

                attr = GetFileSize(fileSize);
            }
            else // folders name shortening
            {
                name = name.Substring(0, UntilX - (30 + FromX));
                name = name + "...";
            }

            Console.CursorLeft = FromX;
            Console.Write(name + creationTime.ToString(" yy/MM/dd HH:mm:ss ") + attr.PadLeft(8)); // dt.lenght = 19 

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

            return string.Format("{0:n2}{1}", dValue, SizeSuffixes[i]);
        }
    }
}
