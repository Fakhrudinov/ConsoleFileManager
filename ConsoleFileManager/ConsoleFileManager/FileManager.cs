using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ConsoleFileManager
{
    public class FileManager
    {
        public string StartDirectoryLeft { get; set; }

        public string StartDirectoryRight { get; set; }

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

            Console.SetWindowSize(ConsoleWidth, ConsoleHeight);
            //Console.SetBufferSize(Console.LargestWindowWidth, Console.LargestWindowHeight);

            Borders border = new Borders();
            FilePanel filePanelLeft = new FilePanel(StartDirectoryLeft, 1, ConsoleWidth / 2 - 1);
            filePanelLeft.PaginationStart = 0;
            
            FilePanel filePanelRight = new FilePanel(StartDirectoryRight, ConsoleWidth / 2, ConsoleWidth - 1);
            filePanelRight.PaginationStart = 0;
            
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
            filePanelLeft.ShowDirectoryContent(StartDirectoryLeft);

            filePanelRight.PanelHeight = ConsoleHeight;
            filePanelRight.FromX = ConsoleWidth / 2;
            filePanelRight.UntilX = ConsoleWidth - 1;
            filePanelRight.ShowDirectoryContent(StartDirectoryRight);

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
                        switch (userKey.Key)
                        {
                            case ConsoleKey.Tab:
                                Console.Write("tab? " + userKey.Key);
                                break;
                            case ConsoleKey.Enter:
                                Console.Write("Execute command " + NewCommandText);
                                NewCommandText = "";
                                Console.Clear();
                                PrintFileManager(filePanelLeft, filePanelRight, border);
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

            int padding = ConsoleWidth  / 9; // delimeter = to 1 more than actual command amount. Just for good look

            Console.Write("[F1 Help]".PadRight(padding));
            Console.Write("[F3 View]".PadRight(padding));
            Console.Write("[F4 Edit]".PadRight(padding));
            Console.Write("[F5 Copy]".PadRight(padding));
            Console.Write("[F6 Move]".PadRight(padding));
            Console.Write("[F7 MkDir]".PadRight(padding));
            Console.Write("[F8 Del]");

            try
            {
                Console.SetCursorPosition(ConsoleWidth - 14, ConsoleHeight - 6);
            }
            catch (System.ArgumentOutOfRangeException)
            {
                Console.SetCursorPosition(0, 0);
            }            
            Console.Write("[Alt F4 Exit]"); // lenght = 13
        }

        private void PrintUserCommand()
        {
            Console.SetCursorPosition(1, ConsoleHeight - 4);
            Console.Write("Info: " + NewCommandText);
            Console.SetCursorPosition(1, ConsoleHeight - 3);
            Console.Write("Command: " + NewCommandText);
        }
    }
}
