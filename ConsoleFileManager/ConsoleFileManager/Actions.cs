using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
        int PanelHeight { get; set; }
        string StartDirectory { get; set; }

        public Actions(FilePanel activePanel, string passiveStartDirectory)
        {
            Active = activePanel;
            PassiveStartDirectory = passiveStartDirectory;

            StartDirectory = Active.StartDirectory;
            CurrentItem = Active.CurrentItem;
            CurrentItemName = Active.CurrentItemName;
            FromX = Active.FromX;
            UntilX = Active.UntilX;
            PanelHeight = Active.PanelHeight;
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



        private string AskUserForNewName(string actionName)
        {
            int lineNumber = PanelHeight / 3;

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
                    catch (Exception)
                    {
                        // Show Alert ?
                    }
                }
                else // dir
                {
                    DirectoryInfo dirTarget = new DirectoryInfo(targetItem);

                    if (dir.Exists && !dirTarget.Exists)
                        try
                        {
                            Directory.Move(sourceItem, targetItem);
                        }
                        catch (Exception)
                        {
                            // Show Alert ?
                        }
                    else
                    {
                        // Show Alert ?
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
                    catch (Exception)
                    {
                        // Show Alert ?
                    }
                }
                else
                {
                    // Show Alert ?
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
                catch (Exception)
                {
                    // Show Alert ?
                }
            }
        }

        internal bool UserConfirmAction(string actionName, string targetDirectory)
        {
            bool isConfirm = false;
            if (CurrentItem != 0) // not a parent Dir
            {
                int lineNumber = PanelHeight / 3;
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
                        // Show Alert ?
                    }
                }
                else // dir copy
                {
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

    }
}
