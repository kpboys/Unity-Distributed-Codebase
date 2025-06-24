using Prim8VR_Installer.Helpers;
using Prim8VR_Installer.Menus.MenuTypes;

namespace Prim8VR_Installer.Menus
{
    public class AddProjectMenu : IMenu
    {
        private BasicMenuLayout layout;
        private bool running;
        public AddProjectMenu(BasicMenuLayout layout)
        {
            //Add ProjectHandler here
            this.layout = layout;
        }
        public void Start()
        {
            running = true;
            while (running)
            {
                layout.Clear();
                layout.WriteLine("Please paste the link to the project repo you wish to add");
                string input = InputHelper.ReadLine();
                //
                //Give input to ProjectHandler here
                //
                running = false;
                MenuHandler.Instance.ChangeState(MenuHandler.MenuStates.MainMenu);
            }
        }
    }
}
