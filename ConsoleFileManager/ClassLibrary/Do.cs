using System;

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
            ClassLibrary.Do.SetCursorPosition(x, y);
            Console.WriteLine(text.PadRight(lenght));
        }

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
            int charPointerEnd = (xCursor) - 2;
            while (charPointer < alertText.Length)
            {
                SetCursorPosition(cursorX, ++lineNumber);
                if (charPointerEnd > alertText.Length)
                {
                    Console.WriteLine(" " + alertText.Substring(charPointer).PadRight(xCursor - 2) + " ");
                }
                else
                {
                    Console.WriteLine(" " + alertText.Substring(charPointer, charPointerEnd) + " ");
                }

                charPointer = charPointerEnd;
                charPointerEnd = charPointer + charPointerEnd;
            }

            //footer
            SetCursorPosition(cursorX, ++lineNumber);
            Console.Write(" ".PadRight(xCursor));
            SetCursorPosition(cursorX, ++lineNumber);
            Console.Write(" Press Enter to close alert.".PadRight(xCursor));

            SetCursorPosition(cursorX + 29, lineNumber);
            Console.ReadLine();

            Console.BackgroundColor = ConsoleColor.Black;
        }

        public static bool SetCursorPosition(int x, int y)
        {
            try
            {
                Console.SetCursorPosition(x, y);
            }
            catch (System.ArgumentOutOfRangeException)
            {
                return true;
            }

            return false;
        }
    }
}
