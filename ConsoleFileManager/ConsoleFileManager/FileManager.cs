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

            GetUserCommands(filePanelLeft, filePanelRight);

            PrintUserCommand();
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

        private void PrintUserCommand()
        {
            Console.SetCursorPosition(1, ConsoleHeight - 2);
            Console.Write("Command: " + NewCommandText);
            //Console.SetCursorPosition(10, ConsoleHeight - 2);
        }
    }
}
