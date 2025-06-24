using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prim8VR_Installer.Menus.MenuTypes
{
    public class BasicMenuLayout
    {
        internal List<string> log = new List<string>();

        public void WriteLine(string msg, bool renderImmediately = true)
        {
            log.Add(msg);
            if (renderImmediately)
                RenderLog();
        }
        public void RenderLog()
        {
            Console.Clear();
            for (int i = 0; i < log.Count; i++)
            {
                Console.WriteLine(log[i]);
            }
        }
        public void Clear(bool withLog = false)
        {
            Console.Clear();
            if(withLog)
                ResetLog();
        }
        public void ResetLog()
        {
            log.Clear();
        }
    }
}
