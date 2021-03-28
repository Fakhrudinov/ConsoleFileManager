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

        //public int PanelWidth { get; set; }
        public int CurrentItem { get; set; }
        public int TotalItems { get; set; }

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

            Console.Write(currDir + StartDirectory);

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

                if (currentPage > 1 && currentPage == pagesCount)
                {
                    for (int i = itemsToShow.Length; i < itemsOnPage; i++)
                    {
                        Console.SetCursorPosition(FromX, 4 + i);

                        Console.WriteLine(" ".PadRight(UntilX - FromX));
                    }
                }


                //
                //CurrentItem в пределах очереди
                //CurrentItem выходит вниз 
                //  очередь убавить сверху, прибавить снизу - нижний CurrentItem

                    //CurrentItem выходит ввверх 
                    //  очередь убавить снизу, прибавить сверху - верхний CurrentItem

                    //itemsOnPage увеличился - добавить к очереди снизу. если нет снизу - добавить сверху.

                    //itemsOnPage уменьшился -
                    //  убавлять снизу пока не достигнем CurrentItem.
                    //  достигнут CurrentItem - убавлять сверху

            }
            else
            {
                Console.SetCursorPosition(FromX, 2);
                Console.Write("Path not found: " + StartDirectory);
            }
            
            Console.SetCursorPosition(FromX, PanelHeight - 7);
            Console.WriteLine($"ttl itm{TotalItems} itmOnPg{itemsOnPage} Page {currentPage}/{pagesCount} Curr{CurrentItem} dir{dirsCount}/files{filesCount}");
            //Console.Write(Console.BufferWidth + "/" + Console.WindowWidth + "//" + Console.BufferHeight + "/" + Console.WindowHeight);
            //Console.Write($"Pagination: Shown ({totalItems} / {pagesCount}) from {dirsCount}/{filesCount} amnt:{PanelHeight - 18}");

            Console.ResetColor();
        }

        internal void ChangeCurrentItem(int sizeOfChange)
        {
            CurrentItem += sizeOfChange;

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

//if (CurrentItem < itemsOnPage)
//{
//    Console.SetCursorPosition(FromX, PanelHeight - (linesAmount + 7));
//    if (CurrentItem == 0)
//        Console.BackgroundColor = ConsoleColor.DarkGray;

//    Console.WriteLine("..");// root dir 

//    Console.BackgroundColor = ConsoleColor.Black;
//    linesAmount--;
//}

//if (linesAmount >= dirsCount)
//{
//    // show all dirs
//    foreach (string s in dirs)
//    {
//        if (CurrentItem == currentPage * itemsOnPage - linesAmount)
//            Console.BackgroundColor = ConsoleColor.DarkGray;

//        DirectoryInfo dirInfo = new DirectoryInfo(s);
//        PrintStringToConsole(dirInfo.Name, dirInfo.Attributes.ToString(), dirInfo.CreationTime, 0, linesAmount);
//        linesAmount--;

//        Console.BackgroundColor = ConsoleColor.Black;
//    }
//}
//else
//{
//    for (int dir = 0; dir < linesAmount; dir++)
//    {
//        if (CurrentItem == currentPage * itemsOnPage - linesAmount)
//            Console.BackgroundColor = ConsoleColor.DarkGray;

//        DirectoryInfo dirInfo = new DirectoryInfo(dirs[dir]);
//        PrintStringToConsole(dirInfo.Name, dirInfo.Attributes.ToString(), dirInfo.CreationTime, 0, linesAmount);

//        Console.BackgroundColor = ConsoleColor.Black;
//    }
//}

//if (linesAmount > 0)
//{
//    if (linesAmount >= filesCount)
//    {
//        //show all files
//        foreach (string s in files)
//        {
//            if (CurrentItem == currentPage * itemsOnPage - linesAmount)
//                Console.BackgroundColor = ConsoleColor.DarkGray;

//            FileInfo fileInf = new FileInfo(s);
//            PrintStringToConsole(fileInf.Name, null, fileInf.CreationTime, fileInf.Length, linesAmount);
//            filesTotalSize = filesTotalSize + fileInf.Length;

//            linesAmount--;
//            Console.BackgroundColor = ConsoleColor.Black;
//        }
//    }
//    else
//    {
//        for (int file = 0; file < linesAmount; file++)
//        {
//            if (CurrentItem == currentPage * itemsOnPage - linesAmount)
//                Console.BackgroundColor = ConsoleColor.DarkGray;

//            FileInfo fileInf = new FileInfo(files[file]);
//            PrintStringToConsole(fileInf.Name, null, fileInf.CreationTime, fileInf.Length, linesAmount - file);
//            filesTotalSize = filesTotalSize + fileInf.Length;

//            Console.BackgroundColor = ConsoleColor.Black;
//        }
//    }
//}