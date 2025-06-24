using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prim8VR_Installer.Behaviors
{
    public enum BuildOrigin
    {
        Remote,
        Local
    }
    public class BuildData
    {
        public string Name { get; set; }
        public BuildOrigin Origin {  get; set; }
        public string FilePath { get; set; }

        public virtual string GetFilePath()
        {
            return FilePath;
        }

        //More stuff like: OwnerProject, BuildDate, Version, etc. 
    }
}
