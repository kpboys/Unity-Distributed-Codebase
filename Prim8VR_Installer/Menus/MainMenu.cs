using Prim8VR_Installer.Menus.MenuTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prim8VR_Installer.Menus
{
    public class MainMenu : IMenu
    {
        private ListSelectionMenuLayout layout;
        public MainMenu(ListSelectionMenuLayout layout)
        {
            this.layout = layout;
        }

        public void Start()
        {
            layout.InputLoop().Invoke();
        }
    }
}
