using System;
using System.IO;

namespace ConsoleFileManager
{
    public class Actions
    {
        FilePanel Active { get; set; }
        string PassiveStartDirectory { get; set; }

        int CurrentItem { get; set; }
        string CurrentItemName { get; set; }
        int FromX { get; set; }
        int UntilX { get; set; }
        int Height { get; set; }
        int Width { get; set; }
        string StartDirectory { get; set; }

        public Actions(FilePanel activePanel, string passiveStartDirectory, int width)
        {
            Active = activePanel;
            PassiveStartDirectory = passiveStartDirectory;

            StartDirectory = Active.StartDirectory;
            CurrentItem = Active.CurrentItem;
            CurrentItemName = Active.CurrentItemName;
            FromX = Active.FromX;
            UntilX = Active.UntilX;
            Height = Active.PanelHeight;
            Width = width;
        }

        internal void ChangeCurrentItem(int sizeOfChange)
        {
            if (sizeOfChange == 100)// page down
            {
                sizeOfChange = Active.ItemsOnPage;
            }
            else if (sizeOfChange == -100)// page up
            {
                sizeOfChange = Active.ItemsOnPage * -1;
            }

            Active.CurrentItem += sizeOfChange; // up & down arrow

            if (sizeOfChange == 1000) // End
            {
                Active.CurrentItem = Active.TotalItems - 1;
            }
            else if (sizeOfChange == -1000) // Home
            {
                Active.CurrentItem = 0;
            }

            if (Active.CurrentItem < 0)
            {
                Active.CurrentItem = 0;
            }

            if (Active.CurrentItem > Active.TotalItems - 1)
            {
                Active.CurrentItem = Active.TotalItems - 1;
            }

            Active.ShowDirectoryContent();
        }

        internal void RenameItem(string actionName)
        {
            if (CurrentItem != 0) // not a parent Dir
            {
                string newName = AskUserForNewName(actionName);

                string sourceItem = Path.Combine(StartDirectory, CurrentItemName);
                DirectoryInfo dir = new DirectoryInfo(sourceItem);

                string targetItem = Path.Combine(StartDirectory, newName);

                MoveOrRename(sourceItem, targetItem, dir);
            }
        }

        internal void MoveItemTo(string targetDirectory)
        {
            string sourceItem = Path.Combine(StartDirectory, CurrentItemName);
            DirectoryInfo dir = new DirectoryInfo(sourceItem);

            string targetItem = Path.Combine(targetDirectory, CurrentItemName);

            MoveOrRename(sourceItem, targetItem, dir);
        }

        private void MoveOrRename(string sourceItem, string targetItem, DirectoryInfo dir)
        {
            if (CurrentItem != 0) // not a parent Dir
            {
                if (!dir.Attributes.ToString().Contains("Directory")) // file 
                {
                    try
                    {
                        // To move a file or folder to a new location:
                        File.Move(sourceItem, targetItem, true);
                    }
                    catch (Exception f)
                    {
                        ShowAlert(f.Message);
                    }
                }
                else // dir
                {
                    DirectoryInfo dirTarget = new DirectoryInfo(targetItem);

                    if (dir.Exists && !dirTarget.Exists)
                    {
                        try
                        {
                            Directory.Move(sourceItem, targetItem);
                        }
                        catch (Exception d)
                        {
                            ShowAlert(d.Message);
                        }
                    }
                    else
                    {
                        ShowAlert($"Directory '{targetItem}' already exist!");
                    }
                }
            }
        }

        internal void CreateNewDir(string actionName)
        {
            string newName = AskUserForNewName(actionName);

            if (newName.Length > 0)
            {
                string newDir = Path.Combine(StartDirectory, newName);
                DirectoryInfo dirTarget = new DirectoryInfo(newDir);

                if (!dirTarget.Exists)
                {
                    try
                    {
                        Directory.CreateDirectory(newDir);
                    }
                    catch (Exception d)
                    {
                        ShowAlert(d.Message);
                    }
                }
                else
                {
                    ShowAlert($"Directory '{newDir}' already exist!");
                }
            }
        }

        internal void DeleteItem(string startDirectory)
        {
            if (CurrentItem != 0) // not a parent Dir
            {
                string itemToDelete = Path.Combine(StartDirectory, CurrentItemName);
                DirectoryInfo dirInfo = new DirectoryInfo(itemToDelete);
                try
                {
                    if (!dirInfo.Attributes.ToString().Contains("Directory")) //  files
                    {
                        FileInfo fileInf = new FileInfo(itemToDelete);
                        fileInf.Delete();
                    }
                    else // dir
                    {
                        dirInfo.Delete(true);
                    }
                }
                catch (Exception f)
                {
                    ShowAlert(f.Message);
                }
            }
        }   

        internal void ExecuteCurrent()
        {
            if (CurrentItem != 0) // not a parent Dir
            {
                string itemToExe = Path.Combine(StartDirectory, CurrentItemName);
                DirectoryInfo dir = new DirectoryInfo(itemToExe);

                if (!dir.Attributes.ToString().Contains("Directory")) // file 
                {
                    try
                    {
                        System.Diagnostics.Process process = new System.Diagnostics.Process();
                        process.StartInfo.FileName = itemToExe;
                        process.StartInfo.UseShellExecute = true;
                        process.Start();
                    }
                    catch (Exception ex)
                    {
                        // Show Alert ?
                    }
                }
                else // dir
                {
                    Active.StartDirectory = itemToExe;
                    Active.CurrentItem = 0;
                    Active.ShowDirectoryContent();
                }

            }
            else // go to parent dir
            {
                DirectoryInfo dir = new DirectoryInfo(StartDirectory);
                if (dir.Parent != null && dir.Parent.Exists)
                {
                    Active.StartDirectory = dir.Parent.FullName.ToString();
                }
                else
                {
                    //  go to disk root
                    Active.StartDirectory = dir.FullName.ToString().Substring(0, dir.FullName.IndexOf('\\'));
                }

                Active.CurrentItem = 0;
                Active.ShowDirectoryContent();
            }
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
                        ShowAlert(ex.Message);
                    }
                }
                else // dir copy
                {
                    // If the destination directory doesn't exist, create it.
                    try
                    {
                        Directory.CreateDirectory(targetItem);
                    }
                    catch (Exception d)
                    {
                        ShowAlert(d.Message);
                    }

                    // Get the files in the directory and copy them to the new location.
                    DirectoryInfo[] dirs = dirInfo.GetDirectories();
                    FileInfo[] files = dirInfo.GetFiles();
                    foreach (FileInfo file in files)
                    {
                        string tempPath = Path.Combine(targetItem, file.Name);
                        try
                        {
                            file.CopyTo(tempPath, false);
                        }
                        catch (Exception f)
                        {
                            ShowAlert(f.Message);
                        }
                    }

                    // copying subdirectories, copy them and their contents to new location.
                    foreach (DirectoryInfo subdir in dirs)
                    {
                        string tempPath = Path.Combine(targetItem, subdir.Name);
                        try
                        {
                            CopyItemTo(subdir.FullName, tempPath);
                        }
                        catch (Exception f)
                        {
                            ShowAlert(f.Message);
                        }
                    }
                }
            }
        }

        internal void ShowHelp()
        {
            int lineNumber = Height / 10;
            string actionName = "Help";

            //header
            Console.SetCursorPosition((UntilX - FromX) / 2, lineNumber);
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            int padding = ((UntilX - FromX) / 2) + (actionName.Length / 2);
            Console.Write(actionName.PadRight(padding).PadLeft(UntilX - FromX));


            Console.SetCursorPosition((UntilX - FromX) / 2, ++lineNumber);
            Console.WriteLine(" F1 - Help Panel ".PadRight(UntilX - FromX));

            Console.SetCursorPosition((UntilX - FromX) / 2, ++lineNumber);
            Console.WriteLine(" F2 - Rename File or Directory".PadRight(UntilX - FromX));

            Console.SetCursorPosition((UntilX - FromX) / 2, ++lineNumber);
            Console.WriteLine(" F5 - Copy File or Directory".PadRight(UntilX - FromX));

            Console.SetCursorPosition((UntilX - FromX) / 2, ++lineNumber);
            Console.WriteLine(" F6 - Move File or Directory".PadRight(UntilX - FromX));

            Console.SetCursorPosition((UntilX - FromX) / 2, ++lineNumber);
            Console.WriteLine(" F7 - Create new Directory".PadRight(UntilX - FromX));

            Console.SetCursorPosition((UntilX - FromX) / 2, ++lineNumber);
            Console.WriteLine(" F8 - Delete File or Directory".PadRight(UntilX - FromX));

            Console.SetCursorPosition((UntilX - FromX) / 2, ++lineNumber);
            Console.WriteLine("  ".PadRight(UntilX - FromX));

            Console.SetCursorPosition((UntilX - FromX) / 2, ++lineNumber);
            Console.WriteLine(" Manual commands, commands - case insensitive: ".PadRight(UntilX - FromX));

            Console.SetCursorPosition((UntilX - FromX) / 2, ++lineNumber);
            Console.WriteLine(" (Ctrl + Enter): Copy current element to command line".PadRight(UntilX - FromX));

            Console.SetCursorPosition((UntilX - FromX) / 2, ++lineNumber);
            Console.WriteLine(" Change directory: cd NewPath ".PadRight(UntilX - FromX));

            Console.SetCursorPosition((UntilX - FromX) / 2, ++lineNumber);
            Console.WriteLine(" Copy: cp sourcePath TargetPath ".PadRight(UntilX - FromX));

            Console.SetCursorPosition((UntilX - FromX) / 2, ++lineNumber);
            Console.WriteLine(" Move: mv sourcePath TargetPath ".PadRight(UntilX - FromX));

            Console.SetCursorPosition((UntilX - FromX) / 2, ++lineNumber);
            Console.WriteLine(" Rename: rm sourcePath TargetPath ".PadRight(UntilX - FromX));

            Console.SetCursorPosition((UntilX - FromX) / 2, ++lineNumber);
            Console.WriteLine(" New Directory: mkdir newName ".PadRight(UntilX - FromX));

            Console.SetCursorPosition((UntilX - FromX) / 2, ++lineNumber);
            Console.WriteLine(" Execute File: run FileName ".PadRight(UntilX - FromX));

            //footer
            Console.SetCursorPosition((UntilX - FromX) / 2, ++lineNumber);
            Console.Write(" ".PadRight(UntilX - FromX));
            Console.SetCursorPosition((UntilX - FromX) / 2, ++lineNumber);
            Console.Write(" Press Enter to close panel.".PadRight(UntilX - FromX));

            Console.SetCursorPosition((UntilX - FromX) / 2 + 20, lineNumber);
            string s = Console.ReadLine();

            Console.BackgroundColor = ConsoleColor.Black;
        }

        internal bool UserConfirmAction(string actionName, string targetDirectory)
        {
            bool isConfirm = false;
            if (CurrentItem != 0) // not a parent Dir
            {
                int lineNumber = Height / 3;
                string itemToExe = Path.Combine(StartDirectory, CurrentItemName);
                DirectoryInfo dirInfo = new DirectoryInfo(itemToExe);

                Console.SetCursorPosition((UntilX - FromX) / 2, lineNumber);
                Console.BackgroundColor = ConsoleColor.DarkRed;
                int padding = ((UntilX - FromX) / 2) + (actionName.Length / 2);
                Console.Write(actionName.PadRight(padding).PadLeft(UntilX - FromX));

                Console.SetCursorPosition((UntilX - FromX) / 2, ++lineNumber);
                string source = $" {actionName} {CurrentItemName} ";
                Console.Write(source.PadRight(UntilX - FromX));

                if (actionName.Equals("Move") || actionName.Equals("Copy"))
                {
                    Console.SetCursorPosition((UntilX - FromX) / 2, ++lineNumber);
                    Console.Write(" to " + targetDirectory.PadRight(UntilX - FromX - 4));
                }

                Console.SetCursorPosition((UntilX - FromX) / 2, ++lineNumber);
                Console.Write(" Press Y to confirm, any other to decline, then Enter".PadRight(UntilX - FromX));

                Console.SetCursorPosition((UntilX - FromX) / 2, ++lineNumber);
                Console.Write(" Y ? ".PadRight(UntilX - FromX));

                Console.SetCursorPosition((UntilX - FromX) / 2 + 6, lineNumber);
                string decision = Console.ReadLine();

                if (decision.ToLower().Equals("y"))
                    isConfirm = true;

                Console.BackgroundColor = ConsoleColor.Black;
            }

            return isConfirm;
        }

        internal string AnalizeCommand(string newCommandText)
        {
            string info = "Unknown command";

            if (newCommandText.Length >= 5)
            {
                if (newCommandText.Substring(0, 5).ToLower().Equals("mkdir"))
                {
                    info = "MakeNewDir";
                }
            }
            else if (newCommandText.Length >= 3)
            {
                if (newCommandText.Substring(0, 3).ToLower().Equals("run"))
                {
                    info = "Execute File";
                }
            }
            else if (newCommandText.Length >= 2)
            {
                if (newCommandText.Substring(0, 2).ToLower().Equals("cd"))
                {
                    info = "ChangeDir";
                }
                else if (newCommandText.Substring(0, 2).ToLower().Equals("cp"))
                {
                    info = "Copy";
                }
                else if (newCommandText.Substring(0, 2).ToLower().Equals("mv"))
                {
                    info = "MoveeDirFile";
                }
                else if (newCommandText.Substring(0, 2).ToLower().Equals("rm"))
                {
                    info = "RemoveDirFile";
                }
            }

            //if contain 'space' - try recognize command
            //else try recognize dir/file

            Console.SetCursorPosition(1, Height - 4);
            Console.Write("Info: " + info);

            int cursor = Console.CursorLeft;
            Console.Write("`".PadRight(Width - (9 + info.Length), '`'));
            Console.CursorLeft = cursor;

            return info;
        }

        private string AskUserForNewName(string actionName)
        {
            int lineNumber = Height / 3;

            Console.SetCursorPosition((UntilX - FromX) / 2, lineNumber);
            Console.BackgroundColor = ConsoleColor.Blue;
            int padding = ((UntilX - FromX) / 2) + (actionName.Length / 2);
            Console.Write(actionName.PadRight(padding).PadLeft(UntilX - FromX));

            Console.SetCursorPosition((UntilX - FromX) / 2, ++lineNumber);
            Console.Write(" Enter new name, or leave it empty and press Enter:".PadRight(UntilX - FromX));

            Console.SetCursorPosition((UntilX - FromX) / 2, ++lineNumber);
            Console.Write(" ".PadRight(UntilX - FromX));

            Console.SetCursorPosition((UntilX - FromX) / 2 + 1, lineNumber);
            string newName = Console.ReadLine();

            Console.BackgroundColor = ConsoleColor.Black;

            return newName;
        }

        private void ShowAlert(string alertText)
        {
            int lineNumber = Height / 3;
            string actionName = "Error when execute!";

            //header
            Console.SetCursorPosition((UntilX - FromX) / 2, lineNumber);
            Console.BackgroundColor = ConsoleColor.Red;
            int padding = ((UntilX - FromX) / 2) + (actionName.Length / 2);
            Console.Write(actionName.PadRight(padding).PadLeft(UntilX - FromX));

            //int delimeter = alertText.Length / (UntilX - FromX);
            int charPointer = 0;
            int charPointerEnd = (UntilX - FromX) - 2;
            while (charPointer < alertText.Length)
            {
                Console.SetCursorPosition((UntilX - FromX) / 2, ++lineNumber);
                if (charPointerEnd > alertText.Length)
                {                    
                    Console.WriteLine(" " + alertText.Substring(charPointer).PadRight(UntilX - (FromX + 2)) + " ");
                }
                else
                {
                    Console.WriteLine(" " + alertText.Substring(charPointer, charPointerEnd) + " ");
                }
                
                charPointer = charPointerEnd + 1;
                charPointerEnd = charPointer + charPointerEnd;
            }

            //footer
            Console.SetCursorPosition((UntilX - FromX) / 2, ++lineNumber);
            Console.Write(" ".PadRight(UntilX - FromX));
            Console.SetCursorPosition((UntilX - FromX) / 2, ++lineNumber);
            Console.Write(" Press Enter to close alert.".PadRight(UntilX - FromX));

            Console.SetCursorPosition((UntilX - FromX) / 2 + 20, lineNumber);
            string s = Console.ReadLine();

            Console.BackgroundColor = ConsoleColor.Black;
        }
    }
}
