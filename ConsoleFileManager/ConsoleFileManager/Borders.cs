﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleFileManager
{
    public class Borders
    {
        public int BorderWidth { get; set; }
        public int BorderHeight { get; set; }


        char lineHoriz = '═';
        char lineVert = '║';

        //type 1
        char cornerTopRight = '╗';
        char cornerTopLeft = '╔';

        //type 2
        char cornerBottomRight = '╝';
        char cornerBottomLeft = '╚';

        //type 3
        char connertorLeft = '╠';
        char connertorRight = '╣';

        //type 4
        char connertorTop = '╦';
        char connertorBottom = '╩';
        
        //type 5 - no connectors

        public void PrintBorders()
        {
            //top 
            PrintHorizontalLine(0, BorderWidth - 1, 0, 1);
            PrintHorizontalLine(0, BorderWidth - 1, 3, 3);

            //bottom
            PrintHorizontalLine(0, BorderWidth - 1, BorderHeight - 7, 3);
            PrintHorizontalLine(0, BorderWidth - 1, BorderHeight - 5, 3);
            PrintHorizontalLine(0, BorderWidth - 1, BorderHeight - 2, 2);

            //left
            PrintVerticalLine(1, 3, 0, 5);
            PrintVerticalLine(4, BorderHeight - 7, 0, 5);
            
            PrintVerticalLine(BorderHeight - 6, BorderHeight - 5, 0, 5);
            PrintVerticalLine(BorderHeight - 4, BorderHeight - 2, 0, 5);

            //right
            PrintVerticalLine(1, 3, BorderWidth - 1, 5);
            PrintVerticalLine(4, BorderHeight - 7, BorderWidth - 1, 5);
            
            PrintVerticalLine(BorderHeight - 6, BorderHeight - 5, BorderWidth - 1, 5);
            PrintVerticalLine(BorderHeight - 4, BorderHeight - 2, BorderWidth - 1, 5);

            //middle
            PrintVerticalLine(0, BorderHeight - 7, BorderWidth / 2 - 1, 4);

            //set cross
            int cursorX = BorderWidth / 2 - 1;
            if (cursorX < 0)
                cursorX = 0;

            ClassLibrary.Do.SetCursorPosition(cursorX, 3);
            Console.Write("╬");
        }

        public void PrintVerticalLine(int startY, int finishY, int x, int connectorType)
        {
            bool exeption = ClassLibrary.Do.SetCursorPosition(x, startY);
            int shift = 0; // shift y in case of connectors absent

            if (exeption == false)
            {
                if (connectorType == 4)
                {
                    Console.WriteLine(connertorTop);
                    shift = 1;
                }
            }

            if (exeption == false)
            {
                for (int i = startY + shift; i < finishY; i++)
                {
                    exeption = ClassLibrary.Do.SetCursorPosition(x, i);
                    if (exeption)
                    {
                        break;
                    }
                    Console.WriteLine(lineVert);
                }

                if (connectorType == 4)
                {
                    ClassLibrary.Do.SetCursorPosition(x, finishY);
                    Console.WriteLine(connertorBottom);
                }
            }
        }

        public void PrintHorizontalLine(int startX, int finishX, int y, int connectorType)
        {
            bool exeption = ClassLibrary.Do.SetCursorPosition(finishX, y);
            if (exeption == false)
            {
                if (connectorType == 1)
                    Console.WriteLine(cornerTopRight);
                if (connectorType == 2)
                    Console.WriteLine(cornerBottomRight);
                if (connectorType == 3)
                    Console.WriteLine(connertorRight);
            }

            exeption = ClassLibrary.Do.SetCursorPosition(startX, y);
            if (exeption == false)
            {
                if (connectorType == 1)
                    Console.WriteLine(cornerTopLeft);
                if (connectorType == 2)
                    Console.WriteLine(cornerBottomLeft);
                if (connectorType == 3)
                    Console.WriteLine(connertorLeft);
                ClassLibrary.Do.SetCursorPosition(startX + 1, y);
            }

            if (exeption == false)
                Console.WriteLine(lineHoriz.ToString().PadLeft(finishX - startX - 1, lineHoriz));
        }
    }
}
