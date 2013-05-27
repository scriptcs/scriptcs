using System.Collections.Generic;

namespace ScriptCs
{
    public class ScriptManifest
    {
        public ScriptManifest()
        {
            PackageAssemblies = new HashSet<string>();
        }

        public HashSet<string> PackageAssemblies { get; set; } 
    }
}