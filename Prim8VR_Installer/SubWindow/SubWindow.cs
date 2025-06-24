using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubWindowSystem
{
    /// <summary>
    /// Class for making "windows" in the console and easily writing in them. 
    /// This lets you have multiple areas to write in in one window.
    /// </summary>
    public class SubWindow
    {
        internal List<string> log;
        internal Rectangle area;
        private Point cursorPos;
        internal string clearLine;
        private bool drawBorder;
        internal char borderChar;
        internal int borderSpacing;

        private Object writeLock = new Object();
        private static Object cursorLock = new Object();

        internal bool old_CouldDraw = true;

        public SubWindow(Rectangle area)
        {
            BaseSetup(area);
            drawBorder = false;
        }
        public SubWindow(Rectangle area, char borderChar, int borderSpacing, bool renderImmediately = true)
        {
            BaseSetup(area);
            drawBorder = true;
            this.borderChar = borderChar;
            this.borderSpacing = borderSpacing;

            if(renderImmediately) 
                DrawBorder(this.area, this.borderChar, this.borderSpacing);
        }
        private void BaseSetup(Rectangle area)
        {
            log = new List<string>();
            this.area = area;
            cursorPos = new Point(area.X, area.Y);
            clearLine = "";
            for (int i = 0; i < area.Width; i++)
            {
                clearLine += " ";
            }
        }
        internal bool CheckIfCanRender()
        {
            bool canDraw = area.X + area.Width + borderSpacing < Console.WindowWidth
                && area.Y + area.Height + borderSpacing < Console.WindowHeight;
            //bool canDraw = area.X + area.Width + (borderSpacing * 2) < Console.WindowWidth
            //    && area.Y + area.Height + (borderSpacing * 2) < Console.WindowHeight;
            if(old_CouldDraw != canDraw)
            {
                old_CouldDraw = canDraw;
                MoveWindow(area.X, area.Y);
            }
            old_CouldDraw = canDraw;
            return canDraw;
        }
        internal void DrawBorder(Rectangle area, char borderChar, int borderSpacing)
        {
            if (CheckIfCanRender() == false) return; 

            (int, int) startPos = Console.GetCursorPosition();
            DrawHorizontal(area.X - 1 - borderSpacing, area.Y - 1 - borderSpacing);
            DrawHorizontal(area.X - 1 - borderSpacing, area.Y + area.Height + borderSpacing);
            DrawVertical(area.X - 1 - borderSpacing, area.Y - 1 - borderSpacing);
            DrawVertical(area.X + area.Width + borderSpacing, area.Y - 1 - borderSpacing);

            Console.SetCursorPosition(startPos.Item1, startPos.Item2);

            //Methods
            void DrawHorizontal(int x, int y)
            {
                if (x > Console.WindowWidth) return;

                string line = "";
                for (int i = 0; i < area.Width + 2 + (borderSpacing * 2); i++)
                {
                    if(i + x < Console.WindowWidth)
                        line += borderChar;
                }
                Console.SetCursorPosition(x, y);
                Console.Write(line);
            }
            void DrawVertical(int x, int y)
            {
                if (x > Console.WindowWidth) return;

                int startY = y;
                for (int i = 0; i < area.Height + 2 + (borderSpacing * 2); i++)
                {
                    Console.SetCursorPosition(x, startY + i);
                    Console.Write(borderChar);
                }
            }
        }
        
        //public void WriteLine(string content)
        //{
        //    (int, int) startPos = Console.GetCursorPosition();
        //    int lineCount = 1;
        //    if(content.Length > area.Width)
        //    {
        //        lineCount = (int)MathF.Ceiling((float)content.Length / (float)area.Width);
        //    }
        //    for(int i = 0; i < lineCount; i++)
        //    {
        //        Console.SetCursorPosition(cursorPos.X, cursorPos.Y);
        //        char[] characters = content.ToCharArray();
        //        int remaining = characters.Length - area.Width * i;
        //        Console.Write(characters, area.Width * i, area.Width > remaining ? remaining : area.Width);
        //        cursorPos.Y++;
        //        if(cursorPos.Y > area.Y + area.Height)
        //        {
        //            Clear();
        //        }
        //    }
        //    Console.SetCursorPosition(startPos.Item1, startPos.Item2);
        //}
        public void WriteLine(string content, bool renderImmediately = true)
        {
            if (content == "") return;

            int lineCount = 1;
            if (content.Length > area.Width)
            {
                lineCount = (int)MathF.Ceiling((float)content.Length / (float)area.Width);
            }

            //Lock so we can handle multiple threads trying to write here
            lock (writeLock)
            {
                int textLength = 0;
                for (int i = 0; i < lineCount; i++)
                {
                    int remaining = content.Length - textLength;
                    //string nText = content.Substring(i * area.Width, area.Width > remaining ? remaining : area.Width);
                    string nText = content.Substring(textLength, area.Width > remaining ? remaining : area.Width);
                    if (nText.Contains("\n"))
                    {
                        nText = nText.Substring(0, nText.IndexOf("\n"));
                        textLength += 1; //Scoot it one index to not have the line shift on the next entry
                    }
                    if ((textLength + nText.Length) > content.Length)
                        textLength = content.Length;
                    else
                        textLength += nText.Length;

                    log.Add(nText);

                    //if (nText.Contains("\n")) //Path for if there are dividers in the Debug text
                    //{
                    //    List<string> splitText = nText.Split('\n').ToList();
                    //    for (int k = 0; k < splitText.Count; k++)
                    //    {
                    //        if (splitText[k].Length > area.Width)
                    //        {
                    //            //Grab the first half and add it to the log
                    //            string subSplitFirstHalf = splitText[k].Substring(0, area.Width);
                    //            log.Add(subSplitFirstHalf);
                    //            //Add the remainder to the list to be proccessed like the rest
                    //            string subSplitSecondHalf = splitText[k].Substring(area.Width);
                    //            splitText.Insert(i + 1, subSplitSecondHalf);
                    //        }
                    //        else
                    //        {
                    //            log.Add(splitText[k]);
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    log.Add(nText);
                    //}
                }
            }
            if(renderImmediately)
                RenderLog();
        }
        private (int, int) startPos;
        public void RenderLog()
        {
            if (CheckIfCanRender() == false) return;

            lock (cursorLock)
            {
                //Get where the cursor was before we do this
                (int, int) startPos = Console.GetCursorPosition();

                //Clear area before rendering
                ClearTextArea();

                int startYIndex = log.Count - area.Height;
                for (int i = 0; i < area.Height; i++)
                {
                    Console.SetCursorPosition(area.X, area.Y + i);
                    int logIndex = startYIndex + i;
                    if (logIndex >= 0)
                    {
                        Console.Write(log[logIndex]);
                    }
                }

                //Set cursor back where we found it :)
                Console.SetCursorPosition(startPos.Item1, startPos.Item2);
            }
        }
        public void ClearTextArea()
        {
            lock(cursorLock)
            {
                for (int i = 0; i < area.Height; i++)
                {
                    Console.SetCursorPosition(area.X, area.Y + i);
                    Console.Write(clearLine);
                }
                cursorPos = new Point(area.X, area.Y);
            }
        }
        public virtual void ClearWholeWindow()
        {
            lock (cursorLock)
            {
                //Get where the cursor was before we do this
                (int, int) startPos = Console.GetCursorPosition();

                string clearLine = "";
                for (int i = 0; i < area.Width + (borderSpacing + 1) * 2; i++)
                {
                    clearLine += " ";
                }
                int startY = area.Y - (borderSpacing + 1);
                int xPos = area.X - (borderSpacing + 1);
                for (int i = 0; i < area.Height + (borderSpacing + 1) * 2; i++)
                {
                    Console.SetCursorPosition(xPos, startY + i);
                    Console.Write(clearLine);
                }

                //Set cursor back where we found it :)
                Console.SetCursorPosition(startPos.Item1, startPos.Item2);
            }
        }
        public void ResetLog()
        {
            log.Clear();
        }
        public virtual void MoveWindow(int x, int y)
        {
            //Get where the cursor was before we do this
            (int, int) startPos = Console.GetCursorPosition();

            ClearWholeWindow();
            area.X = x;
            area.Y = y;
            DrawBorder(area, borderChar, borderSpacing);
            RenderLog();

            //Set cursor back where we found it :)
            Console.SetCursorPosition(startPos.Item1, startPos.Item2);
        }
        public virtual void RefreshWholeWindow()
        {
            ClearWholeWindow();
            DrawBorder(area, borderChar, borderSpacing);
            RenderLog();
        }
    }
}
