using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prim8VR_Installer.Menus
{
    public class MenuHandler
    {
        public enum MenuStates
        {
            Welcome,
            MainMenu,
            CheckDevice,
            BuildSelection,
            AddProject,
            AddBundle
        }
        //Singleton
        private static MenuHandler _instance;
        public static MenuHandler Instance { get { return _instance; } }

        //Control
        private MenuStates state;
        private bool running;

        //References
        private Dictionary<MenuStates, IMenu> menus;
        
        public MenuHandler()
        {
            state = MenuStates.Welcome;
            menus = new Dictionary<MenuStates, IMenu>();
            _instance = this;
        }
        /// <summary>
        /// Add a menu bound to a state
        /// </summary>
        /// <returns>Returns this for chaining</returns>
        public MenuHandler AddStateMenuBinding(MenuStates targetState, IMenu targetMenu)
        {
            if (menus.ContainsKey(targetState)) return this;

            menus.Add(targetState, targetMenu);
            return this;
        }
        public void MenuLoop()
        {
            running = true;
            while (running)
            {
                if(menus.ContainsKey(state) == false)
                {
                    state = MenuStates.MainMenu;
                } 
                menus[state].Start();
            }
        }
        public void ChangeState(MenuStates state)
        {
            this.state = state;
        }
        public void Stop()
        {
            running = false;
        }
    }
}
