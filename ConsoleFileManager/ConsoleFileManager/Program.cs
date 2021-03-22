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

            FilePanel filePanelLeft = new FilePanel(fm.StartDirectory, 1, fm.ConsoleWidth / 2 - 1);
            filePanelLeft.PanelHeight = fm.ConsoleHeight;
            //filePanel.PanelWidth = fm.ConsoleWidth / 2 - 1;
            filePanelLeft.ShowDirectoryContent(fm.StartDirectory);

            FilePanel filePanelRight = new FilePanel(fm.StartDirectory, fm.ConsoleWidth / 2 + 1, fm.ConsoleWidth - 1);
            filePanelRight.PanelHeight = fm.ConsoleHeight;
            //filePanel.PanelWidth = fm.ConsoleWidth / 2 - 1;
            filePanelRight.ShowDirectoryContent(fm.StartDirectory);

            fm.GetUserCommands(filePanelLeft, filePanelRight);
        }
    }
}
