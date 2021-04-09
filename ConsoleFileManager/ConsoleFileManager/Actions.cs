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
            Rename, // 7
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
                string targetItem = Path.Combine(StartDirectory, newName);

                MoveOrRename(sourceItem, targetItem);
            }
        }

        internal void MoveItemTo(string targetDirectory)
        {
            if (CurrentItem != 0) // not a parent Dir
            {
                string sourceItem = Path.Combine(StartDirectory, CurrentItemName);
                string targetItem = Path.Combine(targetDirectory, CurrentItemName);

                MoveOrRename(sourceItem, targetItem);
            }
        }

        private void MoveOrRename(string sourceItem, string targetItem)
        {
            DirectoryInfo dirr = new DirectoryInfo(sourceItem);
            if (!dirr.Attributes.ToString().Contains("Directory")) //  files
            {
                try
                {
                    File.Move(sourceItem, targetItem, true);
                }
                catch (Exception f)
                {
                    Active.ShowAlert(f.Message);
                }
            }
            if (dirr.Exists) // dir
            {
                try
                {
                    Directory.Move(sourceItem, targetItem);
                }
                catch (Exception d)
                {
                    Active.ShowAlert(d.Message);
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
                    Active.ShowAlert(d.Message);
                }
            }
            else
            {
                Active.ShowAlert($"Directory '{newDir}' already exist!");
            }
        }

        internal void DeleteItem(string itemPath)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(itemPath);

            try
            {
                if (!dirInfo.Attributes.ToString().Contains("Directory")) //  files
                {
                    FileInfo fileInf = new FileInfo(itemPath);
                    fileInf.Delete();
                }
                else // dir
                {
                    dirInfo.Delete(true);
                }

                if (Active.CurrentItem > 0)
                    Active.CurrentItem--;
                else
                    Active.CurrentItem = 0;
            }
            catch (Exception d)
            {
                Active.ShowAlert(d.Message);
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
                    RunFile(itemToExe);
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
                if (dir.Parent != null && dir.Parent.Exists) // normal directory
                {
                    Active.StartDirectory = dir.Parent.FullName.ToString();
                }
                else
                {
                    //  go to disk root
                    Active.StartDirectory = dir.Root.ToString();
                }

                Active.CurrentItem = 0;
                Active.ShowDirectoryContent();
            }
        }

        private void RunFile(string itemToExe)
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
                Active.ShowAlert(ex.Message);
            }
        }

        internal void CopyFromPanel()
        {
            string sourceItem = Path.Combine(Active.StartDirectory, CurrentItemName);
            string targetItem = Path.Combine(Passive.StartDirectory, CurrentItemName);

            if (CurrentItem != 0) // not a parent Dir
            {
                DirectoryInfo dirInfo = new DirectoryInfo(sourceItem);

                if (!dirInfo.Attributes.ToString().Contains("Directory")) // file copy
                {
                    CopyFile(sourceItem, targetItem);
                }
                else // dir copy
                {
                    CopyDir(sourceItem, targetItem); 
                }
            }
        }

        internal void CopyFromCommandLine()
        {
            //ArgumentSource

            DirectoryInfo dirInfo = new DirectoryInfo(ArgumentSource);

            if (!dirInfo.Attributes.ToString().Contains("Directory")) // file copy
            {
                CopyFile(ArgumentSource, ArgumentTarget);
            }
            else // dir copy
            {
                CopyDir(ArgumentSource, ArgumentTarget);
            }
        }

        private void CopyDir(string sourceItem, string targetItem)
        {
            string[] dirs = Directory.GetDirectories(sourceItem, "*", SearchOption.AllDirectories);

            if(dirs.Length == 0)
            {
                //empty dir to copy - so just create it
                try
                {
                    Directory.CreateDirectory(targetItem);
                }
                catch (Exception ex)
                {
                    Active.ShowAlert("Creation Directory Error: " + ex.Message);
                }
            }
            else
            {
                foreach (string dir in dirs)
                {
                    try
                    {
                        string newDir = Path.Combine(targetItem, dir.Substring(sourceItem.Length + 1));
                        Directory.CreateDirectory(newDir);
                    }
                    catch (Exception ex)
                    {
                        Active.ShowAlert("Creation Directory Error: " + ex.Message);
                    }
                }
            }            

            foreach (string file_name in Directory.GetFiles(sourceItem, "*", SearchOption.AllDirectories))
            {
                try
                {
                    string newFile = Path.Combine(targetItem, file_name.Substring(sourceItem.Length + 1));
                    File.Copy(file_name, newFile);
                }
                catch (Exception ex)
                {
                    Active.ShowAlert("File copy error: " + ex.Message);
                }                
            }
        }

        private void CopyFile(string sourceItem, string targetItem)
        {
            string targetDirectory = targetItem.Substring(0, targetItem.LastIndexOf('\\'));           
            
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(targetDirectory);
                if(!dirInfo.Exists)
                    Directory.CreateDirectory(targetDirectory);
            }
            catch (Exception ex)
            {
                Active.ShowAlert("Creation Directory Error: " + ex.Message);
            }

            try
            {                
                File.Copy(sourceItem, targetItem, true);
            }
            catch (Exception ex)
            {
                Active.ShowAlert("File copy error: " + ex.Message);
            }
        }

        internal void ShowInfo(string itemPath)
        {
            int lineNumber = Height / 4;
            int xCursor = (UntilX - FromX) / 2;

            //header
            Console.SetCursorPosition(xCursor, lineNumber);
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            int padding = (xCursor) + ("Information".Length / 2);
            Console.Write("Information".PadRight(padding).PadLeft(xCursor * 2));

            if (Active.CurrentItem != 0) // not a parent dir
            {
                DirectoryInfo dirInfo = new DirectoryInfo(itemPath);                 
                if (!dirInfo.Attributes.ToString().Contains("Directory")) //  files
                {
                    FileInfo fileInf = new FileInfo(itemPath);
                    PrintLinePanelText( "        File", xCursor, ++lineNumber, xCursor * 2);
                    PrintLinePanelText($"        Name: {fileInf.Name}", xCursor, ++lineNumber, xCursor * 2);
                    PrintLinePanelText($"     Created: {fileInf.CreationTime}", xCursor, ++lineNumber, xCursor * 2);
                    PrintLinePanelText($" Last writed: {fileInf.LastWriteTime}", xCursor, ++lineNumber, xCursor * 2);
                    PrintLinePanelText($"    ReadOnly: {fileInf.IsReadOnly}", xCursor, ++lineNumber, xCursor * 2);
                    PrintLinePanelText($" Size, bytes: {fileInf.Length}", xCursor, ++lineNumber, xCursor * 2);
                    PrintLinePanelText($"  Attributes: {fileInf.Attributes}", xCursor, ++lineNumber, xCursor * 2);
                }
                else // dirs
                {
                    PrintLinePanelText( "   Directory", xCursor, ++lineNumber, xCursor * 2);
                    PrintLinePanelText($"        Name: {dirInfo.Name}", xCursor, ++lineNumber, xCursor * 2);
                    PrintLinePanelText($"     Created: {dirInfo.CreationTime}", xCursor, ++lineNumber, xCursor * 2);
                    PrintLinePanelText($" Last writed: {dirInfo.LastWriteTime}", xCursor, ++lineNumber, xCursor * 2);
                    PrintLinePanelText($"  Attributes: {dirInfo.Attributes}", xCursor, ++lineNumber, xCursor * 2);
                }
            }
            else
            {
                PrintLinePanelText(" Info about parent directory not avaliable.", xCursor, ++lineNumber, xCursor * 2);
                PrintLinePanelText(" Firstly go there and select directory", xCursor, ++lineNumber, xCursor * 2);
            }

            //footer
            PrintLinePanelText(" ", xCursor, ++lineNumber, xCursor * 2);
            PrintLinePanelText(" Press Enter to close panel.", xCursor, ++lineNumber, xCursor * 2);
            Console.SetCursorPosition(xCursor + 28, lineNumber);
            Console.ReadLine();

            Console.BackgroundColor = ConsoleColor.Black;
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
            PrintLinePanelText(" F3 - File or Directory Information", xCursor, ++lineNumber, totalLenght);
            PrintLinePanelText(" F5 - Copy File or Directory", xCursor, ++lineNumber, totalLenght);
            PrintLinePanelText(" F6 - Move File or Directory", xCursor, ++lineNumber, totalLenght);
            PrintLinePanelText(" F7 - Create new Directory", xCursor, ++lineNumber, totalLenght);
            PrintLinePanelText(" F8 - Delete File or Directory", xCursor, ++lineNumber, totalLenght);
            PrintLinePanelText(" F9 - Rename File or Directory", xCursor, ++lineNumber, totalLenght);
            PrintLinePanelText(" arrows Up and Down, PgUp PgDown - select items", xCursor, ++lineNumber, totalLenght);
            PrintLinePanelText(" Tab - switch between panels", xCursor, ++lineNumber, totalLenght);
            PrintLinePanelText(" ", xCursor, ++lineNumber, totalLenght);
            PrintLinePanelText(" Manual commands, commands - case insensitive.", xCursor, ++lineNumber, totalLenght);
            PrintLinePanelText(longest, xCursor, ++lineNumber, totalLenght);
            PrintLinePanelText(" Set passive panel same as active: equal", xCursor, ++lineNumber, totalLenght);
            PrintLinePanelText(" Change directory: cd NewPath ", xCursor, ++lineNumber, totalLenght);
            PrintLinePanelText(" Copy: cp sourcePath (to passive panel)", xCursor, ++lineNumber, totalLenght);
            PrintLinePanelText(" Move: mv sourcePath (to passive panel)", xCursor, ++lineNumber, totalLenght);
            PrintLinePanelText(" Remove: rm sourcePath", xCursor, ++lineNumber, totalLenght);
            PrintLinePanelText(" New Directory: mkdir newName", xCursor, ++lineNumber, totalLenght);
            PrintLinePanelText(" Execute File: run FileName", xCursor, ++lineNumber, totalLenght);
            PrintLinePanelText(" Rename: name sourcePath, newName", xCursor, ++lineNumber, totalLenght);
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

            if(newCommandText.Length >= 2)
            {
                if (newCommandText.Substring(0, 2).ToLower().Equals("cd"))
                {
                    Info = "ChangeDir: cd [Directory in active panel or new path]";
                    if (newCommandText.Length > 2)
                        CommandController(Command.ChangeDir, newCommandText.Substring(2));
                }
                else if (newCommandText.Substring(0, 2).ToLower().Equals("cp"))
                {
                    Info = "Copy to pasive panel: cp [object], [optional New Path]";
                    if (newCommandText.Length > 2)
                        CommandController(Command.Copy, newCommandText.Substring(2));
                }
                else if (newCommandText.Substring(0, 2).ToLower().Equals("mv"))
                {
                    Info = "Move to pasive panel: mv [object], [optional New Path]";
                    if (newCommandText.Length > 2)
                        CommandController(Command.Move, newCommandText.Substring(2));
                }
                else if (newCommandText.Substring(0, 2).ToLower().Equals("rm"))
                {
                    Info = "Remove Dir or File: rm [object]";
                    if (newCommandText.Length > 2)
                        CommandController(Command.Remove, newCommandText.Substring(2));
                }
                else if (newCommandText.Length >= 3)
                {
                    if (newCommandText.Substring(0, 3).ToLower().Equals("run"))
                    {
                        Info = "Run File: Run [fileName in active panel]";
                        if (newCommandText.Length > 3)
                            CommandController(Command.RunFile, newCommandText.Substring(3));
                    }
                    else if (newCommandText.Length >= 4)
                    {
                        if (newCommandText.Substring(0, 4).ToLower().Equals("name"))
                        {
                            Info = "Rename: Name [object], [New Name]";
                            if (newCommandText.Length > 4)
                                CommandController(Command.Rename, newCommandText.Substring(4));
                        }
                        else if (newCommandText.Length >= 5)
                        {
                            if (newCommandText.Substring(0, 5).ToLower().Equals("mkdir"))
                            {
                                Info = "MakeNewDir: mkdir [New Directory Name in active panel]";
                                if (newCommandText.Length > 5)
                                    CommandController(Command.MakeNewDir, newCommandText.Substring(5));
                            }
                            else if (newCommandText.Substring(0, 5).ToLower().Equals("equal"))
                            {
                                Info = "Set passive panel path same as active: equal";
                                if (newCommandText.Length == 5)
                                    CommandController(Command.EqualisePanels, newCommandText.Substring(5));
                                else if (newCommandText.Length > 5)
                                    Info = "Wrong format: Equal must be without arguments";
                            }
                        }
                    }                    
                }
            }

            // user press Enter - execute user command
            if (execute)
            {
                if (CanBeExecute)
                {
                    ExecuteUserCommand();
                }
                else
                {
                    Active.ShowAlert("Unknown command or not correct arguments");
                }

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
            string sourceItem = ArgumentSource;
            if (CommandType != (int)Command.EqualisePanels)
            {
                try
                {
                    DirectoryInfo source = new DirectoryInfo(sourceItem);
                    if (!source.Exists)
                    {
                        sourceItem = Path.Combine(Active.StartDirectory, ArgumentSource);
                    }
                }
                catch (Exception e)
                {
                    Active.ShowAlert(e.Message);
                }
            }

            switch (CommandType)
            {
                case (int)Command.EqualisePanels:
                    Passive.StartDirectory = Active.StartDirectory;
                    break;
                case (int)Command.MakeNewDir:
                    MkDir(ArgumentSource);
                    break;
                case (int)Command.RunFile:
                    RunFile(sourceItem);
                    break;
                case (int)Command.Remove:
                    DeleteItem(sourceItem);
                    break;
                case (int)Command.Rename:
                    MoveOrRename(ArgumentSource, ArgumentTarget);
                    break;
                case (int)Command.Move:
                    MoveOrRename(ArgumentSource, ArgumentTarget);
                    break;
                case (int)Command.Copy:
                    CopyFromCommandLine();
                    break;
                case (int)Command.ChangeDir:
                    Active.CurrentItem = 0;
                    Active.StartDirectory = ArgumentSource;
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
                        ArgumentSource = arguments.Substring(1);
                    }
                    break;

                case Command.ChangeDir:
                    checkArguments = CheckExist(arguments, true);
                    if (checkArguments)
                    {
                        CanBeExecute = true;
                        CommandType = (int)Command.ChangeDir;
                        ArgumentSource = GetCorrectPath(arguments.Substring(1));
                    }
                    break;

                case Command.RunFile:
                    checkArguments = CheckExist(arguments, true);
                    if (checkArguments)
                    {
                        CanBeExecute = true;
                        CommandType = (int)Command.RunFile;
                        ArgumentSource = arguments.Substring(1);
                    }
                    break;

                case Command.Remove:
                    checkArguments = CheckExist(arguments, true);
                    if (checkArguments)
                    {
                        CanBeExecute = true;
                        CommandType = (int)Command.Remove;
                        ArgumentSource = arguments.Substring(1);
                    }
                    break;

                case Command.Rename:
                    checkArguments = CheckPairExist(arguments, true, false);
                    if (checkArguments)
                        CommandType = (int)Command.Rename;
                    break;

                case Command.Move:
                    checkArguments = CheckPairExist(arguments, false, false);
                    if (checkArguments)
                        CommandType = (int)Command.Move;
                    break;
                case Command.Copy:
                    checkArguments = CheckPairExist(arguments, false, false);
                    if (checkArguments)
                        CommandType = (int)Command.Copy;
                    break;
            }
        }

        private bool CheckPairExist(string arguments, bool secondRequired, bool secondMustExist)
        {
            bool checkFirstArguments = false;
            bool checkSecondArguments = false;

            //first is always must be exist
            if (!arguments.Contains(','))
                checkFirstArguments = CheckExist(arguments, true);
            else
            {
                CanBeExecute = false;
                string arg1 = arguments.Substring(0, arguments.IndexOf(','));
                checkFirstArguments = CheckExist(arg1, true);

                //without correct first argument - second is useless
                if (checkFirstArguments == true)
                {
                    //Info = "First argument correct, now enter second argument";
                    if (arguments.Substring(arguments.IndexOf(',')).Length > 1)
                    {
                        checkSecondArguments = CheckExist(arguments.Substring(arguments.IndexOf(',') + 1), secondMustExist);

                        if (checkSecondArguments == true)// OK both correct
                        {
                            //Info = $"OK, From '{arguments.Substring(1, arguments.IndexOf(',') - 1)}' " +
                            //    $"to '{arguments.Substring(arguments.IndexOf(',') + 2)}'";

                            ArgumentSource = GetCorrectPath(arguments.Substring(1, arguments.IndexOf(',') - 1));
                            ArgumentTarget = GetNonExistPath(arguments.Substring(arguments.IndexOf(',') + 2));
                            
                            CanBeExecute = true;
                            return true;
                        }
                    }
                }
            }

            // if first exist and second not necessary = ok
            if (checkFirstArguments && !secondRequired)
            {
                if (!arguments.Contains(',')) // if second argumennt not entered only
                {
                    ArgumentSource = GetCorrectPath(arguments.Substring(1));
                    ArgumentTarget = Path.Combine(Passive.StartDirectory, ArgumentSource.Substring(ArgumentSource.LastIndexOf('\\') + 1));

                    CanBeExecute = true;
                    return true;
                }
            }

            return false;
        }

        private string GetNonExistPath(string path)
        {
            if (path.Contains('\\')) // full path
            {
               //check exist - for full path - must be NOT exist
               if (!CheckIsItFullPath(path))
               {
                    //check exist - for first part before last '\' - must be exist
                    if (!CheckIsItFullPath(path.Substring(0, path.LastIndexOf('\\'))))
                        path = Path.Combine(Passive.StartDirectory, path);
               }

                return path;
            }
            else // just name
            {
                path = Path.Combine(Passive.StartDirectory, path);
            }
            return path;
        }

        // check path - return if exist or combine with current directory
        private string GetCorrectPath(string pathOrItem)
        {
            bool exist = CheckIsItFullPath(pathOrItem);
            if (exist)
                return pathOrItem;
            else
                return Path.Combine(Active.StartDirectory, pathOrItem);
        }

        private bool CheckIsItFullPath(string sourceItem)
        {
            FileInfo fileSource = new FileInfo(sourceItem);
            if (fileSource.Exists)
            {
                return true;
            }
            // then check if it is direct path to dir
            DirectoryInfo dirSource = new DirectoryInfo(sourceItem);
            if (dirSource.Exists)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check for one argument. Used in MkDir Run Rm 
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="expected"></param>
        /// <returns></returns>
        private bool CheckExist(string arguments, bool expected)
        {
            // firstly check - first symbol must be ' ' as delimeter from command and argument
            if (arguments[0].Equals(' ')) // ok it is space
            {
                bool actual = false;

                string sourceItem = arguments.Substring(1);
                if (sourceItem.Length > 0)
                {
                    // firsly check if it is direct path to file or dir
                    actual = CheckIsItFullPath(sourceItem);

                    // if not - check this item in current folder
                    if (!actual)
                    {
                        sourceItem = Path.Combine(Active.StartDirectory, sourceItem).ToLower();
                        foreach (string str in Active.AllItems)
                        {
                            if (str.ToLower().Equals(sourceItem))
                            {
                                actual = true;
                                break;
                            }
                        }

                        //still not exist - check combined path
                        DirectoryInfo dirSource = new DirectoryInfo(sourceItem);
                        if (dirSource.Exists)
                        {
                            actual = true;
                        }
                    }
                }
                else
                {
                    Info = $"Enter Dir or File name, ' ' not enough.";
                    return false;
                }

                if (expected == actual)
                    return true;
                else
                {
                    if(expected)
                        Info = $"Directory or file '{arguments.Substring(1)}' not found!";
                    else
                        Info = $"Directory or file '{arguments.Substring(1)}' already exist!";

                    return false;
                }
            }
            else // wrong command!
            {                
                Info = "There is no space between command and path!" + arguments;

                return false;
            }
        }

        private string AskUserForNewName(string actionName)
        {
            int cursorX = (UntilX - FromX) / 2;
            int lineNumber = Height / 3;
            string requestText = " Enter new name, or leave it empty and press Enter:";
            int lenght = UntilX - FromX;


            if ((UntilX - FromX) < requestText.Length)
            {
                lenght = requestText.Length + 1;
                cursorX = (UntilX - FromX) - requestText.Length / 2;
            }
            
            Console.BackgroundColor = ConsoleColor.Blue;

            Console.SetCursorPosition(cursorX, lineNumber);
            Console.Write(actionName.PadRight(((lenght - actionName.Length) / 2) + actionName.Length).PadLeft(lenght));

            PrintLinePanelText(requestText, cursorX, ++lineNumber, lenght);
            PrintLinePanelText(" ", cursorX, ++lineNumber, lenght);
            //Console.SetCursorPosition(cursorX, ++lineNumber);
            //Console.Write(requestText.PadRight(lenght));

            //Console.SetCursorPosition(cursorX, ++lineNumber);
            //Console.Write(" ".PadRight(lenght));

            Console.SetCursorPosition(cursorX + 1, lineNumber);
            string newName = Console.ReadLine();

            Console.BackgroundColor = ConsoleColor.Black;

            return newName;
        }

        public void ShowAlertx(string alertText)
        {
            int cursorX = (UntilX - FromX) / 2;
            int lineNumber = Height / 3;
            string actionName = "Error when execute!";

            //header
            Console.SetCursorPosition(cursorX, lineNumber);
            Console.BackgroundColor = ConsoleColor.Red;
            int padding = (cursorX) + (actionName.Length / 2);
            Console.Write(actionName.PadRight(padding).PadLeft(UntilX - FromX));

            //int delimeter = alertText.Length / (UntilX - FromX);
            int charPointer = 0;
            int charPointerEnd = (UntilX - FromX) - 2;
            while (charPointer < alertText.Length)
            {
                Console.SetCursorPosition(cursorX, ++lineNumber);
                if (charPointerEnd > alertText.Length)
                {                    
                    Console.WriteLine(" " + alertText.Substring(charPointer).PadRight(UntilX - (FromX + 2)) + " ");
                }
                else
                {
                    Console.WriteLine(" " + alertText.Substring(charPointer, charPointerEnd) + " ");
                }
                
                charPointer = charPointerEnd;
                charPointerEnd = charPointer + charPointerEnd;
            }

            //footer
            PrintLinePanelText("  ", cursorX, ++lineNumber, UntilX - FromX);
            PrintLinePanelText(" Press Enter to close alert.", cursorX, ++lineNumber, UntilX - FromX);
            //Console.SetCursorPosition(cursorX, ++lineNumber);
            //Console.Write(" ".PadRight(UntilX - FromX));
            //Console.SetCursorPosition(cursorX, ++lineNumber);
            //Console.Write(" Press Enter to close alert.".PadRight(UntilX - FromX));

            Console.SetCursorPosition(cursorX + 29, lineNumber);
            Console.ReadLine();

            Console.BackgroundColor = ConsoleColor.Black;
        }
    }
}
