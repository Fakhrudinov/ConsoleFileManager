using System;
using System.IO;

namespace ClassLibrary
{
    public class Do
    {
        /// <summary>
        /// to shrink code to one line - set cursor and print
        /// </summary>
        /// <param name="text"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="lenght"></param>
        public static void PrintLinePanelText(string text, int x, int y, int lenght)
        {
            SetCursorPosition(x, y);
            Console.WriteLine(text.PadRight(lenght));
        }

        /// <summary>
        /// Show alert. text, xCursor = width
        /// </summary>
        /// <param name="alertText"></param>
        /// <param name="xCursor"></param>
        public static void ShowAlert(string alertText, int xCursor)
        {
            int cursorX = (xCursor) / 2;
            int lineNumber = 10;
            string actionName = "Error when execute!";

            //header
            SetCursorPosition(cursorX, lineNumber);
            Console.BackgroundColor = ConsoleColor.Red;
            int padding = (cursorX) + (actionName.Length / 2);
            Console.Write(actionName.PadRight(padding).PadLeft(xCursor));

            int charPointer = 0;
            int charLenght = (xCursor) - 2;
            while (charPointer < alertText.Length)
            {
                SetCursorPosition(cursorX, ++lineNumber);
                if (charPointer + charLenght > alertText.Length)
                {
                    Console.Write(" " + alertText.Substring(charPointer).PadRight(xCursor - 2) + " ");
                }
                else
                {
                    Console.Write(" " + alertText.Substring(charPointer, charLenght) + " ");
                }

                charPointer = charPointer + charLenght;
            }

            //footer
            SetCursorPosition(cursorX, ++lineNumber);
            Console.Write(" ".PadRight(xCursor));
            SetCursorPosition(cursorX, ++lineNumber);
            Console.Write(" Press Enter to close alert.".PadRight(xCursor));

            SetCursorPosition(cursorX + 29, lineNumber);
            Console.ReadLine();

            Console.BackgroundColor = ConsoleColor.Black;

            //save info to file
            WriteAlertToFile(alertText);
        }

        /// <summary>
        /// split long (text > maxLenght) like 123..890
        /// </summary>
        /// <param name="text"></param>
        /// <param name="maxLenght"></param>
        /// <returns></returns>
        public static string TextLineCutter(string text, int maxLenght)
        {
            if (text.Length > maxLenght)
            {
                int even = maxLenght % 2;
                string text1 = text.Substring(0, maxLenght / 2 + even);
                if (text.Length - maxLenght == 1)
                {
                    text1 = text1 + ".";
                }
                if (text.Length - maxLenght > 1)
                {
                    text1 = text1.Substring(0, text1.Length - 1) + "..";
                }
                text = text1 + text.Substring(text.Length - (maxLenght / 2) + 1);
            }
            else
            {
                text = text.PadRight(maxLenght);
            }

            return text;
        }

        private static void CheckCreateDirectory(string dirName)
        {
            //check update temp dir exist/create
            DirectoryInfo dir = new DirectoryInfo(dirName);
            if (!dir.Exists)
            {
                try
                {
                    dir.Create();
                }
                catch (Exception)
                {
                    // nothing here - or errors became to looping alert
                }
            }
        }

        private static void WriteAlertToFile(string alertText)
        {
            //Check directory exist
            string dirName = "Log";
            CheckCreateDirectory(dirName);

            DateTime dt = DateTime.Now;
            string fileName = "Alerts_" + dt.ToString("yyyy_MMM_dd") + ".log";
            try
            {
                DirectoryInfo dir = new DirectoryInfo(Path.Combine(dirName, fileName));
                string str = dir.FullName;
                File.AppendAllLines(str, new[] { dt.ToString("hh:mm:ss"), alertText, "" });
            }
            catch
            {
                // nothing here - or errors became to looping alert
            }
        }

        public static void WriteCommandToFile(string newCommandText)
        {
            string fileName = ".\\commandHystory.log";

            //add new command to file
            WriteFile(fileName, newCommandText);
        }

        private static void WriteFile(string fileName, string text)
        {
            int maxLineCount = 20;
            string[] linesActual = File.ReadAllLines(fileName);

            if (maxLineCount <= linesActual.Length)
            {
                string allIn = text + Environment.NewLine;

                for (int i = linesActual.Length - 1; i > linesActual.Length - maxLineCount; i--)
                {
                    allIn = linesActual[i] + Environment.NewLine + allIn;
                }
                
                try
                {
                    File.WriteAllText(fileName, allIn);
                }
                catch (Exception e)
                {
                    ShowAlert($"Save used command to {fileName} failed. " + e.Message, 10);
                }
            }
            else //  just add new line
            {
                try
                {
                    File.AppendAllLines(fileName, new[] { text });
                }
                catch (Exception e)
                {
                    ShowAlert($"Save used command to {fileName} failed. " + e.Message, 10);
                }
            }
        }

        public static bool SetCursorPosition(int x, int y)
        {
            try
            {
                Console.SetCursorPosition(x, y);
            }
            catch (ArgumentOutOfRangeException)
            {
                return true;
            }

            return false;
        }
    }
}
