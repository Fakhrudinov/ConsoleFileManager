using System;
using System.Collections.Generic;
using System.IO;


namespace ConsoleFileManager
{
    internal class FilePanel
    {
        internal string StartDirectory { get; set; }
        internal bool IsActive { get; set; }
        internal int CurrentItem { get; set; }
        internal string CurrentItemName { get; set; }
        internal int TotalItems { get; set; }
        private int _itemsOnPage;
        internal int ItemsOnPage
        {
            get
            {
                return _itemsOnPage;
            }
            set
            {
                if (value > 0)
                _itemsOnPage = value;
            }
        }
        internal int FromX { get; set; }
        internal int UntilX { get; set; }
        internal int PanelHeight { get; set; }
        internal List<string> AllItems { get; set; }

        internal FilePanel(string StartDir, int fromX, int untilX)
        {
            StartDirectory = StartDir;
            FromX = fromX;
            UntilX = untilX;
        }

        internal void ShowDirectoryContent()
        {
            if (!IsActive)
                Console.ForegroundColor = ConsoleColor.DarkGray;

            ClassLibrary.Do.SetCursorPosition(FromX, 1);
            string currDir = "Current Dir: ";
            if ((currDir + StartDirectory).Length > UntilX - FromX)
                currDir = "";

            currDir = currDir + StartDirectory;

            currDir = ClassLibrary.Do.TextLineCutter(currDir, UntilX - FromX);
            Console.Write(currDir);

            TotalItems = 0;
            List<string> allItems = new List<string>();
            int dirsCount = 0;
            int filesCount = 0;
            int pagesCount = 0;
            int currentPage = 0;

            DriveInfo[] drives = DriveInfo.GetDrives();

            if (Directory.Exists(StartDirectory))
            {
                foreach (DriveInfo drive in drives)
                {
                    if (drive.Name.ToLower().Contains(StartDirectory.ToLower().Substring(0, 2)))
                    {
                        ClassLibrary.Do.SetCursorPosition(FromX, 2);

                        string currDisk = $"Current Disk: " + $"{drive.Name} Total:{GetFileSize(drive.TotalSize)} " +
                            $"Free:{ GetFileSize(drive.TotalFreeSpace)}";
                        if(currDisk.Length > UntilX - FromX)
                            currDisk = $"{drive.Name} Total:{GetFileSize(drive.TotalSize)}";
                        
                        currDisk = ClassLibrary.Do.TextLineCutter(currDisk, UntilX - FromX);
                        Console.Write(currDisk);
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
                    ClassLibrary.Do.ShowAlert($"File panel - when try to get dirs and files from {StartDirectory} Error - " + e.Message, UntilX - FromX);
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
                        if (IsActive)
                            Console.BackgroundColor = ConsoleColor.DarkGray;
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
                                ClassLibrary.Do.ShowAlert($"File panel - when try get data from file {allItems[itemsToShow[i]]} error - " + e.Message, UntilX - FromX);
                            }                            
                        }
                        else // dirs
                        {
                            try
                            {
                                PrintStringToConsole(dirInfo.Name, dirInfo.Attributes.ToString(), dirInfo.CreationTime, 0, i);
                            }
                            catch (Exception e)
                            {
                                ClassLibrary.Do.ShowAlert($"File panel - when try get data from directory {allItems[itemsToShow[i]]} error - " + e.Message, UntilX - FromX);
                            }                            
                        }
                    }

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
            else // something wrong with current dir
            {
                ClassLibrary.Do.ShowAlert("Current directory path not found: " + StartDirectory, UntilX - FromX);

                //is disk exist?
                DirectoryInfo dir = new DirectoryInfo(StartDirectory);
                if (StartDirectory.Contains(Path.DirectorySeparatorChar))
                {
                    dir = new DirectoryInfo(StartDirectory.Substring(0, StartDirectory.IndexOf(Path.DirectorySeparatorChar)));
                }
                
                if(dir.Exists) // if exist - roll back on directory tree until find existing dir
                {
                    StartDirectory = StartDirectory.Substring(0, StartDirectory.LastIndexOf(Path.DirectorySeparatorChar));
                }
                else // find ready disk and set as StartDirectory
                {
                    foreach (DriveInfo drive in drives)
                    {
                        if (drive.IsReady)
                        {
                            StartDirectory = drive.RootDirectory.ToString();
                            break;
                        }
                    }
                }

                ShowDirectoryContent();
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
    }
}