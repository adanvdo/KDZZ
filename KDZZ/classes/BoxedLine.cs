using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KDZZ
{
    public class BoxedLine
    {
        private string[] textLines;
        private int boxWidth;
        private ConsoleColor foreColor;
        private ConsoleColor backColor;
        private bool drawTop;
        private bool drawBottom;

        public BoxedLine(string[] text, ConsoleColor backColor, ConsoleColor foreColor, bool drawTopBorder, bool drawBottomBorder) :
            this(text, -1, backColor, foreColor, drawTopBorder, drawBottomBorder) 
        {
            int top = 0;
            foreach (string s in text)
            {
                if (s.Length > top)
                    top = s.Length;
            }
            boxWidth = top + 2;
        }

        public BoxedLine(string[] text, int width, ConsoleColor backColor, ConsoleColor foreColor, bool drawTopBorder, bool drawBottomBorder)
        {
            textLines = text;
            boxWidth = width;
            drawTop = drawTopBorder;
            drawBottom = drawBottomBorder;
            this.foreColor = foreColor;
            this.backColor = backColor;
        }

        public static void DrawBoxedLine(string[] text, int width, ConsoleColor backColor, ConsoleColor foreColor, bool centerText, bool drawTopBorder, bool drawBottomBorder)
        {
            int rows = text.Length;
            BoxedLine box = new BoxedLine(text, width, backColor, foreColor, drawTopBorder, drawBottomBorder);
            List<string> boxified = new List<string>();

            if (drawTopBorder)
            {
                string border = " ┌";

                for (int w = 0; w < box.boxWidth; w++)
                {
                    border += "─";
                }
                border += "┐ ";
                boxified.Add(border);
            }

            for (int i = 0; i < rows; i++)
            {
                string line = " │ ";
                if (text[i].Length < box.boxWidth)
                {
                    if (centerText)
                    {
                        int d = box.boxWidth - text[i].Length;
                        if (d % 2 != 0)
                            d--;
                        int p = d / 2;
                        for (int spc = p; spc > 0; spc--)
                        {
                            line += " ";
                        }
                        line += text[i];
                        for (int spc = p; spc > 0; spc--)
                        {
                            line += " ";
                        }
                    }
                    else
                    {
                        line += text[i];
                        for (int spc = box.boxWidth - (text[i].Length +1); spc > 0; spc--)
                        {
                            line += " ";
                        }
                    }
                    line += "│ ";
                }
                else line += text[i] + " │ ";
                boxified.Add(line);
            }

            if (drawBottomBorder)
            {
                string border = " └";
                for (int w = 0; w < box.boxWidth; w++)
                {
                    border += "─";
                }
                border += "┘ ";
                boxified.Add(border);
            }

            foreach (string s in boxified)
            {
                Console.BackgroundColor = box.backColor;
                Console.ForegroundColor = box.foreColor;
                Console.WriteLine(s);
            }
        }
        
    }
}
