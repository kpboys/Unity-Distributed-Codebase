using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubWindowSystem
{
    public class LoadingBar : SubWindow
    {
        private char filledChar;
        private char emptyChar;
        public LoadingBar(Rectangle area, char borderChar, int borderSpacing, bool renderImmediately = true) : base(area, borderChar, borderSpacing, renderImmediately)
        {

        }
        public LoadingBar SetupBarDesign(char filledChar, char emptyChar)
        {
            this.filledChar = filledChar;
            this.emptyChar = emptyChar;
            return this;
        }
        public void SetPercentage(float percent)
        {
            if (percent < 0 || percent > 1) return; //must be from 0 to 1

            int filledCharCount = (int)MathF.Round(area.Width * percent);
            string filledLine = "";
            for (int i = 0; i < area.Width; i++)
            {
                if(i < filledCharCount)
                    filledLine += filledChar;
                else
                    filledLine += emptyChar;
            }
            for (int i = 0; i < area.Height; i++)
            {
                WriteLine(filledLine, false);
            }
            RenderLog();
        }
    }
}
