using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ConsoleFileManager
{
    internal class FileManager
    {
        private string _startDirectoryLeft { get; set; }
        private string _startDirectoryRight { get; set; }
        private int _consoleWidth { get; set; }

        private int _consoleHeight;
        internal int ConsoleHeight
        {
            get
            {
                return _consoleHeight;
            }
            set
            {
                if (value > 6)
                    _consoleHeight = value;
                else
                {
                    _consoleHeight = 7;
                    Console.WindowHeight = 7;
                }
            }
        }

        private int _currentItemLeft { get; set; }
        private int _currentItemRight { get; set; }
        private bool _leftIsActive { get; set; }
        private FilePanel _active { get; set; }
        private FilePanel _passive { get; set; }
        private string _newCommandText { get; set; }

        internal FileManager()
        {
            DataXML xml = new DataXML();
            xml.GetDataFromXML();

            _consoleWidth = xml.XMLConsoleWidth;
            ConsoleHeight = xml.XMLConsoleHeight;
            _startDirectoryLeft = xml.XMLStartDirectoryLeft;
            _startDirectoryRight = xml.XMLStartDirectoryRight;
            _newCommandText = "";
            bool leftIsActive = xml.LeftIsActive;

            Console.SetWindowSize(_consoleWidth, ConsoleHeight);
            //Console.SetBufferSize(Console.LargestWindowWidth, Console.LargestWindowHeight);

            Borders border = new Borders();
            FilePanel filePanelLeft = new FilePanel(_startDirectoryLeft, 1, _consoleWidth / 2 - 1);
            filePanelLeft.CurrentItem = xml.XMLLeftActiveItem;


            FilePanel filePanelRight = new FilePanel(_startDirectoryRight, _consoleWidth / 2, _consoleWidth - 1);
            filePanelRight.CurrentItem = xml.XMLRightActiveItem;

            if (leftIsActive)
            {
                filePanelLeft.IsActive = true;
                _active = filePanelLeft;
                filePanelRight.IsActive = false;
                _passive = filePanelRight;
            }
            else
            {
                filePanelLeft.IsActive = false;
                _active = filePanelRight;
                filePanelRight.IsActive = true;
                _passive = filePanelLeft;
            }

            _currentItemLeft = filePanelLeft.CurrentItem;
            _currentItemRight = filePanelRight.CurrentItem;
            _leftIsActive = filePanelLeft.IsActive;

            PrintFileManager(filePanelLeft, filePanelRight, border);

            GetUserCommands(filePanelLeft, filePanelRight, border);// Don't add any command after call this method - because endless while!
        }

        private void PrintFileManager(FilePanel filePanelLeft, FilePanel filePanelRight, Borders border)
        {
            border.BorderWidth = _consoleWidth;
            border.BorderHeight = ConsoleHeight;
            border.PrintBorders();

            filePanelLeft.PanelHeight = ConsoleHeight;
            filePanelLeft.UntilX = _consoleWidth / 2 - 1;
            filePanelLeft.ShowDirectoryContent();

            filePanelRight.PanelHeight = ConsoleHeight;
            filePanelRight.FromX = _consoleWidth / 2;
            filePanelRight.UntilX = _consoleWidth - 1;
            filePanelRight.ShowDirectoryContent();

            PrintSingleKeyCommands(); // like F1 F2 etc.

            PrintUserCommand();
        }

        /// <summary>
        /// avaiting key input and react on keys, react on change panels size
        /// </summary>
        /// <param name="filePanelLeft"></param>
        /// <param name="filePanelRight"></param>
        /// <param name="border"></param>
        private void GetUserCommands(FilePanel filePanelLeft, FilePanel filePanelRight, Borders border)
        {
            bool exit = false;
            while (!exit)
            {
                bool xmlBeSaved = false;

                if (Console.WindowWidth != _consoleWidth || Console.WindowHeight != ConsoleHeight)
                {

                    if (Console.WindowWidth != _consoleWidth)
                    {
                        xmlBeSaved = true;
                    }

                    if (Console.WindowHeight != ConsoleHeight)
                    {
                        xmlBeSaved = true;
                    }

                    Console.Clear();
                    _consoleWidth = Console.WindowWidth;
                    ConsoleHeight = Console.WindowHeight;

                    PrintFileManager(filePanelLeft, filePanelRight, border);
                }

                //else values to xml check for changes
                if (filePanelLeft.StartDirectory != _startDirectoryLeft)
                {
                    xmlBeSaved = true;
                }

                if (filePanelRight.StartDirectory != _startDirectoryRight)
                {
                    xmlBeSaved = true;
                }

                if (filePanelLeft.CurrentItem != _currentItemLeft)
                {
                    xmlBeSaved = true;
                }

                if (filePanelRight.CurrentItem != _currentItemRight)
                {
                    xmlBeSaved = true;
                }

                if (filePanelLeft.IsActive != _leftIsActive)
                {
                    xmlBeSaved = true;
                    _leftIsActive = filePanelLeft.IsActive;
                }

                if (xmlBeSaved)
                {
                    string pathConfigXML = "Resources/Config.xml";
                    XmlDocument xmlConfig = new XmlDocument();
                    xmlConfig.Load(pathConfigXML);

                    XmlNode nodeWidth = xmlConfig.SelectSingleNode("//Width");
                    nodeWidth.InnerText = Console.WindowWidth.ToString();

                    XmlNode nodeHeight = xmlConfig.SelectSingleNode("//Height");
                    nodeHeight.InnerText = Console.WindowHeight.ToString();

                    XmlNode nodeLastDirL = xmlConfig.SelectSingleNode("//LeftStartDir");
                    nodeLastDirL.InnerText = filePanelLeft.StartDirectory;

                    XmlNode nodeLastDirR = xmlConfig.SelectSingleNode("//RightStartDir");
                    nodeLastDirR.InnerText = filePanelRight.StartDirectory;

                    XmlNode nodeLeftActive = xmlConfig.SelectSingleNode("//LeftActiveItem");
                    nodeLeftActive.InnerText = filePanelLeft.CurrentItem.ToString();

                    XmlNode nodeRightActive = xmlConfig.SelectSingleNode("//RightActiveItem");
                    nodeRightActive.InnerText = filePanelRight.CurrentItem.ToString();

                    XmlNode nodeActive = xmlConfig.SelectSingleNode("//LeftIsActive");
                    nodeActive.InnerText = filePanelLeft.IsActive.ToString();

                    try
                    {
                        xmlConfig.Save(pathConfigXML);
                    }
                    catch (Exception s)
                    {
                        ClassLibrary.Do.ShowAlert($"Save to configuration XML file {pathConfigXML} Error - " + s.Message, _consoleWidth / 2);
                    }
                }

                Actions newActon = new Actions(_active, _passive, _consoleWidth);
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo userKey = Console.ReadKey(true);

                    //show commands history
                    if ((ConsoleModifiers.Control & userKey.Modifiers) != 0 && userKey.Key == ConsoleKey.E)
                    {
                        _newCommandText = newActon.GetCommandsHystory();

                        PrintFileManager(filePanelLeft, filePanelRight, border);
                    }
                    // search something with mask like *.*
                    else if ((ConsoleModifiers.Control & userKey.Modifiers) != 0 && userKey.Key == ConsoleKey.S)// S - because Ctrl+F already reserved with windows search
                    {
                        string newSearch = newActon.GetNameForMaskedSearh();
                        if (newSearch.Length > 0)
                        {
                            PrintFileManager(filePanelLeft, filePanelRight, border);
                            newActon.ShowFindedItems(newSearch);
                        }

                        PrintFileManager(filePanelLeft, filePanelRight, border);
                    }

                    if (Char.IsControl(userKey.KeyChar))
                    {
                        bool isConfirmed = false;

                        switch (userKey.Key)
                        {
                            case ConsoleKey.Tab:
                                FilePanel temp = _active;
                                _active = _passive;
                                _passive = temp;
                                _active.IsActive = true;
                                _passive.IsActive = false;
                                filePanelLeft.ShowDirectoryContent();
                                filePanelRight.ShowDirectoryContent();
                                ClassLibrary.Do.WriteCommandToFile($"cd {_active.StartDirectory}");
                                break;

                            case ConsoleKey.DownArrow:
                                newActon.ChangeCurrentItem(1);                                
                                break;

                            case ConsoleKey.UpArrow:
                                newActon.ChangeCurrentItem(-1);                                
                                break;

                            case ConsoleKey.PageDown:
                                newActon.ChangeCurrentItem(100);                                
                                break;

                            case ConsoleKey.PageUp:
                                newActon.ChangeCurrentItem(-100);                                
                                break;

                            case ConsoleKey.Home:
                                newActon.ChangeCurrentItem(-1000);                                
                                break;

                            case ConsoleKey.End:
                                newActon.ChangeCurrentItem(1000);                                
                                break;

                            case ConsoleKey.F1:
                                newActon.ShowHelp();
                                PrintFileManager(filePanelLeft, filePanelRight, border);
                                break;

                            case ConsoleKey.F3:                                
                                newActon.ShowInfo();
                                PrintFileManager(filePanelLeft, filePanelRight, border);
                                break;

                            case ConsoleKey.F5:
                                isConfirmed = newActon.UserConfirmAction("Copy", _passive.StartDirectory);
                                if (isConfirmed)
                                    newActon.CopyFromPanel();
                                PrintFileManager(filePanelLeft, filePanelRight, border);
                                break;

                            case ConsoleKey.F6:
                                isConfirmed = newActon.UserConfirmAction("Move", _passive.StartDirectory);
                                if(isConfirmed)
                                    newActon.MoveItemTo(_passive.StartDirectory);
                                PrintFileManager(filePanelLeft, filePanelRight, border);
                                break;

                            case ConsoleKey.F7:
                                newActon.CreateNewDir("Create New Directory");
                                PrintFileManager(filePanelLeft, filePanelRight, border);
                                break;

                            case ConsoleKey.F8:
                                isConfirmed = newActon.UserConfirmAction("Delete", _active.CurrentItemName);
                                if (isConfirmed && _active.CurrentItem != 0)
                                    newActon.DeleteItem(Path.Combine(_active.StartDirectory, _active.CurrentItemName));
                                PrintFileManager(filePanelLeft, filePanelRight, border);
                                break;

                            case ConsoleKey.F9:
                                newActon.RenameItem("Rename");
                                PrintFileManager(filePanelLeft, filePanelRight, border);
                                break;

                            case ConsoleKey.Enter:
                                if ((userKey.Modifiers & ConsoleModifiers.Control) != 0)
                                {
                                    _newCommandText = _newCommandText + _active.CurrentItemName;
                                }
                                else
                                {
                                    if (_newCommandText.Length > 0)
                                    {
                                        newActon.AnalizeCommand(_newCommandText, true);
                                        ClassLibrary.Do.WriteCommandToFile(_newCommandText);
                                        _newCommandText = "";                                        
                                    }
                                    else
                                    {
                                        newActon.ExecuteCurrent();
                                    }
                                    PrintFileManager(filePanelLeft, filePanelRight, border);
                                }
                                break;

                            case ConsoleKey.Backspace:
                                if (_newCommandText.Length > 0)
                                    _newCommandText = _newCommandText.Substring(0, _newCommandText.Length - 1);
                                break;

                            default:
                                break;
                        }
                    }
                    else
                    {
                        if ((userKey.Modifiers & ConsoleModifiers.Shift) != 0)
                        {
                            _newCommandText = _newCommandText + userKey.KeyChar;
                        }
                        else
                        {
                            _newCommandText = _newCommandText + userKey.KeyChar.ToString().ToLower();
                        }
                    }

                    PrintUserCommand();
                }
            }
        }

        /// <summary>
        /// info line with F-key list
        /// </summary>
        private void PrintSingleKeyCommands()
        {
            ClassLibrary.Do.SetCursorPosition(1, ConsoleHeight - 6);

            string f1 = "[F1 Help]";
            string f3 = "[F3 Info]";
            string f5 = "[F5 Copy]";
            string f6 = "[F6 Move]";
            string f7 = "[F7 MkDir]";
            string f8 = "[F8 Del]";
            string f9 = "[F9 Rename]";
            string exit = "[Alt F4 Exit]";
            int exitLenght = 14;
            int totalLenght = 80;

            if(_consoleWidth < totalLenght)
            {
                f1 = "[F1]";
                f3 = "[F3]";
                f5 = "[F5]";
                f6 = "[F6]";
                f7 = "[F7]";
                f8 = "[F8]";
                f9 = "[F9]";
                exit = "[Alt F4]";
                exitLenght = 9;
                totalLenght = 32;
            }

            //80 = all symbols lenght
            int padding = (_consoleWidth - totalLenght) / 8; // delimeter between [F] text
            if (padding < 0)
                padding = 0;

            Console.BackgroundColor = ConsoleColor.Black;//to clean leftovers from Info panels

            Console.Write(f1 + new string(' ', padding));
            Console.Write(f3 + new string(' ', padding));
            Console.Write(f5 + new string(' ', padding));
            Console.Write(f6 + new string(' ', padding));
            Console.Write(f7 + new string(' ', padding));
            Console.Write(f8 + new string(' ', padding));
            Console.Write(f9);
            
            int padddingLast = _consoleWidth - exitLenght - Console.CursorLeft;
            if (padddingLast < 0)
                padddingLast = 0;
            Console.Write(" ".PadRight(padddingLast));//paint line till [Alt F4 Exit]

            ClassLibrary.Do.SetCursorPosition(_consoleWidth - exitLenght, ConsoleHeight - 6);          
            Console.Write(exit);
        }

        /// <summary>
        /// print user command and set cursor to the command end
        /// </summary>
        private void PrintUserCommand()
        {
            if (_newCommandText.Length > 0)
            {
                Actions newUserAction = new Actions(_active, _passive, _consoleWidth);
                newUserAction.AnalizeCommand(_newCommandText, false);
            }

            ClassLibrary.Do.SetCursorPosition(1, ConsoleHeight - 3);

            string commandOnConsole = ClassLibrary.Do.TextLineCutter("Command:" + _newCommandText, Console.WindowWidth - 2);
            Console.Write(commandOnConsole);
            
            ClassLibrary.Do.SetCursorPosition(("Command:" + _newCommandText).Length + 1, Console.CursorTop);
        }
    }
}
