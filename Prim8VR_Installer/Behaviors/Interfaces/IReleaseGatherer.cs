using Prim8VR_Installer.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prim8VR_Installer.Interfaces
{
    public interface IReleaseGatherer
    {
        public List<string> GetAvailableReleases();
        public string GetFilePathOfRelease(string releaseName);
    }
}
