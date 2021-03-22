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

            FilePanel filePanel = new FilePanel(fm.StartDirectory);
            filePanel.PanelHeight = fm.ConsoleHeight;
            filePanel.PanelWidth = fm.ConsoleWidth / 2 - 1;

            fm.GetUserCommands(filePanel);
        }
    }
}
