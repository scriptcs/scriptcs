using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ScriptCs.Contracts
{
    public class AssemblyReferences
    {
        public AssemblyReferences() : this (new HashSet<string>(), new HashSet<Assembly>())
        {}

        public AssemblyReferences(HashSet<string> pathReference, HashSet<Assembly> assemblies)
        {
            PathReferences = pathReference;
            Assemblies = assemblies;
        }

        public HashSet<string> PathReferences { get; set; }
        public HashSet<Assembly> Assemblies { get; set; }

        public AssemblyReferences Except(AssemblyReferences obj)
        {
            Guard.AgainstNullArgument("obj", obj);

            var deltaObject = new AssemblyReferences
                {
                    PathReferences = new HashSet<string>(PathReferences.Except(obj.PathReferences)),
                    Assemblies = new HashSet<Assembly>(Assemblies.Except(obj.Assemblies))
                };
            return deltaObject;
        }

        public void Union(AssemblyReferences obj)
        {
            PathReferences.UnionWith(obj.PathReferences);
            Assemblies.UnionWith(obj.Assemblies);
        }

        public static AssemblyReferences New(IEnumerable<string> pathReference, IEnumerable<Assembly> assemblies)
        {
            return new AssemblyReferences(new HashSet<string>(pathReference), new HashSet<Assembly>(assemblies));
        }
    }
}