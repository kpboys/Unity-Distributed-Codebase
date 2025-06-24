using Prim8VR_Installer.Behaviors;
using Prim8VR_Installer.Helpers;
using Prim8VR_Installer.Menus.MenuTypes;
using SubWindowSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prim8VR_Installer.Menus
{
    public class CheckDeviceMenu : IMenu
    {
        private BasicMenuLayout layout;
        private GetDevice deviceGetter;
        private bool running;
        public CheckDeviceMenu(BasicMenuLayout layout, GetDevice deviceGetter)
        {
            this.layout = layout;
            this.deviceGetter = deviceGetter;
        }
        //public async Task Start()
        public void Start()
        {
            layout.Clear(true);
            layout.WriteLine("Please connect a VR headset and press any key when you've done so.");
            InputHelper.ReadKey(true);

            running = true;
            while (running)
            {
                layout.Clear(true);
                layout.WriteLine("Checking for devices...");
                bool result = deviceGetter.DoCheck();
                if (result)
                {
                    layout.WriteLine("Device found:\n" + deviceGetter.TargetDevice.ToString());
                    layout.WriteLine("Press any key to continue...");
                    InputHelper.ReadKey(true);
                    running = false;
                    MenuHandler.Instance.ChangeState(MenuHandler.MenuStates.BuildSelection);
                }
                else
                {
                    layout.WriteLine("Could not get device. Plus ensure device is connected and authorized" +
                        "\nPress R to refresh and try again." +
                        "\n\nPress E to return to the main menu.");
                    ConsoleKey key = ConsoleKey.None;
                    while (key != ConsoleKey.R && running)
                    {
                        key = InputHelper.ReadKey(true).Key;
                        if (key == ConsoleKey.E)
                        {
                            running = false;
                            MenuHandler.Instance.ChangeState(MenuHandler.MenuStates.MainMenu);
                        }
                        //DEBUG
                        else if(key == ConsoleKey.O)
                        {
                            running = false;
                            MenuHandler.Instance.ChangeState(MenuHandler.MenuStates.BuildSelection);
                        }
                    }

                }
            }
            layout.Clear(true);
        }
    }
}
