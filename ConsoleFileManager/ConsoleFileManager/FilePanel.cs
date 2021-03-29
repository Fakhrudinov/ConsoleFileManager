using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ConsoleFileManager
{
    public class FilePanel
    {
        public string StartDirectory { get; set; }
        public bool IsActive { get; set; }
        public int CurrentItem { get; set; }
        string CurrentItemName { get; set; }
        public int TotalItems { get; set; }
        int ItemsOnPage { get; set; }
        public int FromX { get; set; }
        public int UntilX { get; set; }
        public int PanelHeight { get; set; }

        public FilePanel(string StartDir, int fromX, int untilX)
        {
            StartDirectory = StartDir;
            FromX = fromX;
            UntilX = untilX;
        }

        public void ShowDirectoryContent()
        {
            if (!IsActive)
                Console.ForegroundColor = ConsoleColor.DarkGray;

            Console.SetCursorPosition(FromX, 1);
            string currDir = "Current Dir: ";
            if ((currDir + StartDirectory).Length > UntilX - FromX)
                currDir = "";

            currDir = currDir + StartDirectory;
            Console.Write((currDir).PadRight(UntilX - (FromX + currDir.Length)));

            TotalItems = 0;
            List<string> allItems = new List<string>();
            int dirsCount = 0;
            int filesCount = 0;
            //long filesTotalSize = 0;
            int pagesCount = 0;
            int itemsOnPage = 0;
            int currentPage = 0;

            DriveInfo[] drives = DriveInfo.GetDrives();

            if (Directory.Exists(StartDirectory))
            {
                foreach (DriveInfo drive in drives)
                {
                    if (drive.Name.ToLower().Contains(StartDirectory.ToLower().Substring(0, StartDirectory.IndexOf('\\'))))
                    {
                        Console.SetCursorPosition(FromX, 2);
                        string currDisk = $"Current Disk: ";
                        if((currDisk + $"{drive.Name} Total:{GetFileSize(drive.TotalSize)} Free:{ GetFileSize(drive.TotalFreeSpace)}").Length > UntilX - FromX)
                            currDisk = "";
                        Console.Write(currDisk + $"{drive.Name} Total:{GetFileSize(drive.TotalSize)} Free:{ GetFileSize(drive.TotalFreeSpace)}");
                    }
                }

                // ++ все дир и файлы запихать в один лист
                string[] dirs = Directory.GetDirectories(StartDirectory);
                string[] files = Directory.GetFiles(StartDirectory);
                dirsCount = dirs.Length;
                filesCount = files.Length;
                allItems.Add("..");
                allItems.AddRange(dirs);
                allItems.AddRange(files);

                TotalItems = allItems.Count;
                if (CurrentItem > TotalItems)
                    CurrentItem = 0;
                
                itemsOnPage = PanelHeight - 11;
                ItemsOnPage = itemsOnPage;

                //++ понять на какой странице CurrentItem
                //вывести эту страницу
                if (itemsOnPage > TotalItems)
                {
                    itemsOnPage = TotalItems;
                    pagesCount = 1;
                    currentPage = 1;
                }
                else
                {
                    pagesCount = TotalItems / itemsOnPage;

                    if (TotalItems % itemsOnPage > 0)
                        pagesCount++;

                    currentPage = (CurrentItem / itemsOnPage) + 1;
                }

                //fill array which contain line numbers to show
                int[] itemsToShow = new int[itemsOnPage];
                // if page is last - shrink numberes of lines
                if (currentPage > 1 && currentPage == pagesCount)
                {
                    itemsToShow = new int[TotalItems % itemsOnPage];
                }
                //fill 
                for (int i = 0; i < itemsToShow.Length; i++)
                {
                    if (currentPage > 1)
                    {
                        itemsToShow[i] = ((currentPage - 1) * itemsOnPage) + i; 
                    }
                    else
                    {
                        itemsToShow[i] = i;
                    }
                }

                //understand where is CurrentItem in array itemsToShow
                int arrCurrent = CurrentItem % itemsOnPage;

                //print lines og current page
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

                            string attr = dirInfo.Attributes.ToString();
                        if (!dirInfo.Attributes.ToString().Contains("Directory")) //  files
                        {
                            FileInfo fileInf = new FileInfo(allItems[itemsToShow[i]]);

                            PrintStringToConsole(dirInfo.Name, null, dirInfo.CreationTime, fileInf.Length, i);
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

                //fill all empty lines with spaces - delete previous data

                if (currentPage == 1 || (currentPage > 1 && currentPage == pagesCount))
                {
                    for (int i = itemsToShow.Length; i < PanelHeight - 11; i++)
                    {
                        Console.SetCursorPosition(FromX, 4 + i);

                        Console.WriteLine(" ".PadRight(UntilX - FromX));
                    }
                }
            }
            else
            {
                Console.SetCursorPosition(FromX, 2);
                Console.Write("Path not found: " + StartDirectory);
            }

            //Console.WriteLine($"ttl itm{TotalItems} itmOnPg{itemsOnPage} Page {currentPage}/{pagesCount} Curr{CurrentItem} dir{dirsCount}/files{filesCount}");
            //Console.Write(Console.BufferWidth + "/" + Console.WindowWidth + "//" + Console.BufferHeight + "/" + Console.WindowHeight);
            string paginationSummary = $"Page {currentPage} from {pagesCount}. Total Dirs: {dirsCount}, Files: {filesCount}";
            Console.SetCursorPosition(((UntilX - FromX - paginationSummary.Length) / 2) + FromX, PanelHeight - 7);
            Console.Write(paginationSummary.PadRight((UntilX - FromX - paginationSummary.Length) / 2), '═');

            Console.ResetColor();
        }

        internal void ExecuteCurrent()
        {
            if (CurrentItem != 0) // not a parent Dir
            {
                string itemToExe = Path.Combine(StartDirectory, CurrentItemName);
                DirectoryInfo dirInfo = new DirectoryInfo(itemToExe);
                if (!dirInfo.Attributes.ToString().Contains("Directory")) // file 
                {

                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    process.StartInfo.FileName = itemToExe;
                    process.StartInfo.UseShellExecute = true;
                    process.Start();
                }
                else // dir
                {
                    StartDirectory = itemToExe;
                    CurrentItem = 0;
                }
            }
            else // go to parent dir
            {
                DirectoryInfo dirInfo = new DirectoryInfo(StartDirectory);
                if(dirInfo.Parent != null)
                {
                    StartDirectory = dirInfo.Parent.FullName.ToString();
                }

                CurrentItem = 0;

                // CurrentItemName &&&&
            }
            ShowDirectoryContent();
        }

        internal void CopyItemTo(string sourceDir, string targetDirectory)
        {
            string sourceItem = Path.Combine(sourceDir, CurrentItemName);
            string targetItem = Path.Combine(targetDirectory, CurrentItemName);

            if (CurrentItem != 0) // not a parent Dir
            {
                DirectoryInfo dirInfo = new DirectoryInfo(sourceItem);
                if (!dirInfo.Attributes.ToString().Contains("Directory")) // file copy
                {                    
                    try
                    {
                        Directory.CreateDirectory(targetDirectory);
                        File.Copy(sourceItem, targetItem, true);
                    }
                    catch (Exception ex)
                    {
                        // Show Alert ?
                    }
                }
                else // dir copy
                {
                    if (!dirInfo.Exists)
                    {
                        throw new DirectoryNotFoundException(
                            "Source directory does not exist or could not be found: "
                            + sourceItem);
                    }

                    // If the destination directory doesn't exist, create it.
                    try
                    {
                        Directory.CreateDirectory(targetItem);
                    }
                    catch (Exception)
                    {
                        // Show Alert ?
                    }

                    // Get the files in the directory and copy them to the new location.
                    DirectoryInfo[] dirs = dirInfo.GetDirectories();
                    FileInfo[] files = dirInfo.GetFiles();
                    foreach (FileInfo file in files)
                    {
                        string tempPath = Path.Combine(targetItem, file.Name);
                        file.CopyTo(tempPath, false);
                    }

                    // copying subdirectories, copy them and their contents to new location.
                    foreach (DirectoryInfo subdir in dirs)
                    {
                        string tempPath = Path.Combine(targetItem, subdir.Name);
                        CopyItemTo(subdir.FullName, tempPath);
                    }                  
                }
            }
            else
            {
                // Show Alert ?
            }
        }

        /// <summary>
        /// Action when pressed UpArrow DownArrow PageDown PageUp Home End
        /// </summary>
        /// <param name="sizeOfChange"></param>
        internal void ChangeCurrentItem(int sizeOfChange)
        {
            if      (sizeOfChange == 100)   
                sizeOfChange = ItemsOnPage;
            else if (sizeOfChange == -100)  
                sizeOfChange = ItemsOnPage * -1;

            CurrentItem += sizeOfChange;

            if (sizeOfChange == 1000)
                CurrentItem = TotalItems - 1;
            else if (sizeOfChange == -1000)
                CurrentItem = 0;

            if (CurrentItem < 0)
                CurrentItem = 0;

            if (CurrentItem > TotalItems - 1)
                CurrentItem = TotalItems - 1;

            ShowDirectoryContent();
        }

        private void PrintStringToConsole(string name, string attributes, DateTime creationTime, long fileSize, int lineNum)
        {
            string attr = "<DIR>";

            Console.SetCursorPosition(FromX, 4 + lineNum);

            if (name.Equals(".."))
            {
                Console.WriteLine(".." + "<ParentDIR>".PadLeft(UntilX - (FromX + 2)));
            }
            else
            {
                if (name.Length <= UntilX - (27 + FromX) && attributes == null) // files 
                {
                    name = name.PadRight(UntilX - (27 + FromX));
                    attr = GetFileSize(fileSize);
                }
                else if (name.Length <= UntilX - (27 + FromX))
                {
                    name = name.PadRight(UntilX - (27 + FromX));
                }
                else if (name.Length > UntilX - (27 + FromX) && attributes == null) // files name shortening - extension always visible
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

                Console.Write(name + creationTime.ToString(" yy/MM/dd HH:mm:ss ") + attr.PadLeft(8)); // dt.lenght = 19 
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