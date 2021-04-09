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

        public void GetUserCommands(FilePanel filePanelLeft, FilePanel filePanelRight, Borders border)
        {
            bool exit = false;
            while (!exit)
            {
                string pathConfigXML = "Resources/Config.xml";
                XmlDocument xmlConfig = new XmlDocument();
                xmlConfig.Load(pathConfigXML);

                bool xmlBeSaved = false;

                if (Console.WindowWidth != ConsoleWidth || Console.WindowHeight != ConsoleHeight)
                {

                    if (Console.WindowWidth != ConsoleWidth)
                    {
                        XmlNode nodeWidth = xmlConfig.SelectSingleNode("//Width");
                        nodeWidth.InnerText = Console.WindowWidth.ToString();
                        xmlBeSaved = true;
                    }

                    if (Console.WindowHeight != ConsoleHeight)
                    {
                        XmlNode nodeHeight = xmlConfig.SelectSingleNode("//Height");
                        nodeHeight.InnerText = Console.WindowHeight.ToString();
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
                    XmlNode nodeLastDirL = xmlConfig.SelectSingleNode("//LeftStartDir");
                    nodeLastDirL.InnerText = filePanelLeft.StartDirectory;
                    xmlBeSaved = true;
                }

                if (filePanelRight.StartDirectory != StartDirectoryRight)
                {
                    XmlNode nodeLastDirR = xmlConfig.SelectSingleNode("//RightStartDir");
                    nodeLastDirR.InnerText = filePanelRight.StartDirectory;
                    xmlBeSaved = true;
                }

                if (filePanelLeft.CurrentItem != CurrentItemLeft)
                {
                    XmlNode nodeLeftActive = xmlConfig.SelectSingleNode("//LeftActiveItem");
                    nodeLeftActive.InnerText = filePanelLeft.CurrentItem.ToString();
                    xmlBeSaved = true;
                }

                if (filePanelRight.CurrentItem != CurrentItemRight)
                {
                    XmlNode nodeRightActive = xmlConfig.SelectSingleNode("//RightActiveItem");
                    nodeRightActive.InnerText = filePanelRight.CurrentItem.ToString();
                    xmlBeSaved = true;
                }

                if (filePanelLeft.IsActive != LeftIsActive)
                {
                    XmlNode nodeActive = xmlConfig.SelectSingleNode("//LeftIsActive");
                    nodeActive.InnerText = filePanelLeft.IsActive.ToString();
                    xmlBeSaved = true;
                    LeftIsActive = filePanelLeft.IsActive;
                }

                if (xmlBeSaved)
                {
                    try
                    {
                        xmlConfig.Save(pathConfigXML);
                    }
                    catch (Exception s)
                    {
                        Active.ShowAlert($"Can't save data to xml file {pathConfigXML}.\r" + s.Message);
                    }
                }

                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo userKey = Console.ReadKey(true);

                    if (Char.IsControl(userKey.KeyChar))
                    {
                        bool isConfirmed = false;
                        Actions newActon = new Actions(Active, Passive, ConsoleWidth);

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
                                newActon.ShowInfo(Path.Combine(Active.StartDirectory, Active.CurrentItemName));
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
                        // return pointer at command line
                        PrintUserCommand();
                    }
                    else
                    {
                        if ((userKey.Modifiers & ConsoleModifiers.Shift) != 0)
                        {
                            NewCommandText = NewCommandText + userKey.KeyChar;
                            Console.Write(userKey.KeyChar);
                        }
                        else
                        {
                            NewCommandText = NewCommandText + userKey.KeyChar.ToString().ToLower();
                            Console.Write(userKey.KeyChar.ToString().ToLower());
                        }

                        PrintUserCommand();
                    }                   
                }
            }
        }


        private void PrintSingleKeyCommands()
        {
            Console.SetCursorPosition(1, ConsoleHeight - 6);

            //80 = all symbols lenght
            int padding = (ConsoleWidth - 80) / 8; // delimeter between [F] text
            if (padding < 0)
                padding = 0;

            Console.Write("[F1 Help]" + new string(' ', padding));
            Console.Write("[F3 Info]" + new string(' ', padding));
            Console.Write("[F5 Copy]" + new string(' ', padding));
            Console.Write("[F6 Move]" + new string(' ', padding));
            Console.Write("[F7 NewDir]" + new string(' ', padding));
            Console.Write("[F8 Del]" + new string(' ', padding));
            Console.Write("[F9 Rename]");
            try
            {
                Console.SetCursorPosition(ConsoleWidth - 14, ConsoleHeight - 6);
            }
            catch (System.ArgumentOutOfRangeException)
            {
                Console.SetCursorPosition(0, 0);
            }            
            Console.Write("[Alt F4 Exit]");
        }

        private void PrintUserCommand()
        {
            //string info = "";

            if (NewCommandText.Length > 0)
            {
                Actions newUserAction = new Actions(Active, Passive, ConsoleWidth);
                newUserAction.AnalizeCommand(NewCommandText, false);
            }

            //Console.SetCursorPosition(1, ConsoleHeight - 4);
            //Console.Write("Info: " + info);
            Console.SetCursorPosition(1, ConsoleHeight - 3);
            Console.Write("Command:" + NewCommandText);

            int cursor = Console.CursorLeft;
            int pad = ConsoleWidth - (NewCommandText.Length + 10);
            if (pad < 0)
                pad = 0;
            Console.Write("*".PadRight(pad, '*'));
            Console.CursorLeft = cursor;
        }
    }
}
