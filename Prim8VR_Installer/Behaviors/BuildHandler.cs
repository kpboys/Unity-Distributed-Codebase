using Prim8VR_Installer.Helpers;
using Prim8VR_Installer.Interfaces;
using Prim8VR_Installer.JsonObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prim8VR_Installer.Behaviors
{
    public class BuildHandler
    {
        public IReleaseGatherer _releaseGatherer;

        public string lastFailedBuildName = "";

        private GeneralConfig config;
        private ExecuteCommandHelper commandHelper;
        private bool atAdbDirectory;
        //Dependency injection
        public BuildHandler(GeneralConfig config, IReleaseGatherer _releaseGatherer)
        {
            this.config = config;
            this._releaseGatherer = _releaseGatherer;
            atAdbDirectory = false;
        }
        //TODO: Handle build names not being unique. Make better method for deciding a Key for the Dictionary
        public void Initialize()
        {
            commandHelper = new ExecuteCommandHelper((x) => ProcessCommandOutput(x));
            commandHelper.Initialize();
        }
        //TODO: Make other methods to get other BuildData fields, for more information for the user
        public List<string> GetAvailableBuildNames()
        {
            List<string> result = _releaseGatherer.GetAvailableReleases();
            Program.WriteInDebug("Count: " + result.Count);
            return result;
        }

        public bool InstallBuild(string buildName)
        {
            bool success = false;
            //If we're not at the directory for ADB, cd to it
            if(atAdbDirectory == false)
            {
                success = commandHelper.DoCommand("cd " + config.AdbPath);
                if(success == false)
                {
                    Program.WriteInDebug("Failed to go to directory");
                    return false;
                }
                atAdbDirectory = true;
            }
            //Get filepath and add quotes to it if it doesn't have it
            string filePath = _releaseGatherer.GetFilePathOfRelease(buildName);
            if (filePath[0] != '"')
                filePath = "\"" + filePath;
            if (filePath[filePath.Length-1] != '"')
                filePath = filePath + "\"";

            //Do install command
            success = commandHelper.DoCommand("./adb install " + filePath);
            if (success == false)
            {
                Program.WriteInDebug("Failed to install");
                return false;
            }

            return true;
        }
        public bool InstallBuilds(List<string> targetBuilds)
        {
            bool success = true;
            for (int i = 0; i < targetBuilds.Count; i++)
            {
                //Progress showing stuff:
                float percent = (float)(i + 1) / (float)(targetBuilds.Count + 1);
                Program.WriteInDebug("Loading progress: " + percent.ToString());
                Program.PopupMessage($"Installing build \n>{targetBuilds[i]}<\nPlease wait...", false);
                Program.ShowLoading(percent);

                if (InstallBuild(targetBuilds[i]) == false)
                {
                    success = false;
                    lastFailedBuildName = targetBuilds[i];
                    break; //TODO: Add config option to make it continue installing even if one fails
                }
            }
            Program.ClearLoadBar();
            return success;
        }

        public void ProcessCommandOutput(string output)
        {
            //TODO: Add proper output interpretation and handling here
        }
    }
}
