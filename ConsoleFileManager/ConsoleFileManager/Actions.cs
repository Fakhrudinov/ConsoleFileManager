using ReportingLib;
using ReportingLib.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConsoleFileManager
{
    internal class Actions
    {
        private enum Command
        {
            ChangeDir,      // 0
            Copy,           // 1
            MakeNewDir,     // 2
            Move,           // 3
            Remove,         // 4
            RunFile,        // 5
            EqualisePanels, // 6
            Rename,         // 7
            Find,           // 8
            Unknown
        }

        private string _info { get; set; }

        private bool _canBeExecute { get; set; }
        private int _commandType { get; set; }
        private string _argumentSource { get; set; }
        private string _argumentTarget { get; set; }

        private FilePanel _active { get; set; }
        private FilePanel _passive { get; set; }

        private int _currentItem { get; set; }
        private string _currentItemName { get; set; }
        private int _fromX { get; set; }
        private int _untilX { get; set; }
        private int _height { get; set; }
        private int _width { get; set; }
        private string _startDirectory { get; set; }

        internal Actions(FilePanel activePanel, FilePanel passivePanel, int width)
        {
            _active = activePanel;
            _passive = passivePanel;
            _startDirectory = _active.StartDirectory;
            _currentItem = _active.CurrentItem;
            _currentItemName = _active.CurrentItemName;
            _fromX = _active.FromX;
            _untilX = _active.UntilX;
            _height = _active.PanelHeight;
            _width = width;

            _canBeExecute = false;
            _commandType = (int)Command.Unknown;
        }

        /// <summary>
        /// user press arrows up/down, pgUp pgDown, Home or End
        /// </summary>
        /// <param name="sizeOfChange"></param>
        internal void ChangeCurrentItem(int sizeOfChange)
        {
            if (sizeOfChange == 100)// page down
            {
                sizeOfChange = _active.ItemsOnPage;
            }
            else if (sizeOfChange == -100)// page up
            {
                sizeOfChange = _active.ItemsOnPage * -1;
            }

            _active.CurrentItem += sizeOfChange; // up & down arrow

            if (sizeOfChange == 1000) // End
            {
                _active.CurrentItem = _active.TotalItems - 1;
            }
            else if (sizeOfChange == -1000) // Home
            {
                _active.CurrentItem = 0;
            }

            if (_active.CurrentItem < 0)
            {
                _active.CurrentItem = 0;
            }

            if (_active.CurrentItem > _active.TotalItems - 1)
            {
                _active.CurrentItem = _active.TotalItems - 1;
            }

            _active.ShowDirectoryContent();
        }
        /// <summary>
        /// F9
        /// </summary>
        /// <param name="actionName"></param>
        internal void RenameItem(string actionName)
        {
            if (_currentItem != 0) // not a parent Dir
            {
                string newName = AskUserForNewName(actionName);

                if (newName.Length > 0)
                {
                    string sourceItem = Path.Combine(_startDirectory, _currentItemName);
                    string targetItem = Path.Combine(_startDirectory, newName);

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
            if (_currentItem != 0) // not a parent Dir
            {
                string sourceItem = Path.Combine(_startDirectory, _currentItemName);
                string targetItem = Path.Combine(targetDirectory, _currentItemName);

                MoveOrRename(sourceItem, targetItem);

                ClassLibrary.Do.WriteCommandToFile($"mv {sourceItem}, {targetItem}");
            }
        }

        /// <summary>
        /// user action 'name' and 'mv'
        /// </summary>
        /// <param name="sourceItem"></param>
        /// <param name="targetItem"></param>
        internal void MoveOrRename(string sourceItem, string targetItem)
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
                    ClassLibrary.Do.ShowAlert("Move Or Rename - File - " + f.Message, _untilX - _fromX);
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
                    ClassLibrary.Do.ShowAlert("Move Or Rename - Directory - " + d.Message, _untilX - _fromX);
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
        internal void MkDir(string newName)
        {
            string newDir = Path.Combine(_startDirectory, newName);
            DirectoryInfo dirTarget = new DirectoryInfo(newDir);

            if (!dirTarget.Exists)
            {
                try
                {
                    Directory.CreateDirectory(newDir);
                }
                catch (Exception d)
                {
                    ClassLibrary.Do.ShowAlert(d.Message, _untilX - _fromX);
                }
            }
            else
            {
                ClassLibrary.Do.ShowAlert($"Make New Directory - Directory '{newDir}' already exist!", _untilX - _fromX);
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

                if (_active.CurrentItem > 0)
                    _active.CurrentItem--;
                else
                    _active.CurrentItem = 0;
            }
            catch (Exception d)
            {
                ClassLibrary.Do.ShowAlert("Delete - " + d.Message, _untilX - _fromX);
            }

            ClassLibrary.Do.WriteCommandToFile($"rm {itemPath}");
        }

        /// <summary>
        /// enter pressed on object in panel
        /// </summary>
        internal void ExecuteCurrent()
        {
            if (_currentItem != 0) // not a parent Dir
            {
                string itemToExe = Path.Combine(_startDirectory, _currentItemName);
                DirectoryInfo dir = new DirectoryInfo(itemToExe);

                ClassLibrary.Do.WriteCommandToFile("run " + itemToExe);

                if (!dir.Attributes.ToString().Contains("Directory")) // file 
                {
                    RunFile(itemToExe);
                }
                else // dir
                {
                    _active.StartDirectory = itemToExe;
                    _active.CurrentItem = 0;
                    _active.ShowDirectoryContent();
                }
            }
            else // go to parent dir
            {
                DirectoryInfo dir = new DirectoryInfo(_startDirectory);
                if (dir.Parent != null && dir.Parent.Exists) // normal directory
                {
                    _active.StartDirectory = dir.Parent.FullName.ToString();
                    ClassLibrary.Do.WriteCommandToFile("cd " + dir.Parent.FullName);
                }
                else
                {
                    ShowChangeDisk();
                }

                _active.CurrentItem = 0;
                _active.ShowDirectoryContent();
            }
        }

        /// <summary>
        /// Show dialog when user input mask for new search
        /// </summary>
        /// <returns>mask</returns>
        internal string GetNameForMaskedSearh()
        {
            return GetNameFromUser("Find by mask", "Enter mask for search. '*' for a sequence of any characters, '?' to replace one character in the search. Leave empty for cancel request.");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="newSearch"></param>
        internal void ShowFindedItems(string newSearch)
        {
            var findedItems = new List<String>();
            DirectoryInfo currentDirectory = new DirectoryInfo(_active.StartDirectory);

            foreach (DirectoryInfo dir in currentDirectory.GetDirectories(newSearch))
            {
                findedItems.Add(dir.Name);
            }

            foreach (FileInfo file in currentDirectory.GetFiles(newSearch))
            {
                findedItems.Add(file.Name);
            }
            
            if(findedItems.Count == 0)
            {
                //show alert
                List<string> helpText = new List<string>()
            {
                "Search results",//header
                "Nothing found for your search:",
                newSearch,
                "Press Enter to close panel."//footer
            };

                PrintInfoPanel(helpText, ConsoleColor.DarkMagenta);
            }
            else
            {
                //show selector
                string choise = SelectDialogFromArray(findedItems.ToArray(), 0, "Search results");
                
                if (!choise.Equals("Select nothing"))
                {
                    //set up new current item
                    choise = Path.Combine(_active.StartDirectory, choise);//_active.StartDirectory

                    _active.CurrentItem = _active.AllItems.IndexOf(choise); ;
                    _active.ShowDirectoryContent();
                }
                //else do nothing
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
                ClassLibrary.Do.ShowAlert("Try to execute file - " + ex.Message, _untilX - _fromX);
            }
        }

        /// <summary>
        /// F5 copy 
        /// </summary>
        internal void CopyFromPanel()
        {
            string sourceItem = Path.Combine(_active.StartDirectory, _currentItemName);
            string targetItem = Path.Combine(_passive.StartDirectory, _currentItemName);

            if (_currentItem != 0) // not a parent Dir
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

                if (drives[i].Name.ToLower().Contains(_startDirectory.ToLower().Substring(0, 2)))
                {
                    selected = i;
                }
            }

            string choise =  SelectDialogFromArray(lines, selected, "Change Disk");
            
            //change drive
            choise = choise.Substring(0, choise.IndexOf(Path.DirectorySeparatorChar) + 1);            
            _active.StartDirectory = choise;
            ClassLibrary.Do.WriteCommandToFile("cd " + choise);
            _active.CurrentItem = 0;
        }

        /// <summary>
        /// user select new disk or command from hystory or masked search, navigate with arrows
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="selected"></param>
        /// <param name="dialogName"></param>
        /// <returns></returns>
        private string SelectDialogFromArray(string[] linesInitial, int selected, string dialogName)
        {
            string[] lines;
            bool showWarning = false;

            if (!dialogName.Equals("Search results"))
            {
                lines = linesInitial;
            }
            else
            {
                lines = new string[linesInitial.Length + 1];
                lines[0] = "Select nothing";
                Array.Copy(linesInitial, 0, lines, 1, linesInitial.Length);                
            }

            bool quit = false;
            while (quit == false)
            {
                //to future do - make pagination here.
                // now - just shrink array to fit on page                
                if (linesInitial.Length > _height - 11)
                {
                    lines = new string[_height - 10];
                    lines[0] = "Select nothing";
                    Array.Copy(linesInitial, 0, lines, 1, _height - 11);
                    showWarning = true;
                }

                int lineNumber = ((_height - lines.Length) / 2) - 2;//-2 for header(1string), and footer(2strings)
                int xCursor = 2;
                int totalLenght = _width - 4;

                Console.BackgroundColor = ConsoleColor.DarkBlue;

                //header
                Console.ForegroundColor = ConsoleColor.Yellow;
                ClassLibrary.Do.PrintDialogHeader(dialogName, xCursor, lineNumber, totalLenght);
                Console.ForegroundColor = ConsoleColor.Gray;

                //body
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

                //warning about shrinked result - remove when pagination being implemented!!!!!!
                if (showWarning)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    ClassLibrary.Do.PrintLinePanelText(" Other results is skipped. Specify you search.", xCursor, ++lineNumber, totalLenght);
                } 
                //footer
                Console.ForegroundColor = ConsoleColor.Yellow;
                ClassLibrary.Do.PrintDialogHeader(" Press UpArrow or DownArrow to select line", xCursor, ++lineNumber, totalLenght);
                ClassLibrary.Do.PrintDialogHeader(" Enter to choise.", xCursor, ++lineNumber, totalLenght);
                Console.ForegroundColor = ConsoleColor.Gray;
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
            DirectoryInfo dirInfo = new DirectoryInfo(_argumentSource);

            if (!dirInfo.Attributes.ToString().Contains("Directory")) // file copy
            {
                CopyFile(_argumentSource, _argumentTarget);
            }
            else // dir copy
            {
                CopyDir(_argumentSource, _argumentTarget);
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
                    ClassLibrary.Do.ShowAlert(" Creation Directory Error: " + ex.Message, _untilX - _fromX);
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
                        ClassLibrary.Do.ShowAlert(" Creation Directory Error: " + ex.Message, _untilX - _fromX);
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
                    ClassLibrary.Do.ShowAlert(" File copy error: " + ex.Message, _untilX - _fromX);
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
                ClassLibrary.Do.ShowAlert("Creation Directory Error: " + ex.Message, _untilX - _fromX);
            }

            try
            {                
                File.Copy(sourceItem, targetItem, true);
            }
            catch (Exception ex)
            {
                ClassLibrary.Do.ShowAlert(" File copy error: " + ex.Message, _untilX - _fromX);
            }
        }

        /// <summary>
        /// F3 show info about object
        /// </summary>
        /// <param name="itemPath"></param>
        internal void ShowInfo()
        {
            IReport report = new Report();
            List<string> infoText = new List<string>() { "Information" };

            if (_active.CurrentItem != 0) // not a parent dir
            {
                DirectoryInfo dirInfo = new DirectoryInfo(Path.Combine(_active.StartDirectory, _active.CurrentItemName));                 
                if (!dirInfo.Attributes.ToString().Contains("Directory")) //  files
                {
                    FileInfo fileInf = new FileInfo(Path.Combine(_active.StartDirectory, _active.CurrentItemName));

                    FileInfoModel file = new FileInfoModel();
                    file.FullName = fileInf.FullName;
                    file.Name = fileInf.Name;
                    file.IsReadOnly = fileInf.IsReadOnly;
                    file.FileSize = fileInf.Length;
                    file.Created = fileInf.CreationTime;
                    file.LastModyfied = fileInf.LastWriteTime;

                    infoText.Add( "       File");
                    infoText.Add($"       Name: {fileInf.Name}");
                    infoText.Add($"    Created: {fileInf.CreationTime}");
                    infoText.Add($"Last writed: {fileInf.LastWriteTime}");
                    infoText.Add($"   ReadOnly: {fileInf.IsReadOnly}");
                    infoText.Add($"Size, bytes: {fileInf.Length}");

                    if (fileInf.Attributes.ToString().Contains(','))
                    {
                        file.Attributes = fileInf.Attributes.ToString().Split(',');

                        infoText.Add($" Attributes: {file.Attributes[0]}");
                        for (int i = 1; i < file.Attributes.Length; i++)
                        {
                            infoText.Add($"            {file.Attributes[i]}");
                        }
                    }
                    else
                    {
                        infoText.Add($"Size, bytes: {fileInf.Length}");
                        file.Attributes = new string [] { fileInf.Attributes.ToString() };                            
                    }                        

                    report.ReportAboutCurrentFile(file);
                }
                else // dirs
                {
                    DirectoryInfoModel dir = new DirectoryInfoModel();
                    dir.FullName = dirInfo.FullName;
                    dir.Name = dirInfo.Name;
                    dir.Created = dirInfo.CreationTime;
                    dir.LastModyfied = dirInfo.LastWriteTime;

                    infoText.Add( "  Directory");
                    infoText.Add($"       Name: {dirInfo.Name}");
                    infoText.Add($"    Created: {dirInfo.CreationTime}");
                    infoText.Add($"Last writed: {dirInfo.LastWriteTime}");
                    infoText.Add($" Total size: {GetDirectorySize(dirInfo)} bytes");

                    if (dirInfo.Attributes.ToString().Contains(','))
                    {
                        dir.Attributes = dirInfo.Attributes.ToString().Split(',');

                        infoText.Add($" Attributes: {dir.Attributes[0]}");
                        for (int i = 1; i < dir.Attributes.Length; i++)
                        {
                            infoText.Add($"            {dir.Attributes[i]}");
                        }
                    }
                    else
                    { 
                        infoText.Add($" Attributes: {dirInfo.Attributes}");
                    }

                    report.ReportAboutCurrentDirectory(dir);
                }
            }
            else
            {
                infoText.Add("Parent directory info not avaliable.");
                infoText.Add("Firstly go there and select directory.");
            }

            //footer
            infoText.Add("Press Enter to close panel.");

            PrintInfoPanel(infoText, ConsoleColor.DarkBlue);
        }

        /// <summary>
        /// Get total size of directory with all subDirectories
        /// </summary>
        /// <param name="dir">Start directory</param>
        /// <returns></returns>        
        // Slow on big folders! 
        private long GetDirectorySize(DirectoryInfo dir)
        {
            long resultSize = 0;

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                resultSize += file.Length;
            }
            // subdirectory
            DirectoryInfo[] subDirectorys = dir.GetDirectories();
            foreach (DirectoryInfo subDir in subDirectorys)
            {
                resultSize += GetDirectorySize(subDir);
            }
            return resultSize;
        }



        /// <summary>
        /// show help panel with all avaliable actions
        /// </summary>
        internal void ShowHelp()
        {
            List<string> helpText = new List<string>()
            {
                "Help",//header
                "F1 - Help Panel                    F3 - File or Folder Info",
                "F5 - Copy File or Folder           F6 - Move File or Folder",
                "F7 - Create new Folder             F8 - Delete File or Folder",
                "F9 - Rename File or Folder         Alt+F4 - Exit",
                "arrows Up and Down, PgUp PgDown, Home End - navigate inside panel",
                "Tab - switch between panels",
                "(Ctrl + E): Command history select dialog",
                "(Ctrl + S): Search by mask dialog",
                "",
                "Manual commands, commands - case insensitive.",
                "(Ctrl + Enter): Copy current element to command line",                
                "Set passive panel same as active: equal",
                "Copy: cp sourcePath (to passive panel)",
                "Move: mv sourcePath (to passive panel)",
                "Change directory: cd NewPath       Execute File: run FileName",
                "Remove: rm sourcePath              New Folder: mkdir newName",
                //"New Folder: mkdir newName",
                //"Execute File: run FileName",
                "Rename: name sourcePath, newName",
                "Press Enter to close panel."//footer
            };

            PrintInfoPanel(helpText, ConsoleColor.DarkBlue);
        }

        private void PrintInfoPanel(List<string> panelText, ConsoleColor bgColor)
        {
            //calculate panel size
            int numbersOfLines = panelText.Count;
            int longestLine = 1;
            foreach (string line in panelText)
            {
                if (line.Length > longestLine)
                {
                    longestLine = line.Length + 1;
                }
            }
            //set cursor and sizes
            int xCursor = (_untilX - _fromX) - (longestLine / 2);
            int totalLenght = longestLine + 1;
            int lineNumber = (_height - numbersOfLines) / 2;

            //Print
            Console.BackgroundColor = bgColor;
            //Print header
            Console.ForegroundColor = ConsoleColor.Yellow;
            ClassLibrary.Do.PrintDialogHeader(" " + panelText[0], xCursor, lineNumber, totalLenght);
            Console.ForegroundColor = ConsoleColor.Gray;
            //Print body
            for (int i = 1; i < panelText.Count - 1; i++)
            {
                ClassLibrary.Do.PrintLinePanelText(" " + panelText[i], xCursor, ++lineNumber, totalLenght);
            }

            //Print footer
            Console.ForegroundColor = ConsoleColor.Yellow;
            ClassLibrary.Do.PrintDialogHeader(" " + panelText[panelText.Count - 1], xCursor, ++lineNumber, totalLenght);
            Console.ResetColor();

            //set cursor at the end and wait 'Enter'
            ClassLibrary.Do.SetCursorPosition(_untilX - _fromX, lineNumber);
            Console.ReadLine();
            //reset bg color
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
            if (_currentItem != 0) // not a parent Dir
            {
                int totalLenght;
                int lineNumber = _height / 3;               

                string text = " Press Y to confirm, any other to decline, then Enter";
                string source = $" {actionName} {_currentItemName} ";                
                if (actionName.Equals("Move") || actionName.Equals("Copy"))
                {
                    totalLenght = GetLongestInt(" to " + targetDirectory, text, source);
                }
                else
                {
                    totalLenght = GetLongestInt(text, source);
                }

                if(totalLenght > _width - 4)
                {
                    totalLenght = _width - 4;
                    source = ClassLibrary.Do.TextLineCutter(source, _width - (4 + 6)); // 6 is copy or move lenght
                    targetDirectory = ClassLibrary.Do.TextLineCutter(targetDirectory, _width - (4 + 4));
                }


                int xCursor = ((_width - totalLenght) / 2);

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
            _info = "Unknown command";
            _canBeExecute = false;
            _commandType = (int)Command.Unknown;

            if(newCommandText.Length >= 2)
            {
                if (newCommandText.Substring(0, 2).ToLower().Equals("cd"))
                {
                    _info = "ChangeDir: cd [Directory in active panel or new path]";
                    if (newCommandText.Length > 2)
                        CommandController(Command.ChangeDir, newCommandText.Substring(2));
                }
                else if (newCommandText.Substring(0, 2).ToLower().Equals("cp"))
                {
                    _info = "Copy to pasive panel: cp [object], [optional New Path]";
                    if (newCommandText.Length > 2)
                        CommandController(Command.Copy, newCommandText.Substring(2));
                }
                else if (newCommandText.Substring(0, 2).ToLower().Equals("mv"))
                {
                    _info = "Move to pasive panel: mv [object], [optional New Path]";
                    if (newCommandText.Length > 2)
                        CommandController(Command.Move, newCommandText.Substring(2));
                }
                else if (newCommandText.Substring(0, 2).ToLower().Equals("rm"))
                {
                    _info = "Remove Dir or File: rm [object]";
                    if (newCommandText.Length > 2)
                        CommandController(Command.Remove, newCommandText.Substring(2));
                }
                else if (newCommandText.Length >= 3)
                {
                    if (newCommandText.Substring(0, 3).ToLower().Equals("run"))
                    {
                        _info = "Run File: Run [fileName in active panel]";
                        if (newCommandText.Length > 3)
                            CommandController(Command.RunFile, newCommandText.Substring(3));
                    }
                    else if (newCommandText.Length >= 4)
                    {
                        if (newCommandText.Substring(0, 4).ToLower().Equals("name"))
                        {
                            _info = "Rename: Name [object], [New Name]";
                            if (newCommandText.Length > 4)
                                CommandController(Command.Rename, newCommandText.Substring(4));
                        }
                        else if (newCommandText.Substring(0, 4).ToLower().Equals("find"))
                        {
                            _info = "Find by mask: Find [search mask, '*' and '?' availible to use]";
                            if (newCommandText.Length > 4)
                                CommandController(Command.Find, newCommandText.Substring(4));
                        }
                        else if (newCommandText.Length >= 5)
                        {
                            if (newCommandText.Substring(0, 5).ToLower().Equals("mkdir"))
                            {
                                _info = "MakeNewDir: mkdir [New Directory Name in active panel]";
                                if (newCommandText.Length > 5)
                                    CommandController(Command.MakeNewDir, newCommandText.Substring(5));
                            }
                            else if (newCommandText.Substring(0, 5).ToLower().Equals("equal"))
                            {
                                _info = "Set passive panel path same as active: equal";
                                if (newCommandText.Length == 5)
                                    CommandController(Command.EqualisePanels, newCommandText.Substring(5));
                                else if (newCommandText.Length > 5)
                                    _info = "Wrong format: Equal must be without arguments";
                            }
                        }
                    }                    
                }
            }

            // user press Enter - execute user command
            if (execute)
            {
                if (_canBeExecute)
                {
                    ExecuteUserCommand();
                }
                else
                {
                    ClassLibrary.Do.ShowAlert("Try to execute command line - Unknown command or not correct arguments", _untilX - _fromX);
                }
                _info = "";
            }

            PrintCommandInfo();
        }

        /// <summary>
        /// print tips or error for command line
        /// </summary>
        private void PrintCommandInfo()
        {
            ClassLibrary.Do.SetCursorPosition(1, _height - 4);
            string infoOnConsole = ClassLibrary.Do.TextLineCutter(_info, _width - 2);
            Console.Write(infoOnConsole);
        }

        /// <summary>
        /// execute current command. current command setted in CommandController
        /// </summary>
        internal void ExecuteUserCommand()
        {
            string sourceItem = _argumentSource;
            if (_commandType != (int)Command.EqualisePanels)
            {
                try
                {
                    DirectoryInfo source = new DirectoryInfo(sourceItem);
                    if (!source.Exists)
                    {
                        sourceItem = Path.Combine(_active.StartDirectory, _argumentSource);
                    }
                }
                catch (Exception e)
                {
                    ClassLibrary.Do.ShowAlert("Execute User Command - Try to get directory - " + e.Message, _untilX - _fromX);

                }
            }

            switch (_commandType)
            {
                case (int)Command.EqualisePanels:
                    _passive.StartDirectory = _active.StartDirectory;
                    break;
                case (int)Command.MakeNewDir:
                    MkDir(_argumentSource);
                    break;
                case (int)Command.RunFile:
                    RunFile(sourceItem);
                    break;
                case (int)Command.Remove:
                    DeleteItem(sourceItem);
                    break;
                case (int)Command.Rename:
                    MoveOrRename(_argumentSource, _argumentTarget);
                    break;
                case (int)Command.Move:
                    MoveOrRename(_argumentSource, _argumentTarget);
                    break;
                case (int)Command.Copy:
                    CopyFromCommandLine();
                    break;
                case (int)Command.Find:
                    ShowFindedItems(_argumentSource);
                    break;
                case (int)Command.ChangeDir:
                    _active.CurrentItem = 0;
                    _active.StartDirectory = _argumentSource;
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
                    _canBeExecute = true;
                    _commandType = (int)Command.EqualisePanels;
                    break;

                case Command.MakeNewDir:
                    bool checkArguments = CheckExist(arguments, false, true);
                    if (checkArguments)
                    {
                        _canBeExecute = true;
                        _commandType = (int)Command.MakeNewDir;
                        _argumentSource = arguments.Substring(1);
                    }
                    break;

                case Command.ChangeDir:
                    checkArguments = CheckExist(arguments, true, true);
                    if (checkArguments)
                    {
                        _canBeExecute = true;
                        _commandType = (int)Command.ChangeDir;
                        arguments = " " + CheckParentRelativePath(arguments.Substring(1), true);
                        _argumentSource = GetCorrectPath(arguments.Substring(1));
                        _argumentSource = ItIsDirectory(_argumentSource); // files not allowed here. Cut path to directory
                    }
                    break;

                case Command.RunFile:
                    checkArguments = CheckExist(arguments, true, true);
                    if (checkArguments)
                    {
                        _canBeExecute = true;
                        _commandType = (int)Command.RunFile;
                        _argumentSource = arguments.Substring(1);
                    }
                    break;

                case Command.Remove:
                    checkArguments = CheckExist(arguments, true, true);
                    if (checkArguments)
                    {
                        _canBeExecute = true;
                        _commandType = (int)Command.Remove;
                        _argumentSource = arguments.Substring(1);
                    }
                    break;

                case Command.Rename:
                    checkArguments = CheckPairExist(arguments, true, false);
                    if (checkArguments)
                        _commandType = (int)Command.Rename;
                    break;

                case Command.Move:
                    checkArguments = CheckPairExist(arguments, false, false);
                    if (checkArguments)
                        _commandType = (int)Command.Move;
                    break;

                case Command.Copy:
                    checkArguments = CheckPairExist(arguments, false, false);
                    if (checkArguments)
                        _commandType = (int)Command.Copy;
                    break;

                case Command.Find:
                    if (arguments.Length > 1)
                    {
                        _canBeExecute = true;
                        _commandType = (int)Command.Find;
                        _argumentSource = arguments.Substring(1);
                    }
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
                _canBeExecute = false;
                string arg1 = arguments.Substring(0, arguments.IndexOf(','));
                checkFirstArguments = CheckExist(arg1, true, true);

                //without correct first argument - second is useless
                if (checkFirstArguments == true)
                {
                    if (arguments.Substring(arguments.IndexOf(',')).Length > 1)
                    {
                        // for rename we check in active panel. else in passive
                        if (_info.Contains("Rename"))
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
                            _argumentSource = GetCorrectPath(arguments.Substring(1, arguments.IndexOf(',') - 1));

                            //set path from second argument
                            //if it is RENAME - we need to use active panel current directory. Else - passive current directory
                            if (_info.Contains("Rename"))
                            {
                                _argumentTarget = GetNonExistPath(arguments.Substring(arguments.IndexOf(',') + 2), true);
                            }
                            else
                            {
                                _argumentTarget = GetNonExistPath(arguments.Substring(arguments.IndexOf(',') + 2), false);
                            } 
                            
                            _canBeExecute = true;
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
                    _argumentSource = GetCorrectPath(arguments.Substring(1));
                    _argumentTarget = Path.Combine(_passive.StartDirectory, _argumentSource.Substring(_argumentSource.LastIndexOf('\\') + 1));

                    _canBeExecute = true;
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
                            path = Path.Combine(_active.StartDirectory, path);
                        }
                        else // copy move
                        {
                            path = Path.Combine(_passive.StartDirectory, path);
                        }
                    }                     
               }

                return path;
            }
            else // just name
            {
                if (inActivePanel) // Rename
                {
                    path = Path.Combine(_active.StartDirectory, path);
                }
                else // copy move
                {
                    path = Path.Combine(_passive.StartDirectory, path);
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
                return Path.Combine(_active.StartDirectory, pathOrItem);
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
                        _info = $"Directory or file '{arguments.Substring(1)}' not found!";
                        return false;
                    }

                    sourceItem = CheckParentRelativePath(sourceItem, isActivePanel);

                    // firsly check if it is direct path to file or dir
                    actual = CheckIsItFullPath(sourceItem);

                    // if not - check this item in current folder
                    if (!actual)
                    {
                        sourceItem = Path.Combine(_active.StartDirectory, sourceItem).ToLower();
                        if (!isActivePanel)// case os rename
                            sourceItem = Path.Combine(_passive.StartDirectory, sourceItem).ToLower();

                        foreach (string str in _active.AllItems)
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
                    _info = $"Enter Dir or File name, ' ' not enough.";
                    return false;
                }

                if (expected == actual)
                    return true;
                else
                {
                    if(expected)
                        _info = $"Directory or file '{arguments.Substring(1)}' not found!";
                    else
                        _info = $"Directory or file '{arguments.Substring(1)}' already exist!";

                    return false;
                }
            }
            else // wrong command!
            {                
                _info = "There is no space between command and path!" + arguments;

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
                        DirectoryInfo dir = new DirectoryInfo(_active.StartDirectory);
                        if(!isActivePanel)// case os rename
                            dir = new DirectoryInfo(_passive.StartDirectory);

                        if (dir.Parent != null && dir.Parent.Exists)
                        {
                            sourceItem = dir.Parent.FullName.ToString();
                        }
                    }
                    else // middle of string path 
                    {
                        sourceItem = Path.Combine(_active.StartDirectory, head);
                        if (!isActivePanel)// case os rename
                            sourceItem = Path.Combine(_passive.StartDirectory, head);

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
            int xCursor = (_untilX - _fromX) / 2;
            int lineNumber = _height / 3;
            string requestText = " Enter new name, or leave it empty and press Enter:";
            int totalLenght = requestText.Length + 1;
            Console.BackgroundColor = ConsoleColor.Blue;

            if (_width - 4 < totalLenght) // shorten text
            {
                totalLenght = _width - 4;
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

        private string GetNameFromUser(string header, string requestText)
        {
            List<string> textLines = new List<string>();

            // raw panel size calculation
            int xCursor;
            int totalLenght = _width - 4;
            if (totalLenght > 50)
            {
                //xCursor = (_width / 3) / 2;
                totalLenght = (_width / 3) * 2; // 2/3
            }

            //calculate request lines number
            if (requestText.Length + 2 > totalLenght) //+2 spaces at both sides
            {
                List<string> words = requestText.Split(' ').ToList();
                // split
                string newLine = " ";
                foreach (string word in words)
                {
                    if ((newLine + word + " ").Length > totalLenght)
                    {
                        textLines.Add(newLine);
                        newLine = " " + word + " ";
                    }
                    else
                    {
                        newLine = newLine + word + " ";
                    }
                }
                // add last line
                textLines.Add(newLine);
            }
            else
            {
                textLines.Add(requestText);
            }

            // exact panel size calculation
            int numbersOfLines = textLines.Count + 2; // +2 for header and input string
            int longestLine = 1;
            foreach (string line in textLines)
            {
                if (line.Length > longestLine)
                {
                    longestLine = line.Length;
                }
            }
            //set cursor and sizes
            xCursor = (_width - longestLine) / 2;
            totalLenght = longestLine;
            int lineNumber = (_height - numbersOfLines) / 2;

            //Print
            Console.BackgroundColor = ConsoleColor.Blue;
            //Print header
            Console.ForegroundColor = ConsoleColor.Yellow;
            ClassLibrary.Do.PrintDialogHeader(header, xCursor, lineNumber, totalLenght);
            Console.ForegroundColor = ConsoleColor.Gray;

            //Print body
            for (int i = 0; i < textLines.Count; i++)
            {
                ClassLibrary.Do.PrintLinePanelText(textLines[i], xCursor, ++lineNumber, totalLenght);
            }

            //Print footer
            ClassLibrary.Do.PrintLinePanelText(" ", xCursor, ++lineNumber, totalLenght);

            //set cursor at the input line and wait 'Enter'
            ClassLibrary.Do.SetCursorPosition(xCursor + 1, lineNumber);
            string newName = Console.ReadLine();
            //reset bg color
            Console.BackgroundColor = ConsoleColor.Black;

            return newName;
        }
    }
}
