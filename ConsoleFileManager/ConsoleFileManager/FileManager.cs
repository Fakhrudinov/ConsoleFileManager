using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ConsoleFileManager
{
    public class FileManager
    {
        public string StartDirectory { get; set; }

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
            StartDirectory = xml.XMLStartDirectory;

            //Console.CursorVisible = false;
            Console.SetWindowSize(ConsoleWidth, ConsoleHeight);
            Console.SetBufferSize(ConsoleWidth, ConsoleHeight);

            //FilePanel filePanel = new FilePanel(StartDirectory);
            //filePanel.PanelHeight = ConsoleHeight;
            //filePanel.PanelWidth = ConsoleWidth / 2 - 1;

            //filePanel.ShowDirectoryContent(StartDirectory);
            //ShowDirAndFiles();

        }

        public void GetUserCommands(FilePanel filePanelLeft, FilePanel filePanelRight)
        {
            bool exit = false;
            while (!exit)
            {
                if (Console.WindowWidth != ConsoleWidth)
                {
                    ConsoleWidth = Console.WindowWidth;
                    Console.Clear();

                    //filePanelLeft.PanelWidth = ConsoleWidth / 2 - 1;
                    filePanelLeft.UntilX = ConsoleWidth / 2 - 1;
                    filePanelLeft.ShowDirectoryContent(StartDirectory);

                    filePanelRight.FromX = ConsoleWidth / 2 + 1;
                    filePanelRight.UntilX = ConsoleWidth - 1;
                    filePanelRight.ShowDirectoryContent(StartDirectory);
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
    }
}
