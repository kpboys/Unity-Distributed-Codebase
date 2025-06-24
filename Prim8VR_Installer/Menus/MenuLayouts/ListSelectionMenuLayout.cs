using Prim8VR_Installer.Helpers;
using SubWindowSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prim8VR_Installer.Menus.MenuTypes
{
    public class ListSelectionMenuLayout : BasicMenuLayout
    {
        private class SelectionEntry
        {
            public string Name { get; set; }
            public Action Callback { get; set; }
            public string DisplayName { get; set; }
        }
        int indexer = 0;
        List<SelectionEntry> options; //Not a Dictionary because we might need duplicate entries
        Action<string>? debugLogCB;
        public SubWindowWithListSelection? parentWindow;
        private const string spacerCode = "<SPACER>";
        public ListSelectionMenuLayout()
        {
            options = new List<SelectionEntry>();
        }
        public void AddDebugLogCallback(Action<string> callback)
        {
            debugLogCB = callback;
        }
        public void AddOption(string name, Action callback, int index = -1)
        {
            if (index == -1)
            {
                options.Add(new SelectionEntry() { Name = name, Callback = callback, DisplayName = name });
            }
            else if (index < options.Count)
            {
                options.Insert(index, new SelectionEntry() { Name = name, Callback = callback, DisplayName = name });
            }
            else if (debugLogCB != null)
            {
                debugLogCB.Invoke("Error! Index outside selection range");
                return;
            }
            RenderSelection();
        }
        public void AddSpacer(int index = -1)
        {
            AddOption(spacerCode, null, index);
        }
        //TODO: Figure out how this should work with duplicate names... (Index based version maybe?)
        public void ChangeOptionName(string name, string newName)
        {
            for (int i = 0; i < options.Count; i++)
            {
                if (options[i].Name == name)
                {
                    options[i].Name = newName;
                }
            }
            RenderSelection();
        }
        public void ChangeOptionDisplayName(string name, string decoratedVersion)
        {
            for (int i = 0; i < options.Count; i++)
            {
                if (options[i].Name == name)
                {
                    options[i].DisplayName = decoratedVersion;
                }
            }
            RenderSelection();
        }
        public Action InputLoop()
        {
            bool selected = false;
            while (selected == false)
            {
                RenderSelection();
                ConsoleKey key = InputHelper.ReadKey(true).Key;
                if (key == ConsoleKey.Spacebar)
                {
                    selected = true;
                }
                else if (key == ConsoleKey.UpArrow)
                {
                    int startIndex = indexer;
                    do
                    {
                        indexer--;
                        indexer = (int)RealMod(indexer, options.Count);
                        if (indexer == startIndex)
                        {
                            debugLogCB?.Invoke("ERROR, List is only spacers");
                            return null;
                        }
                    } while (options[indexer].Name == spacerCode);
                }
                else if (key == ConsoleKey.DownArrow)
                {
                    int startIndex = indexer;
                    do
                    {
                        indexer++;
                        indexer = (int)RealMod(indexer, options.Count);
                        if (indexer == startIndex)
                        {
                            debugLogCB?.Invoke("ERROR, List is only spacers");
                            return null;
                        }
                    } while (options[indexer].Name == spacerCode);
                }
            }
            //parentWindow.ClearWholeWindow();
            return options[indexer].Callback;
        }
        private float RealMod(float a, float b)
        {
            return (float)(a - b * Math.Floor(a / b));
        }
        private void RenderSelection()
        {
            ResetLog();
            for (int i = 0; i < options.Count; i++)
            {
                string message = "";
                if (options[i].Name == spacerCode)
                {
                    WriteLine("", false); //Skip line for spacer
                    continue;
                }

                if (i == indexer)
                    message += "-> ";

                message += options[i].DisplayName;
                WriteLine(message, false);
            }
            RenderLog();
        }
    }
}
