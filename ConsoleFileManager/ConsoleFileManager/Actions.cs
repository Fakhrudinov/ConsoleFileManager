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

        /// <summary>
        /// user press arrows up/down, pgUp pgDown, Home or End
        /// </summary>
        /// <param name="sizeOfChange"></param>
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
        /// <summary>
        /// F9
        /// </summary>
        /// <param name="actionName"></param>
        internal void RenameItem(string actionName)
        {
            if (CurrentItem != 0) // not a parent Dir
            {
                string newName = AskUserForNewName(actionName);

                if (newName.Length > 0)
                {
                    string sourceItem = Path.Combine(StartDirectory, CurrentItemName);
                    string targetItem = Path.Combine(StartDirectory, newName);

                    MoveOrRename(sourceItem, targetItem);
                    ClassLibrary.Do.WriteCommandToFile($"name {sourceItem}, {targetItem}");
                }
            }
        }

        /// <summary>
        /// F6
        /// </summary>
        /// <param name="targetDirectory"></param>
        internal void MoveItemTo(string targetDirectory)
        {
            if (CurrentItem != 0) // not a parent Dir
            {
                string sourceItem = Path.Combine(StartDirectory, CurrentItemName);
                string targetItem = Path.Combine(targetDirectory, CurrentItemName);

                MoveOrRename(sourceItem, targetItem);

                ClassLibrary.Do.WriteCommandToFile($"mv {sourceItem}, {targetItem}");
            }
        }
        
        /// <summary>
        /// user action 'name' and 'mv'
        /// </summary>
        /// <param name="sourceItem"></param>
        /// <param name="targetItem"></param>
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
                    ClassLibrary.Do.ShowAlert("Move Or Rename - File - " + f.Message, UntilX - FromX);
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
                    ClassLibrary.Do.ShowAlert("Move Or Rename - Directory - " + d.Message, UntilX - FromX);
                }
            }
        }

        /// <summary>
        /// F7
        /// </summary>
        /// <param name="actionName"></param>
        internal void CreateNewDir(string actionName)
        {
            string newName = AskUserForNewName(actionName);

            if (newName.Length > 0)
            {
                MkDir(newName);
            }

            ClassLibrary.Do.WriteCommandToFile($"mkdir {newName}");
        }

        /// <summary>
        /// user command 'mkdir'
        /// </summary>
        /// <param name="newName"></param>
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
                    ClassLibrary.Do.ShowAlert(d.Message, UntilX - FromX);
                }
            }
            else
            {
                ClassLibrary.Do.ShowAlert($"Make New Directory - Directory '{newDir}' already exist!", UntilX - FromX);
            }
        }

        /// <summary>
        /// F8 or 'rm'
        /// </summary>
        /// <param name="itemPath"></param>
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
                ClassLibrary.Do.ShowAlert("Delete - " + d.Message, UntilX - FromX);
            }

            ClassLibrary.Do.WriteCommandToFile($"rm {itemPath}");
        }   

        /// <summary>
        /// enter pressed on object in panel
        /// </summary>
        internal void ExecuteCurrent()
        {
            if (CurrentItem != 0) // not a parent Dir
            {
                string itemToExe = Path.Combine(StartDirectory, CurrentItemName);
                DirectoryInfo dir = new DirectoryInfo(itemToExe);

                ClassLibrary.Do.WriteCommandToFile("run " + itemToExe);

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
                    ClassLibrary.Do.WriteCommandToFile("cd " + dir.Parent.FullName);
                }
                else
                {
                    ShowChangeDisk();
                }

                Active.CurrentItem = 0;
                Active.ShowDirectoryContent();
            }
        }

        /// <summary>
        /// run current object
        /// </summary>
        /// <param name="itemToExe"></param>
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
                ClassLibrary.Do.ShowAlert("Try to execute file - " + ex.Message, UntilX - FromX);
            }
        }

        /// <summary>
        /// F5 copy 
        /// </summary>
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

                ClassLibrary.Do.WriteCommandToFile($"cp {sourceItem}, {targetItem}");
            }
        }


        /// <summary>
        /// change disk dialog
        /// </summary>
        private void ShowChangeDisk()
        {         
            int selected = 0;
            DriveInfo[] drives = DriveInfo.GetDrives();
            string[] lines = new string[drives.Length];
            for (int i = 0; i < drives.Length; i++)
            {
                lines[i] = $"{drives[i].Name} {drives[i].DriveType.ToString().PadRight(10)} {drives[i].VolumeLabel}";

                if (drives[i].Name.ToLower().Contains(StartDirectory.ToLower().Substring(0, 2)))
                {
                    selected = i;
                }
            }

            string choise =  SelectDialogFromArray(lines, selected, "Change Disk");
            
            //change drive
            choise = choise.Substring(0, choise.IndexOf(Path.DirectorySeparatorChar) + 1);            
            Active.StartDirectory = choise;
            ClassLibrary.Do.WriteCommandToFile("cd " + choise);
            Active.CurrentItem = 0;
        }

        /// <summary>
        /// user select new disk or command from hystory, navigate with arrows
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="selected"></param>
        /// <param name="dialogName"></param>
        /// <returns></returns>
        private string SelectDialogFromArray(string[] lines, int selected, string dialogName)
        {
            bool quit = false;
            while (quit == false)
            {
                int lineNumber = (Height - lines.Length) / 2;
                int xCursor = 2;
                int totalLenght = Width - 4;

                Console.BackgroundColor = ConsoleColor.DarkBlue;

                //header
                ClassLibrary.Do.PrintDialogHeader(dialogName, xCursor, lineNumber, totalLenght);

                for (int i = 0; i < lines.Length; i++)
                {
                    if (selected == i)
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                    else
                        Console.BackgroundColor = ConsoleColor.DarkBlue;

                    string line = lines[i];
                    if (lines[i].Length > totalLenght - 2)
                        line = lines[i].Substring(0, totalLenght - 2);

                    ClassLibrary.Do.PrintLinePanelText(" " + line, xCursor, ++lineNumber, totalLenght);
                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                }

                ClassLibrary.Do.PrintLinePanelText(" Press UpArrow or DownArrow to select line", xCursor, ++lineNumber, totalLenght);
                ClassLibrary.Do.PrintLinePanelText(" Enter to choise.", xCursor, ++lineNumber, totalLenght);
                Console.BackgroundColor = ConsoleColor.Black;

                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                switch (keyInfo.Key)
                {
                    case ConsoleKey.Enter:
                        quit = true;
                        break;
                    case ConsoleKey.UpArrow:
                        if (selected > 0)
                            selected--;
                        break;
                    case ConsoleKey.DownArrow:
                        if (selected < lines.Length - 1)
                            selected++;
                        break;
                }
            }

            return lines[selected];
        }

        /// <summary>
        /// Show content from file commandHystory.log. Selected line copy to command line
        /// </summary>
        /// <returns></returns>
        internal string GetCommandsHystory()
        {
            //read file
            string fileName = ".\\commandHystory.log";
            string[] lines = File.ReadAllLines(fileName);

            //last command selected
            int selected = lines.Length - 1;

            string choise = SelectDialogFromArray(lines, selected, "Commands hystory");
            return choise;
        }

        /// <summary>
        /// Copy - user action 'cp'
        /// </summary>
        internal void CopyFromCommandLine()
        {
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

        /// <summary>
        /// CopyDir
        /// </summary>
        /// <param name="sourceItem"></param>
        /// <param name="targetItem"></param>
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
                    ClassLibrary.Do.ShowAlert(" Creation Directory Error: " + ex.Message, UntilX - FromX);
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
                        ClassLibrary.Do.ShowAlert(" Creation Directory Error: " + ex.Message, UntilX - FromX);
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
                    ClassLibrary.Do.ShowAlert(" File copy error: " + ex.Message, UntilX - FromX);
                }                
            }
        }

        /// <summary>
        /// CopyFile
        /// </summary>
        /// <param name="sourceItem"></param>
        /// <param name="targetItem"></param>
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
                ClassLibrary.Do.ShowAlert("Creation Directory Error: " + ex.Message, UntilX - FromX);
            }

            try
            {                
                File.Copy(sourceItem, targetItem, true);
            }
            catch (Exception ex)
            {
                ClassLibrary.Do.ShowAlert(" File copy error: " + ex.Message, UntilX - FromX);
            }
        }

        /// <summary>
        /// F3 show info about object
        /// </summary>
        /// <param name="itemPath"></param>
        internal void ShowInfo()
        {
            int lineNumber = Height / 4;            
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            int totalLenght = 35; // ( Last writed: 14.04.2021 20:32:13 ).lenght
            int xCursor = (Width - totalLenght) / 2;         

            if (Active.CurrentItem != 0) // not a parent dir
            {
                //header
                ClassLibrary.Do.PrintDialogHeader("Information", xCursor, lineNumber, totalLenght);

                DirectoryInfo dirInfo = new DirectoryInfo(Path.Combine(Active.StartDirectory, Active.CurrentItemName));                 
                if (!dirInfo.Attributes.ToString().Contains("Directory")) //  files
                {
                    FileInfo fileInf = new FileInfo(Path.Combine(Active.StartDirectory, Active.CurrentItemName));
                    ClassLibrary.Do.PrintLinePanelText( "        File", xCursor, ++lineNumber, totalLenght);
                    ClassLibrary.Do.PrintLinePanelText($"        Name: {fileInf.Name}", xCursor, ++lineNumber, totalLenght);
                    ClassLibrary.Do.PrintLinePanelText($"     Created: {fileInf.CreationTime}", xCursor, ++lineNumber, totalLenght);
                    ClassLibrary.Do.PrintLinePanelText($" Last writed: {fileInf.LastWriteTime}", xCursor, ++lineNumber, totalLenght);
                    ClassLibrary.Do.PrintLinePanelText($"    ReadOnly: {fileInf.IsReadOnly}", xCursor, ++lineNumber, totalLenght);
                    ClassLibrary.Do.PrintLinePanelText($" Size, bytes: {fileInf.Length}", xCursor, ++lineNumber, totalLenght);

                    if (fileInf.Attributes.ToString().Contains(','))
                    {
                        string[] attr = fileInf.Attributes.ToString().Split(',');
                        lineNumber = ShowAttributes(attr, xCursor, lineNumber);
                    }
                    else
                        ClassLibrary.Do.PrintLinePanelText($"  Attributes: {fileInf.Attributes}", xCursor, ++lineNumber, totalLenght);
                }
                else // dirs
                {
                    ClassLibrary.Do.PrintLinePanelText( "   Directory", xCursor, ++lineNumber, totalLenght);
                    ClassLibrary.Do.PrintLinePanelText($"        Name: {dirInfo.Name}", xCursor, ++lineNumber, totalLenght);
                    ClassLibrary.Do.PrintLinePanelText($"     Created: {dirInfo.CreationTime}", xCursor, ++lineNumber, totalLenght);
                    ClassLibrary.Do.PrintLinePanelText($" Last writed: {dirInfo.LastWriteTime}", xCursor, ++lineNumber, totalLenght);

                    if (dirInfo.Attributes.ToString().Contains(','))
                    {
                        string[] attr = dirInfo.Attributes.ToString().Split(',');
                        lineNumber = ShowAttributes(attr, xCursor, lineNumber);
                    }
                    else
                        ClassLibrary.Do.PrintLinePanelText($"  Attributes: {dirInfo.Attributes}", xCursor, ++lineNumber, totalLenght);
                }
            }
            else
            {
                totalLenght = 39;
                //header
                ClassLibrary.Do.PrintDialogHeader("Information", xCursor, lineNumber, totalLenght);

                ClassLibrary.Do.PrintLinePanelText(" Parent directory info not avaliable.", xCursor, ++lineNumber, totalLenght);
                ClassLibrary.Do.PrintLinePanelText(" Firstly go there and select directory", xCursor, ++lineNumber, totalLenght);
            }

            //footer
            ClassLibrary.Do.PrintLinePanelText(" ", xCursor, ++lineNumber, totalLenght);
            ClassLibrary.Do.PrintLinePanelText(" Press Enter to close panel.", xCursor, ++lineNumber, totalLenght);
            ClassLibrary.Do.SetCursorPosition(xCursor + 28, lineNumber);
            Console.ReadLine();

            Console.BackgroundColor = ConsoleColor.Black;
        }

        /// <summary>
        /// part of F3 panel
        /// </summary>
        /// <param name="attr"></param>
        /// <param name="xCursor"></param>
        /// <param name="lineNumber"></param>
        /// <returns></returns>
        private int ShowAttributes(string[] attr, int xCursor, int lineNumber)
        {
            ClassLibrary.Do.PrintLinePanelText($"  Attributes: {attr[0]}", xCursor, ++lineNumber, xCursor * 2);

            for (int i = 1; i < attr.Length; i++)
            {
                ClassLibrary.Do.PrintLinePanelText($"             {attr[i]}", xCursor, ++lineNumber, xCursor * 2);
            }

            return lineNumber;
        }

        /// <summary>
        /// show help panel with all avaliable actions
        /// </summary>
        internal void ShowHelp()
        {
            int lineNumber = (Height - 25) / 2;
            string longest = " arrows Up and Down, PgUp PgDown, Home End - navigate inside panel"; // longest text lenght
            int xCursor = (UntilX - FromX) - (longest.Length / 2);
            int totalLenght = longest.Length + 1;
            Console.BackgroundColor = ConsoleColor.DarkBlue;

            //header
            ClassLibrary.Do.PrintDialogHeader("Help", xCursor, lineNumber, totalLenght);

            ClassLibrary.Do.PrintLinePanelText(" F1 - Help Panel", xCursor, ++lineNumber, totalLenght);
            ClassLibrary.Do.PrintLinePanelText(" F3 - File or Directory Information", xCursor, ++lineNumber, totalLenght);
            ClassLibrary.Do.PrintLinePanelText(" F5 - Copy File or Directory", xCursor, ++lineNumber, totalLenght);
            ClassLibrary.Do.PrintLinePanelText(" F6 - Move File or Directory", xCursor, ++lineNumber, totalLenght);
            ClassLibrary.Do.PrintLinePanelText(" F7 - Create new Directory", xCursor, ++lineNumber, totalLenght);
            ClassLibrary.Do.PrintLinePanelText(" F8 - Delete File or Directory", xCursor, ++lineNumber, totalLenght);
            ClassLibrary.Do.PrintLinePanelText(" F9 - Rename File or Directory", xCursor, ++lineNumber, totalLenght);
            ClassLibrary.Do.PrintLinePanelText(longest, xCursor, ++lineNumber, totalLenght);
            ClassLibrary.Do.PrintLinePanelText(" Tab - switch between panels", xCursor, ++lineNumber, totalLenght);
            ClassLibrary.Do.PrintLinePanelText(" ", xCursor, ++lineNumber, totalLenght);
            ClassLibrary.Do.PrintLinePanelText(" Manual commands, commands - case insensitive.", xCursor, ++lineNumber, totalLenght);
            ClassLibrary.Do.PrintLinePanelText(" (Ctrl + Enter): Copy current element to command line", xCursor, ++lineNumber, totalLenght);
            ClassLibrary.Do.PrintLinePanelText(" (Ctrl + E): Command history select dialog", xCursor, ++lineNumber, totalLenght);
            ClassLibrary.Do.PrintLinePanelText(" Set passive panel same as active: equal", xCursor, ++lineNumber, totalLenght);
            ClassLibrary.Do.PrintLinePanelText(" Change directory: cd NewPath ", xCursor, ++lineNumber, totalLenght);
            ClassLibrary.Do.PrintLinePanelText(" Copy: cp sourcePath (to passive panel)", xCursor, ++lineNumber, totalLenght);
            ClassLibrary.Do.PrintLinePanelText(" Move: mv sourcePath (to passive panel)", xCursor, ++lineNumber, totalLenght);
            ClassLibrary.Do.PrintLinePanelText(" Remove: rm sourcePath", xCursor, ++lineNumber, totalLenght);
            ClassLibrary.Do.PrintLinePanelText(" New Directory: mkdir newName", xCursor, ++lineNumber, totalLenght);
            ClassLibrary.Do.PrintLinePanelText(" Execute File: run FileName", xCursor, ++lineNumber, totalLenght);
            ClassLibrary.Do.PrintLinePanelText(" Rename: name sourcePath, newName", xCursor, ++lineNumber, totalLenght);
            ClassLibrary.Do.PrintLinePanelText(" ", xCursor, ++lineNumber, totalLenght);
            ClassLibrary.Do.PrintLinePanelText(" Press Enter to close panel.", xCursor, ++lineNumber, totalLenght);

            ClassLibrary.Do.SetCursorPosition(xCursor + 28, lineNumber);
            Console.ReadLine();

            Console.BackgroundColor = ConsoleColor.Black;
        }

        /// <summary>
        /// panel where user choose Y to confirm action
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="targetDirectory"></param>
        /// <returns></returns>
        internal bool UserConfirmAction(string actionName, string targetDirectory)
        {
            bool isConfirm = false;
            if (CurrentItem != 0) // not a parent Dir
            {
                int totalLenght;
                int lineNumber = Height / 3;               

                string text = " Press Y to confirm, any other to decline, then Enter";
                string source = $" {actionName} {CurrentItemName} ";                
                if (actionName.Equals("Move") || actionName.Equals("Copy"))
                {
                    totalLenght = GetLongestInt(" to " + targetDirectory, text, source);
                }
                else
                {
                    totalLenght = GetLongestInt(text, source);
                }

                if(totalLenght > Width - 4)
                {
                    totalLenght = Width - 4;
                    source = ClassLibrary.Do.TextLineCutter(source, Width - (4 + 6)); // 6 is copy or move lenght
                    targetDirectory = ClassLibrary.Do.TextLineCutter(targetDirectory, Width - (4 + 4));
                }


                int xCursor = ((Width - totalLenght) / 2);

                Console.BackgroundColor = ConsoleColor.DarkRed;

                //header
                ClassLibrary.Do.PrintDialogHeader(actionName, xCursor, lineNumber, totalLenght);

                ClassLibrary.Do.PrintLinePanelText(source, xCursor, ++lineNumber, totalLenght);
                if (actionName.Equals("Move") || actionName.Equals("Copy"))
                {
                    ClassLibrary.Do.PrintLinePanelText(" to " + targetDirectory, xCursor, ++lineNumber, totalLenght);
                }

                if(text.Length > totalLenght)
                {
                    ClassLibrary.Do.PrintLinePanelText(" Press Y to confirm, any other", xCursor, ++lineNumber, totalLenght);
                    ClassLibrary.Do.PrintLinePanelText(" to decline, then Enter", xCursor, ++lineNumber, totalLenght);
                }
                else
                    ClassLibrary.Do.PrintLinePanelText(text, xCursor, ++lineNumber, totalLenght);

                ClassLibrary.Do.PrintLinePanelText(" Y ? ", xCursor, ++lineNumber, totalLenght);

                ClassLibrary.Do.SetCursorPosition(xCursor + 6, lineNumber);
                string decision = Console.ReadLine();

                if (decision.ToLower().Equals("y"))
                    isConfirm = true;

                Console.BackgroundColor = ConsoleColor.Black;
            }

            return isConfirm;
        }

        /// <summary>
        /// for correct size - compare string for longest
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
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

        /// <summary>
        /// recognize inputed command
        /// </summary>
        /// <param name="newCommandText"></param>
        /// <param name="execute"></param>
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
                    ClassLibrary.Do.ShowAlert("Try to execute command line - Unknown command or not correct arguments", UntilX - FromX);
                }
                Info = "";
            }

            PrintCommandInfo();
        }

        /// <summary>
        /// print tips or error for command line
        /// </summary>
        private void PrintCommandInfo()
        {
            ClassLibrary.Do.SetCursorPosition(1, Height - 4);
            string infoOnConsole = ClassLibrary.Do.TextLineCutter(Info, Width - 2);
            Console.Write(infoOnConsole);
        }

        /// <summary>
        /// execute current command. current command setted in CommandController
        /// </summary>
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
                    ClassLibrary.Do.ShowAlert("Execute User Command - Try to get directory - " + e.Message, UntilX - FromX);

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

        /// <summary>
        /// set current command and arguments for command
        /// </summary>
        /// <param name="type"></param>
        /// <param name="arguments"></param>
        private void CommandController(Command type, string arguments)
        {
            switch (type)
            {
                case Command.EqualisePanels:
                    CanBeExecute = true;
                    CommandType = (int)Command.EqualisePanels;
                    break;

                case Command.MakeNewDir:
                    bool checkArguments = CheckExist(arguments, false, true);
                    if (checkArguments)
                    {
                        CanBeExecute = true;
                        CommandType = (int)Command.MakeNewDir;
                        ArgumentSource = arguments.Substring(1);
                    }
                    break;

                case Command.ChangeDir:
                    checkArguments = CheckExist(arguments, true, true);
                    if (checkArguments)
                    {
                        CanBeExecute = true;
                        CommandType = (int)Command.ChangeDir;
                        arguments = " " + CheckParentRelativePath(arguments.Substring(1), true);
                        ArgumentSource = GetCorrectPath(arguments.Substring(1));
                        ArgumentSource = ItIsDirectory(ArgumentSource); // files not allowed here. Cut path to directory
                    }
                    break;

                case Command.RunFile:
                    checkArguments = CheckExist(arguments, true, true);
                    if (checkArguments)
                    {
                        CanBeExecute = true;
                        CommandType = (int)Command.RunFile;
                        ArgumentSource = arguments.Substring(1);
                    }
                    break;

                case Command.Remove:
                    checkArguments = CheckExist(arguments, true, true);
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

        /// <summary>
        /// if it is file - remove file name from path
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        private string ItIsDirectory(string fullPath)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(fullPath);
            if (!dirInfo.Attributes.ToString().Contains("Directory")) //  files.
            {
                fullPath = fullPath.Substring(0, fullPath.LastIndexOf('\\'));
            }
            
            return fullPath;
        }

        /// <summary>
        /// Checking pair of path. First path always must be exist. Second path - existanse depends of params
        /// </summary>
        /// <param name="arguments"></param> // path, full, first and second in one string. delimeter is ','
        /// <param name="secondRequired"></param> // must be entered
        /// <param name="secondMustExist"></param> // nececcary or not to input second path
        /// <returns></returns>
        private bool CheckPairExist(string arguments, bool secondRequired, bool secondMustExist)
        {
            bool checkFirstArguments = false;
            bool checkSecondArguments = false;

            //first is always must be exist
            if (!arguments.Contains(','))
                checkFirstArguments = CheckExist(arguments, true, true);
            else
            {
                CanBeExecute = false;
                string arg1 = arguments.Substring(0, arguments.IndexOf(','));
                checkFirstArguments = CheckExist(arg1, true, true);

                //without correct first argument - second is useless
                if (checkFirstArguments == true)
                {
                    if (arguments.Substring(arguments.IndexOf(',')).Length > 1)
                    {
                        // for rename we check in active panel. else in passive
                        if (Info.Contains("Rename"))
                        {
                            checkSecondArguments = CheckExist(arguments.Substring(arguments.IndexOf(',') + 1), secondMustExist, true);
                        }
                        else
                        {
                            checkSecondArguments = CheckExist(arguments.Substring(arguments.IndexOf(',') + 1), secondMustExist, false);
                        }                        

                        if (checkSecondArguments == true)// OK both correct
                        {
                            //set path from first argument
                            ArgumentSource = GetCorrectPath(arguments.Substring(1, arguments.IndexOf(',') - 1));

                            //set path from second argument
                            //if it is RENAME - we need to use active panel current directory. Else - passive current directory
                            if (Info.Contains("Rename"))
                            {
                                ArgumentTarget = GetNonExistPath(arguments.Substring(arguments.IndexOf(',') + 2), true);
                            }
                            else
                            {
                                ArgumentTarget = GetNonExistPath(arguments.Substring(arguments.IndexOf(',') + 2), false);
                            } 
                            
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

        /// <summary>
        /// Checking path in case, when path must be NOT exist. inActivePanel set which panel must be used
        /// </summary>
        /// <param name="path"></param>
        /// <param name="inActivePanel"></param>
        /// <returns></returns>
        private string GetNonExistPath(string path, bool inActivePanel)
        {
            if (path.Contains('\\')) // full path
            {
               //check exist - for full path - must be NOT exist
               if (!CheckIsItFullPath(path))
               {
                    //check exist - for first part before last '\' - must be exist - root dir for new item
                    if (!CheckIsItFullPath(path.Substring(0, path.LastIndexOf('\\'))))
                    {
                        if (inActivePanel) // Rename
                        {
                            path = Path.Combine(Active.StartDirectory, path);
                        }
                        else // copy move
                        {
                            path = Path.Combine(Passive.StartDirectory, path);
                        }
                    }                     
               }

                return path;
            }
            else // just name
            {
                if (inActivePanel) // Rename
                {
                    path = Path.Combine(Active.StartDirectory, path);
                }
                else // copy move
                {
                    path = Path.Combine(Passive.StartDirectory, path);
                }
            }
            return path;
        }

        /// <summary>
        /// check path - return 'as is' if exist or combine with current directory
        /// </summary>
        /// <param name="pathOrItem"></param>
        /// <returns></returns>
        private string GetCorrectPath(string pathOrItem)
        {
            bool exist = CheckIsItFullPath(pathOrItem);
            if (exist)
                return pathOrItem;
            else
                return Path.Combine(Active.StartDirectory, pathOrItem);
        }

        /// <summary>
        /// check existance of path
        /// </summary>
        /// <param name="sourceItem"></param>
        /// <returns></returns>
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
        /// Check for one argument. expected or not. isActivePanel - where to check
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="expected"></param>
        /// <param name="isActivePanel"></param>
        /// <returns></returns>
        private bool CheckExist(string arguments, bool expected, bool isActivePanel)
        {
            // firstly check - first symbol must be ' ' as delimeter from command and argument
            if (arguments[0].Equals(' ')) // ok it is space
            {
                bool actual = false;

                string sourceItem = arguments.Substring(1);
                if (sourceItem.Length > 0)
                {
                    if (sourceItem.Contains("...")) // wrong path
                    {
                        Info = $"Directory or file '{arguments.Substring(1)}' not found!";
                        return false;
                    }

                    sourceItem = CheckParentRelativePath(sourceItem, isActivePanel);

                    // firsly check if it is direct path to file or dir
                    actual = CheckIsItFullPath(sourceItem);

                    // if not - check this item in current folder
                    if (!actual)
                    {
                        sourceItem = Path.Combine(Active.StartDirectory, sourceItem).ToLower();
                        if (!isActivePanel)// case os rename
                            sourceItem = Path.Combine(Passive.StartDirectory, sourceItem).ToLower();

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

        /// <summary>
        /// get absolute path from relative parent path. aisActivePanel - where to check path
        /// </summary>
        /// <param name="sourceItem"></param>
        /// <param name="isActivePanel"></param>
        /// <returns></returns>
        private string CheckParentRelativePath(string sourceItem, bool isActivePanel)
        {
            // ..                   = to parent
            // ..\..                = to parent \ to parent
            // ..\..\dirname        = to parent \ to parent \ to dir
            // .                    = the current directory - remove
            // .\..\dirname         = current \ parent \ dir

            bool parent = true;

            while (parent)
            {
                if (sourceItem.Contains(".."))
                {
                    string head = sourceItem.Substring(0, sourceItem.IndexOf(".."));
                    string tale = sourceItem.Substring(sourceItem.IndexOf("..") + 2);

                    if (head.Length > 0 && head[0].Equals('\\'))
                        head = head.Substring(1);

                    //first - try to get parent
                    if (head.Length == 0) // if at beginning of path
                    {
                        DirectoryInfo dir = new DirectoryInfo(Active.StartDirectory);
                        if(!isActivePanel)// case os rename
                            dir = new DirectoryInfo(Passive.StartDirectory);

                        if (dir.Parent != null && dir.Parent.Exists)
                        {
                            sourceItem = dir.Parent.FullName.ToString();
                        }
                    }
                    else // middle of string path 
                    {
                        sourceItem = Path.Combine(Active.StartDirectory, head);
                        if (!isActivePanel)// case os rename
                            sourceItem = Path.Combine(Passive.StartDirectory, head);

                        DirectoryInfo dir = new DirectoryInfo(sourceItem);
                        if (dir.Parent != null && dir.Parent.Exists)
                        {
                            sourceItem = dir.Parent.FullName.ToString();
                        }
                    }

                    //secong = glue tale if exist
                    if (tale.Length > 0)
                    {
                        sourceItem = sourceItem + tale;
                    }
                }
                else if (sourceItem.Length > 1 && sourceItem.Substring(0, 1).Equals("."))
                {
                    sourceItem = sourceItem.Substring(1);
                }
                else if (sourceItem.Length > 2 && sourceItem.Substring(0, 2).Equals("\\."))
                {
                    sourceItem = sourceItem.Substring(2);
                }

                if (!sourceItem.Contains("..")) // exit from while
                    parent = false;
            }

            return sourceItem;
        }

        /// <summary>
        /// panel when user input new name
        /// </summary>
        /// <param name="actionName"></param>
        /// <returns></returns>
        private string AskUserForNewName(string actionName)
        {
            int xCursor = (UntilX - FromX) / 2;
            int lineNumber = Height / 3;
            string requestText = " Enter new name, or leave it empty and press Enter:";
            int totalLenght = requestText.Length + 1;
            Console.BackgroundColor = ConsoleColor.Blue;

            //header
            

            if (Width - 4 < totalLenght) // shorten text
            {
                totalLenght = Width - 4;
                xCursor = 2;
                ClassLibrary.Do.PrintDialogHeader(actionName, xCursor, lineNumber, totalLenght);
                ClassLibrary.Do.PrintLinePanelText(" Enter new name, or leave ", xCursor, ++lineNumber, totalLenght);
                ClassLibrary.Do.PrintLinePanelText(" it empty and press Enter:", xCursor, ++lineNumber, totalLenght);
            }
            else // normal lenght
            {
                ClassLibrary.Do.PrintDialogHeader(actionName, xCursor, lineNumber, totalLenght);
                ClassLibrary.Do.PrintLinePanelText(requestText, xCursor, ++lineNumber, totalLenght);
            }

            ClassLibrary.Do.PrintLinePanelText(" ", xCursor, ++lineNumber, totalLenght);

            ClassLibrary.Do.SetCursorPosition(xCursor + 1, lineNumber);
            string newName = Console.ReadLine();

            Console.BackgroundColor = ConsoleColor.Black;

            return newName;
        }
    }
}
