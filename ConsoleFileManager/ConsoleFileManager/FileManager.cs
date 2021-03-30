using System;

namespace ConsoleFileManager
{
    public class FileManager
    {
        public string StartDirectoryLeft { get; set; }

        public string StartDirectoryRight { get; set; }

        FilePanel Active { get; set; }
        FilePanel Passive { get; set; }

        string newCommandText;
        string NewCommandText 
        {
            get
            {
                return newCommandText;
            }
            set
            {
                newCommandText = value;
            }
        }

        public int ConsoleWidth { get; set; }
        public int ConsoleHeight { get; set; }

        public FileManager()
        {
            DataXML xml = new DataXML();
            xml.GetDataFromXML();

            ConsoleWidth = xml.XMLConsoleWidth;
            ConsoleHeight = xml.XMLConsoleHeight;
            StartDirectoryLeft = xml.XMLStartDirectoryLeft;
            StartDirectoryRight = xml.XMLStartDirectoryRight;
            NewCommandText = "";

            Console.SetWindowSize(ConsoleWidth, ConsoleHeight);
            //Console.SetBufferSize(Console.LargestWindowWidth, Console.LargestWindowHeight);

            Borders border = new Borders();
            FilePanel filePanelLeft = new FilePanel(StartDirectoryLeft, 1, ConsoleWidth / 2 - 1);
            filePanelLeft.CurrentItem = 50; // читать из XML ------------------------------------------------------------------------
            filePanelLeft.IsActive = true; // читать из XML ------------------------------------------------------------------------
            Active = filePanelLeft; // читать из XML ------------------------------------------------------------------------

            FilePanel filePanelRight = new FilePanel(StartDirectoryRight, ConsoleWidth / 2, ConsoleWidth - 1);
            filePanelRight.CurrentItem = 8; // читать из XML ------------------------------------------------------------------------
            filePanelRight.IsActive = false; // читать из XML ------------------------------------------------------------------------
            Passive = filePanelRight; // читать из XML ------------------------------------------------------------------------

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
                if (Console.WindowWidth != ConsoleWidth || Console.WindowHeight != ConsoleHeight)
                {
                    Console.Clear();
                    //Console.SetBufferSize(Console.LargestWindowWidth, Console.LargestWindowHeight);

                    ConsoleWidth = Console.WindowWidth;
                    ConsoleHeight = Console.WindowHeight;

                    PrintFileManager(filePanelLeft, filePanelRight, border);
                }

                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo userKey = Console.ReadKey(true);

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
                                break;
                            case ConsoleKey.DownArrow:
                                Active.ChangeCurrentItem(1);                                
                                break;
                            case ConsoleKey.UpArrow:
                                Active.ChangeCurrentItem(-1);                                
                                break;
                            case ConsoleKey.PageDown:
                                Active.ChangeCurrentItem(100);                                
                                break;
                            case ConsoleKey.PageUp:
                                Active.ChangeCurrentItem(-100);                                
                                break;
                            case ConsoleKey.Home:
                                Active.ChangeCurrentItem(-1000);                                
                                break;
                            case ConsoleKey.End:
                                Active.ChangeCurrentItem(1000);                                
                                break;
                            case ConsoleKey.F2:
                                Active.RenameItem("Rename");
                                PrintFileManager(filePanelLeft, filePanelRight, border);
                                break;
                            case ConsoleKey.F5:
                                isConfirmed = Active.UserConfirmAction("Copy", Passive.StartDirectory);
                                if (isConfirmed)
                                    Active.CopyItemTo(Active.StartDirectory, Passive.StartDirectory);
                                PrintFileManager(filePanelLeft, filePanelRight, border);
                                break;
                            case ConsoleKey.F6:
                                isConfirmed = Active.UserConfirmAction("Move", Passive.StartDirectory);
                                if(isConfirmed)
                                    Active.MoveItemTo(Passive.StartDirectory);
                                PrintFileManager(filePanelLeft, filePanelRight, border);
                                break;
                            case ConsoleKey.F7:
                                Active.CreateNewDir("Create New Directory");
                                PrintFileManager(filePanelLeft, filePanelRight, border);
                                break;
                            case ConsoleKey.F8:
                                isConfirmed = Active.UserConfirmAction("Delete", Passive.StartDirectory);
                                if (isConfirmed)
                                    Active.DeleteItem(Passive.StartDirectory);
                                PrintFileManager(filePanelLeft, filePanelRight, border);
                                break;

                            case ConsoleKey.Enter:
                                if (NewCommandText.Length > 0)
                                {
                                    Console.Write("Execute command " + NewCommandText);
                                    NewCommandText = "";
                                }
                                else
                                {
                                    Active.ExecuteCurrent();
                                }
                                //Console.Clear();
                                //PrintFileManager(filePanelLeft, filePanelRight, border);
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
                    }                   
                }
            }
        }


        private void PrintSingleKeyCommands()
        {
            Console.SetCursorPosition(1, ConsoleHeight - 6);

            int padding = ConsoleWidth  / 7; // delimeter = amount of all commands

            Console.Write("[F1 Help]".PadRight(padding));
            Console.Write("[F2 Rename]".PadRight(padding));
            Console.Write("[F5 Copy]".PadRight(padding));
            Console.Write("[F6 Move]".PadRight(padding));
            Console.Write("[F7 NewDir]".PadRight(padding));
            Console.Write("[F8 Del]");

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
            Console.SetCursorPosition(1, ConsoleHeight - 4);
            Console.Write("Info: " );
            Console.SetCursorPosition(1, ConsoleHeight - 3);
            Console.Write("Command: " + NewCommandText);
        }
    }
}
