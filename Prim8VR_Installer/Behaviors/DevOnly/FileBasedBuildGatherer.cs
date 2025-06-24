using Prim8VR_Installer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prim8VR_Installer.Behaviors.DevOnly
{
    public class FileBasedBuildGatherer : IReleaseGatherer
    {
        private Dictionary<string, string> builds;
        public FileBasedBuildGatherer()
        {
            builds = new Dictionary<string, string>();
        }

        public void ManuallyAddBuilds(params string[] paths)
        {
            for (int i = 0; i < paths.Length; i++)
            {
                string path = paths[i];
                string name = path.Substring(path.LastIndexOf('\\')+1);
                builds.Add(name, path);
            }
        }
        public List<string> GetAvailableReleases()
        {
            return builds.Keys.ToList();
        }

        public string GetFilePathOfRelease(string releaseName)
        {
            return builds[releaseName];
        }

    }
}
