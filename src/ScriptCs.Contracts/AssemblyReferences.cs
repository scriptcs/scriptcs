namespace ScriptCs.Contracts
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System;
    using System.IO;

    public class AssemblyReferences
    {
        private readonly Dictionary<string, Assembly> _assemblies = new Dictionary<string, Assembly>();
        private readonly Dictionary<string, string> _paths = new Dictionary<string, string>();

        public AssemblyReferences()
            : this(Enumerable.Empty<string>())
        {
        }

        public AssemblyReferences(IEnumerable<string> paths)
            : this(paths, Enumerable.Empty<Assembly>())
        {
        }

        public AssemblyReferences(IEnumerable<string> paths, IEnumerable<Assembly> assemblies)
        {
            Guard.AgainstNullArgument("paths", paths);
            Guard.AgainstNullArgument("assemblies", assemblies);

            foreach (var assembly in assemblies.Where(assembly => assembly != null))
            {
                var name = assembly.GetName().Name;
                if (!_assemblies.ContainsKey(name))
                {
                    _assemblies.Add(name, assembly);
                }
            }

            foreach (var path in paths)
            {
                var name = Path.GetFileName(path);
                if (name == null)
                {
                    continue;
                }

                if (name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ||
                    name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    name = Path.GetFileNameWithoutExtension(name);
                }

                if (!_paths.ContainsKey(name) && !_assemblies.ContainsKey(name))
                {
                    _paths.Add(name, path);
                }
            }
        }

        public IEnumerable<Assembly> Assemblies
        {
            get { return _assemblies.Values.ToArray(); }
        }

        public IEnumerable<string> PathReferences
        {
            get { return _paths.Values.ToArray(); }
        }

        public AssemblyReferences Union(AssemblyReferences references)
        {
            Guard.AgainstNullArgument("references", references);

            return new AssemblyReferences(PathReferences.Union(references.PathReferences), Assemblies.Union(references.Assemblies));
        }

        public AssemblyReferences Union(IEnumerable<Assembly> assemblies)
        {
            Guard.AgainstNullArgument("assemblies", assemblies);

            return new AssemblyReferences(PathReferences, Assemblies.Union(assemblies));
        }

        public AssemblyReferences Union(IEnumerable<string> paths)
        {
            Guard.AgainstNullArgument("paths", paths);

            return new AssemblyReferences(PathReferences.Union(paths), Assemblies);
        }

        public AssemblyReferences Except(AssemblyReferences references)
        {
            Guard.AgainstNullArgument("references", references);

            return new AssemblyReferences(PathReferences.Except(references.PathReferences), Assemblies.Except(references.Assemblies));
        }

        public AssemblyReferences Except(IEnumerable<Assembly> assemblies)
        {
            Guard.AgainstNullArgument("assemblies", assemblies);

            return new AssemblyReferences(PathReferences, Assemblies.Except(assemblies));
        }

        public AssemblyReferences Except(IEnumerable<string> paths)
        {
            Guard.AgainstNullArgument("paths", paths);

            return new AssemblyReferences(PathReferences.Except(paths), Assemblies);
        }
    }
}
