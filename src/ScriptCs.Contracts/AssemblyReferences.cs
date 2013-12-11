using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ScriptCs.Contracts
{
    public class AssemblyReferences
    {
        public AssemblyReferences()
        {
            PathReferences = new HashSet<string>();
            Assemblies = new HashSet<Assembly>();
        }

        public HashSet<string> PathReferences { get; set; }
        public HashSet<Assembly> Assemblies { get; set; }

        public AssemblyReferences Except(AssemblyReferences obj)
        {
            var deltaObject = new AssemblyReferences
                {
                    PathReferences = new HashSet<string>(PathReferences.Except(obj.PathReferences)),
                    Assemblies = new HashSet<Assembly>(Assemblies.Except(obj.Assemblies))
                };
            return deltaObject;
        }
    }
}