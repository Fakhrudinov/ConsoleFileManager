using System;
using System.IO;
using System.Xml;

namespace ConsoleFileManager
{
    public class FileManager
    {
        public string StartDirectoryLeft { get; set; }
        public string StartDirectoryRight { get; set; }
        public int ConsoleWidth { get; set; }

        int consoleHeight;
        public int ConsoleHeight
        {
            get
            {
                return consoleHeight;
            }
            set
            {
                if (value > 6)
                    consoleHeight = value;
                else
                {
                    consoleHeight = 7;
                    Console.WindowHeight = 7;
                }
            }
        }

        public int CurrentItemLeft { get; set; }
        public int CurrentItemRight { get; set; }
        public bool LeftIsActive { get; set; }
        FilePanel Active { get; set; }
        FilePanel Passive { get; set; }
        public string NewCommandText { get; set; }

        public FileManager()
        {
            DataXML xml = new DataXML();
            xml.GetDataFromXML();

            ConsoleWidth = xml.XMLConsoleWidth;
            ConsoleHeight = xml.XMLConsoleHeight;
            StartDirectoryLeft = xml.XMLStartDirectoryLeft;
            StartDirectoryRight = xml.XMLStartDirectoryRight;
            NewCommandText = "";
            bool leftIsActive = xml.LeftIsActive;

            Console.SetWindowSize(ConsoleWidth, ConsoleHeight);
            //Console.SetBufferSize(Console.LargestWindowWidth, Console.LargestWindowHeight);

            Borders border = new Borders();
            FilePanel filePanelLeft = new FilePanel(StartDirectoryLeft, 1, ConsoleWidth / 2 - 1);
            filePanelLeft.CurrentItem = xml.XMLLeftActiveItem;


            FilePanel filePanelRight = new FilePanel(StartDirectoryRight, ConsoleWidth / 2, ConsoleWidth - 1);
            filePanelRight.CurrentItem = xml.XMLRightActiveItem;

            if (leftIsActive)
            {
                filePanelLeft.IsActive = true;
                Active = filePanelLeft;
                filePanelRight.IsActive = false;
                Passive = filePanelRight;
            }
            else
            {
                filePanelLeft.IsActive = false;
                Active = filePanelRight;
                filePanelRight.IsActive = true;
                Passive = filePanelLeft;
            }

            CurrentItemLeft = filePanelLeft.CurrentItem;
            CurrentItemRight = filePanelRight.CurrentItem;
            LeftIsActive = filePanelLeft.IsActive;

            PrintFileManager(filePanelLeft, filePanelRight, border);

            GetUserCommands(filePanelLeft, filePanelRight, border);// Don't add any command after call this method - because endless while!
        }

        private void PrintFileManager(FilePanel filePanelLeft, FilePanel filePanelRight, Borders border)
        {
            border.BorderWidth = ConsoleWidth;
            border.BorderHeight = ConsoleHeight;
            border.PrintBorders();

            filePanelLeft.PanelHeight = ConsoleHeight;
            filePanelLeft.UntilX = ConsoleWidth / 2 - 1;
            filePanelLeft.ShowDirectoryContent();

            filePanelRight.PanelHeight = ConsoleHeight;
            filePanelRight.FromX = ConsoleWidth / 2;
            filePanelRight.UntilX = ConsoleWidth - 1;
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
        public void GetUserCommands(FilePanel filePanelLeft, FilePanel filePanelRight, Borders border)
        {
            bool exit = false;
            while (!exit)
            {
                bool xmlBeSaved = false;

                if (Console.WindowWidth != ConsoleWidth || Console.WindowHeight != ConsoleHeight)
                {

                    if (Console.WindowWidth != ConsoleWidth)
                    {
                        xmlBeSaved = true;
                    }

                    if (Console.WindowHeight != ConsoleHeight)
                    {
                        xmlBeSaved = true;
                    }

                    Console.Clear();
                    ConsoleWidth = Console.WindowWidth;
                    ConsoleHeight = Console.WindowHeight;

                    PrintFileManager(filePanelLeft, filePanelRight, border);
                }

                //else values to xml check for changes
                if (filePanelLeft.StartDirectory != StartDirectoryLeft)
                {
                    xmlBeSaved = true;
                }

                if (filePanelRight.StartDirectory != StartDirectoryRight)
                {
                    xmlBeSaved = true;
                }

                if (filePanelLeft.CurrentItem != CurrentItemLeft)
                {
                    xmlBeSaved = true;
                }

                if (filePanelRight.CurrentItem != CurrentItemRight)
                {
                    xmlBeSaved = true;
                }

                if (filePanelLeft.IsActive != LeftIsActive)
                {
                    xmlBeSaved = true;
                    LeftIsActive = filePanelLeft.IsActive;
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
                        ClassLibrary.Do.ShowAlert($"Save to configuration XML file {pathConfigXML} Error - " + s.Message, ConsoleWidth / 2);
                    }
                }

                Actions newActon = new Actions(Active, Passive, ConsoleWidth);
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo userKey = Console.ReadKey(true);

                    if ((ConsoleModifiers.Control & userKey.Modifiers) != 0 && userKey.Key == ConsoleKey.E)
                    {
                        NewCommandText = newActon.GetCommandsHystory();

                        PrintFileManager(filePanelLeft, filePanelRight, border);
                    }

                    if (Char.IsControl(userKey.KeyChar))
                    {
                        bool isConfirmed = false;

                        switch (userKey.Key)
                        {
                            case ConsoleKey.Tab:
                                FilePanel temp = Active;
                                Active = Passive;
                                Passive = temp;
                                Active.IsActive = true;
                                Passive.IsActive = false;
                                filePanelLeft.ShowDirectoryContent();
                                filePanelRight.ShowDirectoryContent();
                                ClassLibrary.Do.WriteCommandToFile($"cd {Active.StartDirectory}");
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
                                isConfirmed = newActon.UserConfirmAction("Copy", Passive.StartDirectory);
                                if (isConfirmed)
                                    newActon.CopyFromPanel();
                                PrintFileManager(filePanelLeft, filePanelRight, border);
                                break;

                            case ConsoleKey.F6:
                                isConfirmed = newActon.UserConfirmAction("Move", Passive.StartDirectory);
                                if(isConfirmed)
                                    newActon.MoveItemTo(Passive.StartDirectory);
                                PrintFileManager(filePanelLeft, filePanelRight, border);
                                break;

                            case ConsoleKey.F7:
                                newActon.CreateNewDir("Create New Directory");
                                PrintFileManager(filePanelLeft, filePanelRight, border);
                                break;

                            case ConsoleKey.F8:
                                isConfirmed = newActon.UserConfirmAction("Delete", Active.CurrentItemName);
                                if (isConfirmed && Active.CurrentItem != 0)
                                    newActon.DeleteItem(Path.Combine(Active.StartDirectory, Active.CurrentItemName));
                                PrintFileManager(filePanelLeft, filePanelRight, border);
                                break;

                            case ConsoleKey.F9:
                                newActon.RenameItem("Rename");
                                PrintFileManager(filePanelLeft, filePanelRight, border);
                                break;

                            case ConsoleKey.Enter:
                                if ((userKey.Modifiers & ConsoleModifiers.Control) != 0)
                                {
                                    NewCommandText = NewCommandText + Active.CurrentItemName;
                                }
                                else
                                {
                                    if (NewCommandText.Length > 0)
                                    {
                                        newActon.AnalizeCommand(NewCommandText, true);
                                        ClassLibrary.Do.WriteCommandToFile(NewCommandText);
                                        NewCommandText = "";                                        
                                    }
                                    else
                                    {
                                        newActon.ExecuteCurrent();
                                    }
                                    PrintFileManager(filePanelLeft, filePanelRight, border);
                                }
                                break;

                            case ConsoleKey.Backspace:
                                if (NewCommandText.Length > 0)
                                    NewCommandText = NewCommandText.Substring(0, NewCommandText.Length - 1);
                                break;

                            default:
                                break;
                        }
                    }
                    else
                    {
                        if ((userKey.Modifiers & ConsoleModifiers.Shift) != 0)
                        {
                            NewCommandText = NewCommandText + userKey.KeyChar;
                        }
                        else
                        {
                            NewCommandText = NewCommandText + userKey.KeyChar.ToString().ToLower();
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

            if(ConsoleWidth < totalLenght)
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
            int padding = (ConsoleWidth - totalLenght) / 8; // delimeter between [F] text
            if (padding < 0)
                padding = 0;

            Console.Write(f1 + new string(' ', padding));
            Console.Write(f3 + new string(' ', padding));
            Console.Write(f5 + new string(' ', padding));
            Console.Write(f6 + new string(' ', padding));
            Console.Write(f7 + new string(' ', padding));
            Console.Write(f8 + new string(' ', padding));
            Console.Write(f9);
            
            int padddingLast = ConsoleWidth - exitLenght - Console.CursorLeft;
            if (padddingLast < 0)
                padddingLast = 0;
            Console.Write(" ".PadRight(padddingLast));//paint line till [Alt F4 Exit]

            ClassLibrary.Do.SetCursorPosition(ConsoleWidth - exitLenght, ConsoleHeight - 6);          
            Console.Write(exit);
        }

        /// <summary>
        /// print user command and set cursor to the command end
        /// </summary>
        private void PrintUserCommand()
        {
            if (NewCommandText.Length > 0)
            {
                Actions newUserAction = new Actions(Active, Passive, ConsoleWidth);
                newUserAction.AnalizeCommand(NewCommandText, false);
            }

            ClassLibrary.Do.SetCursorPosition(1, ConsoleHeight - 3);

            string commandOnConsole = ClassLibrary.Do.TextLineCutter("Command:" + NewCommandText, Console.WindowWidth - 2);
            Console.Write(commandOnConsole);
            
            ClassLibrary.Do.SetCursorPosition(("Command:" + NewCommandText).Length + 1, Console.CursorTop);
        }
    }
}
