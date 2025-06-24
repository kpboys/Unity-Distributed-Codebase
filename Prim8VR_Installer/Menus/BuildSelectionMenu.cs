using Prim8VR_Installer.Behaviors;
using Prim8VR_Installer.Menus.MenuTypes;
using SubWindowSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prim8VR_Installer.Menus
{
    public class BuildSelectionMenu : IMenu
    {
        private ListSelectionMenuLayout layout;
        private BuildHandler buildHandler;

        private bool running;
        private List<string> selectedBuildNames;
        public BuildSelectionMenu(BuildHandler buildHandler)
        {
            this.buildHandler = buildHandler;
            selectedBuildNames = new List<string>();
        }
        public void Start()
        {
            //Set up list window
            List<string> builds = buildHandler.GetAvailableBuildNames();

            layout = new ListSelectionMenuLayout();
            layout.AddDebugLogCallback((x) => Program.WriteInDebug(x)); //add debug
            for (int i = 0; i < builds.Count; i++)
            {
                string buildName = builds[i];
                layout.AddOption(buildName, () => { SelectBuild(buildName); });
            }

            //Install button
            layout.AddSpacer();
            layout.AddOption("Install selected", () =>
            {
                bool success = buildHandler.InstallBuilds(selectedBuildNames);
                if (success)
                {
                    Program.PopupMessage("Successfully installed all builds!");
                }
                else
                {
                    Program.PopupMessage("Got to build: " + buildHandler.lastFailedBuildName + " where it failed to install.");
                }
            });

            //Exit button
            layout.AddSpacer();
            layout.AddOption("Return to main menu", () =>
            {
                running = false;
                MenuHandler.Instance.ChangeState(MenuHandler.MenuStates.MainMenu);
            });

            //Start actual execution
            RunningLoop();
        }
        private void RunningLoop()
        {
            running = true;
            while (running)
            {
                layout.InputLoop().Invoke();
            }
            Program.WriteInDebug("BuildSelectionMenu exited");
            layout.Clear();
        }
        private void SelectBuild(string buildName)
        {
            if (selectedBuildNames.Contains(buildName))
            {
                selectedBuildNames.Remove(buildName);
                layout.ChangeOptionDisplayName(buildName, buildName); //Remove decoration
            }
            else
            {
                selectedBuildNames.Add(buildName);
                layout.ChangeOptionDisplayName(buildName, GetDecoratedBuildName(buildName));
            }
        }
        private string GetDecoratedBuildName(string buildName)
        {
            return "[" + buildName + "]";
        }
    }
}
