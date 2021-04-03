using System;
using System.IO;

namespace ConsoleFileManager
{
    public class Actions
    {
        enum Command
        {
            ChangeDir,  // 0
            Copy,       // 1
            MakeNewDir, // 2
            Move,       // 3
            Remove,     // 4
            RunFile,    // 5
            EqualisePanels, // 6
            Unknown
        }

        string Info { get; set; }

        public bool CanBeExecute { get; set; }
        public int CommandType { get; set; }
        public string ArgumentSource { get; set; }
        public string ArgumentTarget { get; set; }

        FilePanel Active { get; set; }
        FilePanel Passive { get; set; }

        int CurrentItem { get; set; }
        string CurrentItemName { get; set; }
        int FromX { get; set; }
        int UntilX { get; set; }
        int Height { get; set; }
        int Width { get; set; }
        string StartDirectory { get; set; }


        public Actions(FilePanel activePanel, FilePanel passivePanel, int width)
        {
            Active = activePanel;
            Passive = passivePanel;
            StartDirectory = Active.StartDirectory;
            CurrentItem = Active.CurrentItem;
            CurrentItemName = Active.CurrentItemName;
            FromX = Active.FromX;
            UntilX = Active.UntilX;
            Height = Active.PanelHeight;
            Width = width;

            CanBeExecute = false;
            CommandType = (int)Command.Unknown;
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
                MkDir(newName);
            }
        }

        private void MkDir(string newName)
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
            //string actionName = "Help";
            string longest = " (Ctrl + Enter): Copy current element to command line"; // longest text lenght
            int xCursor = (UntilX - FromX) - (longest.Length / 2);
            int totalLenght = longest.Length + 1;

            //header
            Console.SetCursorPosition(xCursor, lineNumber);
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            int padding = (totalLenght / 2) + ("Help".Length / 2);
            Console.Write("Help".PadRight(padding).PadLeft(totalLenght));

            PrintLinePanelText(" F1 - Help Panel", xCursor, ++lineNumber, totalLenght);
            PrintLinePanelText(" F2 - Rename File or Directory", xCursor, ++lineNumber, totalLenght);
            PrintLinePanelText(" F5 - Copy File or Directory", xCursor, ++lineNumber, totalLenght);
            PrintLinePanelText(" F6 - Move File or Directory", xCursor, ++lineNumber, totalLenght);
            PrintLinePanelText(" F7 - Create new Directory", xCursor, ++lineNumber, totalLenght);
            PrintLinePanelText(" F8 - Delete File or Directory", xCursor, ++lineNumber, totalLenght);
            PrintLinePanelText(" ", xCursor, ++lineNumber, totalLenght);
            PrintLinePanelText(" Manual commands, commands - case insensitive.", xCursor, ++lineNumber, totalLenght);
            PrintLinePanelText(longest, xCursor, ++lineNumber, totalLenght);
            PrintLinePanelText(" Set passive panel same as active: equal", xCursor, ++lineNumber, totalLenght);
            PrintLinePanelText(" Change directory: cd NewPath ", xCursor, ++lineNumber, totalLenght);
            PrintLinePanelText(" Copy: cp sourcePath (to passive panel path)", xCursor, ++lineNumber, totalLenght);
            PrintLinePanelText(" Move: mv sourcePath, TargetPath", xCursor, ++lineNumber, totalLenght);
            PrintLinePanelText(" Rename: rm sourcePath, TargetPath", xCursor, ++lineNumber, totalLenght);
            PrintLinePanelText(" New Directory: mkdir newName", xCursor, ++lineNumber, totalLenght);
            PrintLinePanelText(" Execute File: run FileName", xCursor, ++lineNumber, totalLenght);
            PrintLinePanelText(" Delete: del TargetPath", xCursor, ++lineNumber, totalLenght);
            PrintLinePanelText(" ", xCursor, ++lineNumber, totalLenght);
            PrintLinePanelText(" Press Enter to close panel.", xCursor, ++lineNumber, totalLenght);

            Console.SetCursorPosition(xCursor + 28, lineNumber);
            Console.ReadLine();

            Console.BackgroundColor = ConsoleColor.Black;
        }

        private void PrintLinePanelText(string text, int x, int y, int lenght)
        {
            Console.SetCursorPosition(x, y);
            Console.WriteLine(text.PadRight(lenght));
        }

        internal bool UserConfirmAction(string actionName, string targetDirectory)
        {
            bool isConfirm = false;
            if (CurrentItem != 0) // not a parent Dir
            {
                int lenght;
                int lineNumber = Height / 3;
                
                string text = " Press Y to confirm, any other to decline, then Enter";
                string source = $" {actionName} {CurrentItemName} ";                
                if (actionName.Equals("Move") || actionName.Equals("Copy"))
                {
                    lenght = GetLongestInt(" to " + targetDirectory, text, source);
                }
                else
                {
                    lenght = GetLongestInt(text, source);
                }

                int xCursor = (UntilX - FromX) - (lenght / 2);

                Console.SetCursorPosition(xCursor, lineNumber);
                Console.BackgroundColor = ConsoleColor.DarkRed;
                //int padding = (xCursor) + (actionName.Length / 2);
                Console.Write(actionName.PadRight((lenght / 2) + (actionName.Length / 2)).PadLeft(lenght));

                PrintLinePanelText(source, xCursor, ++lineNumber, lenght);
                if (actionName.Equals("Move") || actionName.Equals("Copy"))
                {
                    PrintLinePanelText(" to " + targetDirectory, xCursor, ++lineNumber, lenght);
                }
                PrintLinePanelText(text, xCursor, ++lineNumber, lenght);
                PrintLinePanelText(" Y ? ", xCursor, ++lineNumber, lenght);

                Console.SetCursorPosition(xCursor + 6, lineNumber);
                string decision = Console.ReadLine();

                if (decision.ToLower().Equals("y"))
                    isConfirm = true;

                Console.BackgroundColor = ConsoleColor.Black;
            }

            return isConfirm;
        }

        private int GetLongestInt(params string[] list)
        {
            int result = 0;
            foreach (string str in list)
            {
                if (str.Length > result)
                    result = str.Length;
            }

            return ++result;
        }

        internal void AnalizeCommand(string newCommandText, bool execute)
        {
            Info = "Unknown command";
            CanBeExecute = false;
            CommandType = (int)Command.Unknown;

            if (newCommandText.Length >= 5)
            {
                if (newCommandText.Substring(0, 5).ToLower().Equals("mkdir"))
                {
                    Info = "MakeNewDir: mkdir [New Directory in active panel]";
                    if(newCommandText.Length > 5)
                        CommandController(Command.MakeNewDir, newCommandText.Substring(5));
                }
                if (newCommandText.Substring(0, 5).ToLower().Equals("equal"))
                {
                    Info = "Set passive panel path same as active: equal";
                    if (newCommandText.Length == 5)
                        CommandController(Command.EqualisePanels, newCommandText.Substring(5));
                    else if (newCommandText.Length > 5)
                        Info = "Wrong format: Equal must be without arguments";
                }
            }
            else if (newCommandText.Length >= 3)
            {
                if (newCommandText.Substring(0, 3).ToLower().Equals("run"))
                {
                    Info = "Execute File: run [fileName in active panel]";
                }
            }
            else if (newCommandText.Length >= 2)
            {
                if (newCommandText.Substring(0, 2).ToLower().Equals("cd"))
                {
                    Info = "ChangeDir: cd [Directory in active panel or new path]";
                }
                else if (newCommandText.Substring(0, 2).ToLower().Equals("cp"))
                {
                    Info = "Copy to pasive panel: cp [object in active panel]";
                }
                else if (newCommandText.Substring(0, 2).ToLower().Equals("mv"))
                {
                    Info = "Move to pasive panel: mv [object in active panel], [optional New name]";
                }
                else if (newCommandText.Substring(0, 2).ToLower().Equals("rm"))
                {
                    Info = "Remove Dir or File: rem [object in active panel]";
                }
            }

            if (execute)
            {
                ExecuteUserCommand();
                Info = "";
            }

            PrintCommandInfo();
        }

        private void PrintCommandInfo()
        {
            Console.SetCursorPosition(1, Height - 4);
            Console.Write(Info);

            if (Info.Length < Width - 2)
                Console.Write("`".PadRight(Width - (2 + Info.Length), '`'));
        }

        internal void ExecuteUserCommand()
        {
            switch (CommandType)
            {
                case (int)Command.EqualisePanels:
                    Passive.StartDirectory = Active.StartDirectory;
                    break;
                case (int)Command.MakeNewDir:
                    MkDir(ArgumentTarget);
                    break;
            }
        }

        private void CommandController(Command type, string arguments)
        {
            switch (type)
            {
                case Command.EqualisePanels:
                    CanBeExecute = true;
                    CommandType = (int)Command.EqualisePanels;
                    break;
                case Command.MakeNewDir:
                    bool checkArguments = CheckExist(arguments, false);
                    if (checkArguments)
                    {
                        CanBeExecute = true;
                        CommandType = (int)Command.MakeNewDir;
                        ArgumentTarget = arguments.Substring(1);
                    }
                    break;
            }
        }



        private bool CheckExist(string arguments, bool expected)
        {
            // firstly check - first symbol must be ' ' as delimeter from command and argument
            if (arguments[0].Equals(' '))
            {
                // ok it is space
                bool actual = false;

                foreach (string str in Active.AllItems)
                {
                    if (str.ToLower().Equals(arguments.ToLower()))
                        actual = true;
                }

                if (expected == actual)
                    return true;
                else
                    return false;
            }
            else
            {
                //wrong command!
                Info = "There is no space between command and path!";

                return false;
            }
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

        public void ShowAlert(string alertText)
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
