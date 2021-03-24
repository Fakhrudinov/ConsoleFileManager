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
            Console.SetBufferSize(ConsoleWidth, ConsoleHeight);

            FilePanel filePanelLeft = new FilePanel(StartDirectoryLeft, 1, ConsoleWidth / 2 - 1);
            filePanelLeft.PanelHeight = ConsoleHeight;
            filePanelLeft.ShowDirectoryContent(StartDirectoryLeft);

            FilePanel filePanelRight = new FilePanel(StartDirectoryRight, ConsoleWidth / 2 + 1, ConsoleWidth - 1);
            filePanelRight.PanelHeight = ConsoleHeight;
            filePanelRight.ShowDirectoryContent(StartDirectoryRight);

            PrintSingleKeyCommands(); // like F1 F2 etc.

            PrintUserCommand();

            GetUserCommands(filePanelLeft, filePanelRight);// Don't add any command after call this method - because endless while!
        }

        public void GetUserCommands(FilePanel filePanelLeft, FilePanel filePanelRight)
        {
            bool exit = false;
            while (!exit)
            {
                if (Console.WindowWidth != ConsoleWidth || Console.WindowHeight != ConsoleHeight)
                {
                    ConsoleWidth = Console.WindowWidth;
                    ConsoleHeight = Console.WindowHeight;
                    Console.Clear();

                    filePanelLeft.PanelHeight = ConsoleHeight;
                    filePanelLeft.UntilX = ConsoleWidth / 2 - 1;
                    filePanelLeft.ShowDirectoryContent(StartDirectoryLeft);

                    filePanelRight.PanelHeight = ConsoleHeight;
                    filePanelRight.FromX = ConsoleWidth / 2 + 1;
                    filePanelRight.UntilX = ConsoleWidth - 1;
                    filePanelRight.ShowDirectoryContent(StartDirectoryRight);
                    
                    PrintSingleKeyCommands();
                    PrintUserCommand();
                }

                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo userKey = Console.ReadKey(true);

                    if (Char.IsControl(userKey.KeyChar))
                    {
                        switch (userKey.Key)
                        {
                            case ConsoleKey.Tab:
                                Console.WriteLine("tab? " + userKey.Key);
                                break;
                            case ConsoleKey.Enter:
                                Console.WriteLine("Execute command " + NewCommandText);
                                NewCommandText = "";

                                Console.Clear();
                                filePanelLeft.ShowDirectoryContent(StartDirectoryLeft);
                                filePanelRight.ShowDirectoryContent(StartDirectoryRight);

                                PrintUserCommand();
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
            Console.SetCursorPosition(1, ConsoleHeight - 3);

            int padding = ConsoleWidth  / 9; // delimeter = to 1 more than actual command amount. Just for good look

            Console.Write("[F1 Help]".PadRight(padding));
            Console.Write("[F3 View]".PadRight(padding));
            Console.Write("[F4 Edit]".PadRight(padding));
            Console.Write("[F5 Copy]".PadRight(padding));
            Console.Write("[F6 Move]".PadRight(padding));
            Console.Write("[F7 MkDir]".PadRight(padding));
            Console.Write("[F8 Del]");

            Console.SetCursorPosition(ConsoleWidth - 14, ConsoleHeight - 3);
            Console.Write("[Alt F4 Exit]"); // lenght = 13
        }

        private void PrintUserCommand()
        {
            Console.SetCursorPosition(1, ConsoleHeight - 2);
            Console.Write("Command: " + NewCommandText);
        }
    }
}
