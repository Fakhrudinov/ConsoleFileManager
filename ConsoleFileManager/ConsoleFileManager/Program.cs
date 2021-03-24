using System;
using System.IO;

namespace ConsoleFileManager
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            * Alexander Fakhrudinov = Александр Фахрудинов
            * asbuka@gmail.com
            */

            FileManager fm = new FileManager();

            //FilePanel filePanelLeft = new FilePanel(fm.StartDirectoryLeft, 1, fm.ConsoleWidth / 2 - 1);
            //filePanelLeft.PanelHeight = fm.ConsoleHeight;
            //filePanelLeft.ShowDirectoryContent(fm.StartDirectoryLeft);

            //FilePanel filePanelRight = new FilePanel(fm.StartDirectoryRight, fm.ConsoleWidth / 2 + 1, fm.ConsoleWidth - 1);
            //filePanelRight.PanelHeight = fm.ConsoleHeight;
            //filePanelRight.ShowDirectoryContent(fm.StartDirectoryRight);

            //fm.GetUserCommands(filePanelLeft, filePanelRight);
        }
    }
}
