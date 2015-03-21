using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ScriptCs.Contracts
{
    [Obsolete("Deprecated in 0.14. Use References instead.")]
    public class AssemblyReferences
    {
        public AssemblyReferences()
            : this(Enumerable.Empty<string>())
        {
        }

        public AssemblyReferences(IEnumerable<string> pathReferences)
            : this(pathReferences, Enumerable.Empty<Assembly>())
        {
        }

        public AssemblyReferences(IEnumerable<Assembly> assemblies)
            : this(Enumerable.Empty<string>(), assemblies)
        {
        }

        public AssemblyReferences(IEnumerable<string> pathReferences, IEnumerable<Assembly> assemblies)
        {
            Guard.AgainstNullArgument("pathReferences", pathReferences);
            Guard.AgainstNullArgument("assemblies", assemblies);

            PathReferences = new HashSet<string>(pathReferences);
            Assemblies = new HashSet<Assembly>(assemblies);
        }

        public HashSet<string> PathReferences { get; private set; }
        public HashSet<Assembly> Assemblies { get; private set; }

        public AssemblyReferences Except(AssemblyReferences obj)
        {
            Guard.AgainstNullArgument("obj", obj);

            return new AssemblyReferences(PathReferences.Except(obj.PathReferences), Assemblies.Except(obj.Assemblies));
        }

        public void Union(AssemblyReferences obj)
        {
            Guard.AgainstNullArgument("obj", obj);

            PathReferences.UnionWith(obj.PathReferences);
            Assemblies.UnionWith(obj.Assemblies);
        }
    }
}
