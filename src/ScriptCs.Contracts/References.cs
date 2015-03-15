namespace ScriptCs.Contracts
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System;
    using System.IO;

    public class References
    {
        private readonly Dictionary<string, Assembly> _assemblies = new Dictionary<string, Assembly>();
        private readonly Dictionary<string, string> _paths = new Dictionary<string, string>();

        public References()
            : this(Enumerable.Empty<string>())
        {
        }

        public References(IEnumerable<Assembly> assemblies)
            : this(assemblies, Enumerable.Empty<string>())
        {
        }

        public References(IEnumerable<string> paths)
            : this(Enumerable.Empty<Assembly>(), paths)
        {
        }

        public References(IEnumerable<Assembly> assemblies, IEnumerable<string> paths)
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

        public IEnumerable<string> Paths
        {
            get { return _paths.Values.ToArray(); }
        }

        public References Union(References references)
        {
            Guard.AgainstNullArgument("references", references);

            return new References(Assemblies.Union(references.Assemblies), Paths.Union(references.Paths));
        }

        public References Union(IEnumerable<Assembly> assemblies)
        {
            Guard.AgainstNullArgument("assemblies", assemblies);

            return new References(Assemblies.Union(assemblies), Paths);
        }

        public References Union(IEnumerable<string> paths)
        {
            Guard.AgainstNullArgument("paths", paths);

            return new References(Assemblies, Paths.Union(paths));
        }

        public References Except(References references)
        {
            Guard.AgainstNullArgument("references", references);

            return new References(Assemblies.Except(references.Assemblies), Paths.Except(references.Paths));
        }

        public References Except(IEnumerable<Assembly> assemblies)
        {
            Guard.AgainstNullArgument("assemblies", assemblies);

            return new References(Assemblies.Except(assemblies), Paths);
        }

        public References Except(IEnumerable<string> paths)
        {
            Guard.AgainstNullArgument("paths", paths);

            return new References(Assemblies, Paths.Except(paths));
        }
    }
}
