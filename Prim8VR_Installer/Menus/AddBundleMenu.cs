using Prim8VR_Installer.Helpers;
using Prim8VR_Installer.Menus.MenuTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prim8VR_Installer.Menus
{
    public class AddBundleMenu : IMenu
    {
        private BasicMenuLayout layout;
        private bool running;
        public AddBundleMenu(BasicMenuLayout layout)
        {
            this.layout = layout;
        }
        public void Start()
        {
            running = true;
            while (running)
            {
                layout.Clear();
                layout.WriteLine("Please paste the link to the bundle repo you wish to add");
                string input = InputHelper.ReadLine();

                //Interact with BundleHandler here

                running = false;
                MenuHandler.Instance.ChangeState(MenuHandler.MenuStates.MainMenu);
            }
        }
    }
}
