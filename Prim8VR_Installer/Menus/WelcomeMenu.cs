using Prim8VR_Installer.Menus.MenuTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prim8VR_Installer.Menus
{
    public class WelcomeMenu : IMenu
    {
        private BasicMenuLayout layout;
        public WelcomeMenu(BasicMenuLayout layout)
        {
            this.layout = layout;
        }
        public void Start()
        {
            layout.Clear();
            layout.WriteLine("Welcome to the Prim8 Installer");
            layout.WriteLine("Press any key to continue");
            MenuHandler.Instance.ChangeState(MenuHandler.MenuStates.MainMenu);
        }
    }
}
