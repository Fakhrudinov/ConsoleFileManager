﻿using System;
using System.Collections.Generic;
using System.IO;


namespace ConsoleFileManager
{
    public class FilePanel
    {
        public string StartDirectory { get; set; }
        public bool IsActive { get; set; }
        public int CurrentItem { get; set; }
        public string CurrentItemName { get; set; }
        public int TotalItems { get; set; }
        int itemsOnPage;
        public int ItemsOnPage
        {
            get
            {
                return itemsOnPage;
            }
            set
            {
                if (value > 0)
                itemsOnPage = value;
            }
        }
        public int FromX { get; set; }
        public int UntilX { get; set; }
        public int PanelHeight { get; set; }
        public List<string> AllItems { get; set; }

        public FilePanel(string StartDir, int fromX, int untilX)
        {
            StartDirectory = StartDir;
            FromX = fromX;
            UntilX = untilX;
        }

        private string TextLineCutter(string text, int maxLenght)
        {
            if (text.Length > maxLenght)
            {

                string text1 = text.Substring(0, maxLenght / 2 - 1) + ".."; 
                text = text1 + text.Substring((text.Length - (maxLenght / 2) + 1));

            }
            else
            {
                text = text.PadRight(maxLenght);
            }

            return text;
        }

        public void ShowDirectoryContent()
        {
            if (!IsActive)
                Console.ForegroundColor = ConsoleColor.DarkGray;

            ClassLibrary.Do.SetCursorPosition(FromX, 1);
            string currDir = "Current Dir: ";
            if ((currDir + StartDirectory).Length > UntilX - FromX)
                currDir = "";

            currDir = currDir + StartDirectory;

            currDir = TextLineCutter(currDir, UntilX - FromX);
            Console.Write(currDir);

            TotalItems = 0;
            List<string> allItems = new List<string>();
            int dirsCount = 0;
            int filesCount = 0;
            int pagesCount = 0;
            //int itemsOnPage = 0;
            int currentPage = 0;

            DriveInfo[] drives = DriveInfo.GetDrives();

            if (Directory.Exists(StartDirectory))
            {
                foreach (DriveInfo drive in drives)
                {
                    if (drive.Name.ToLower().Contains(StartDirectory.ToLower().Substring(0, 2)))
                    {
                        ClassLibrary.Do.SetCursorPosition(FromX, 2);

                        string currDisk = $"Current Disk: ";
                        if((currDisk + $"{drive.Name} Total:{GetFileSize(drive.TotalSize)} Free:{ GetFileSize(drive.TotalFreeSpace)}").Length > UntilX - FromX)
                            currDisk = "";
                        Console.Write(currDisk + $"{drive.Name} Total:{GetFileSize(drive.TotalSize)} Free:{ GetFileSize(drive.TotalFreeSpace)}");
                    }
                }

                // all dirs and files put in one List<string>
                string[] dirs = new string [0];
                string[] files = new string[0];
                try
                {
                    dirs = Directory.GetDirectories(StartDirectory);
                    files = Directory.GetFiles(StartDirectory);
                }
                catch (Exception e)
                {
                    ClassLibrary.Do.ShowAlert(e.Message, UntilX - FromX);
                    //ShowAlert(e.Message);
                }
                // fill List<string>
                dirsCount = dirs.Length;
                filesCount = files.Length;
                allItems.Add("..");
                allItems.AddRange(dirs);
                allItems.AddRange(files);

                AllItems = allItems;

                TotalItems = allItems.Count;
                if (CurrentItem > TotalItems)
                    CurrentItem = 0;

                ItemsOnPage = PanelHeight - 11;

                // understand page which contains CurrentItem
                if (ItemsOnPage > TotalItems)
                {
                    ItemsOnPage = TotalItems;
                    pagesCount = 1;
                    currentPage = 1;
                }
                else
                {
                    pagesCount = TotalItems / ItemsOnPage;

                    if (TotalItems % ItemsOnPage > 0)
                        pagesCount++;

                    currentPage = (CurrentItem / ItemsOnPage) + 1;
                }

                //fill array which contain line numbers to show
                int[] itemsToShow = new int[ItemsOnPage];
                // if page is last - shrink numberes of array elements
                if (currentPage > 1 && currentPage == pagesCount)
                {
                    itemsToShow = new int[TotalItems % ItemsOnPage];
                }
                //fill  array
                for (int i = 0; i < itemsToShow.Length; i++)
                {
                    if (currentPage > 1)
                    {
                        itemsToShow[i] = ((currentPage - 1) * ItemsOnPage) + i; 
                    }
                    else
                    {
                        itemsToShow[i] = i;
                    }
                }

                //understand where is CurrentItem in array itemsToShow
                int arrCurrent = CurrentItem % ItemsOnPage;

                //print lines of current page
                for (int i = 0; i < itemsToShow.Length; i++)
                {
                    if(arrCurrent == i)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGray;

                        if (!IsActive)
                            Console.ForegroundColor = ConsoleColor.Black;
                    }                        
                    
                    if (allItems[itemsToShow[i]].Equals("..")) // print parent Directory
                    {
                        PrintStringToConsole("..", null, DateTime.MinValue, 0, i);
                    }
                    else 
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(allItems[itemsToShow[i]]);
                        if (arrCurrent == i && IsActive)
                            CurrentItemName = dirInfo.Name;

                        if (!dirInfo.Attributes.ToString().Contains("Directory")) //  files
                        {
                            FileInfo fileInf = new FileInfo(allItems[itemsToShow[i]]);

                            try
                            {
                                PrintStringToConsole(dirInfo.Name, null, dirInfo.CreationTime, fileInf.Length, i);
                            }
                            catch (Exception e)
                            {
                                ClassLibrary.Do.ShowAlert(StartDirectory + " " + e.Message, UntilX - FromX);
                                //ShowAlert(StartDirectory + " " + e.Message);
                            }                            
                        }
                        else // dirs
                        {
                            PrintStringToConsole(dirInfo.Name, dirInfo.Attributes.ToString(), dirInfo.CreationTime, 0, i);
                        }
                    }

                    Console.BackgroundColor = ConsoleColor.Black;
                    if (!IsActive)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    }
                    else
                    {
                        Console.ResetColor();
                    }                        
                }

                //fill all empty lines with spaces - delete previous data in panel
                if (currentPage == 1 || (currentPage > 1 && currentPage == pagesCount))
                {
                    for (int i = itemsToShow.Length; i < PanelHeight - 11; i++)
                    {
                        ClassLibrary.Do.SetCursorPosition(FromX, 4 + i);
                        Console.WriteLine(" ".PadRight(UntilX - FromX));
                    }
                }
            }
            else
            {
                ClassLibrary.Do.SetCursorPosition(FromX, 2);
                Console.Write("Path not found: " + StartDirectory);
            }

            // pagination summary:
            string paginationSummary = $"Page {currentPage} from {pagesCount}. Total Dirs: {dirsCount}, Files: {filesCount}";
            if(UntilX - FromX < 45)
                paginationSummary = $"Page {currentPage}/{pagesCount}. Dirs/Files: {dirsCount}/{filesCount}";

            int cursorX = (UntilX - FromX - paginationSummary.Length) / 2;
            if (cursorX < 0)
                cursorX = 0;
            ClassLibrary.Do.SetCursorPosition(cursorX + FromX, PanelHeight - 7);
            Console.Write(paginationSummary.PadRight(cursorX), '═');

            Console.ResetColor();
        }

        private void PrintStringToConsole(string name, string attributes, DateTime creationTime, long fileSize, int lineNum)
        {
            string attr = "<DIR>";
            int padding = 27; // calculate 
            string dateTime = creationTime.ToString(" yy/MM/dd HH:mm:ss ");// 19 symbols

            ClassLibrary.Do.SetCursorPosition(FromX, 4 + lineNum);        
            if (name.Equals(".."))
            {
                Console.WriteLine(".." + "<ParentDIR>".PadLeft(UntilX - (FromX + 2)));
            }
            else
            {
                //decision = show date time or not
                if (UntilX - FromX < 50)
                {
                    dateTime = "";
                    padding = 8;
                }

                if (name.Length <= UntilX - (padding + FromX) && attributes == null) // files 
                {
                    name = name.PadRight(UntilX - (padding + FromX));
                    attr = GetFileSize(fileSize);
                }
                else if (name.Length <= UntilX - (padding + FromX))
                {
                    name = name.PadRight(UntilX - (padding + FromX));
                }
                else if (name.Length > UntilX - (padding + FromX) && attributes == null) // files name shortening - extension always visible
                {
                    string extension = name.Substring(name.LastIndexOf('.'));
                    int pad = UntilX - ((padding + 2 + FromX) + extension.Length);
                    if (pad < 0)
                        pad = 0;
                    name = name.Substring(0, pad);
                    name = name + ".." + extension;

                    attr = GetFileSize(fileSize);
                }
                else // folders name shortening
                {
                    int pad = UntilX - (padding + 3 + FromX);
                    if (pad < 0)
                        pad = 0;
                    name = name.Substring(0, pad);
                    name = name + "...";
                }

                Console.Write(name + dateTime + attr.PadLeft(8)); 
            }
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

        //public void ShowAlert(string alertText)
        //{
        //    int cursorX = (UntilX - FromX) / 2;
        //    int lineNumber = PanelHeight / 3;
        //    string actionName = "Error when execute!";

        //    //header
        //    SetCursorPosition(cursorX, lineNumber);
        //    Console.BackgroundColor = ConsoleColor.Red;
        //    int padding = (cursorX) + (actionName.Length / 2);
        //    Console.Write(actionName.PadRight(padding).PadLeft(UntilX - FromX));

        //    //int delimeter = alertText.Length / (UntilX - FromX);
        //    int charPointer = 0;
        //    int charPointerEnd = (UntilX - FromX) - 2;
        //    while (charPointer < alertText.Length)
        //    {
        //        SetCursorPosition(cursorX, ++lineNumber);
        //        if (charPointerEnd > alertText.Length)
        //        {
        //            Console.WriteLine(" " + alertText.Substring(charPointer).PadRight(UntilX - (FromX + 2)) + " ");
        //        }
        //        else
        //        {
        //            Console.WriteLine(" " + alertText.Substring(charPointer, charPointerEnd) + " ");
        //        }

        //        charPointer = charPointerEnd;
        //        charPointerEnd = charPointer + charPointerEnd;
        //    }

        //    //footer
        //    SetCursorPosition(cursorX, ++lineNumber);
        //    Console.Write(" ".PadRight(UntilX - FromX));
        //    SetCursorPosition(cursorX, ++lineNumber);
        //    Console.Write(" Press Enter to close alert.".PadRight(UntilX - FromX));

        //    SetCursorPosition(cursorX + 29, lineNumber);
        //    Console.ReadLine();

        //    Console.BackgroundColor = ConsoleColor.Black;
        //}

        //private void SetCursorPosition1(int x, int y)
        //{
        //    try
        //    {
        //        Console.SetCursorPosition(x, y);
        //    }
        //    catch (System.ArgumentOutOfRangeException)
        //    {

        //    }
        //}
    }
}