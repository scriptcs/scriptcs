using System.IO;
using Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class AppDomainAssemblyResolver : IAppDomainAssemblyResolver
    {
        private readonly ILog _logger;
        private readonly IFileSystem _fileSystem;
        private readonly IAssemblyResolver _resolver;
        private IDictionary<string, AssemblyInfo> _assemblyInfoMap = new Dictionary<string, AssemblyInfo>();

        public AppDomainAssemblyResolver(ILog logger, IFileSystem fileSystem, IAssemblyResolver resolver)
        {
            _logger = logger;
            _fileSystem = fileSystem;
            _resolver = resolver;
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
        }

        private Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            AssemblyInfo assemblyInfo = null;
            var name = new AssemblyName(args.Name);
            
            if (_assemblyInfoMap.TryGetValue(name.Name, out assemblyInfo))
            {
                lock(assemblyInfo)
                {
                    assemblyInfo.Assembly = Assembly.LoadFile(assemblyInfo.Path);
                }
                _logger.DebugFormat("Resolving: from: {0} to: {1}", args.Name, assemblyInfo.Assembly.GetName());
                return assemblyInfo.Assembly;
            }
            return null;
        }

        public void InitializeAppDomainAssemblyResolver()
        {
            var hostAssemblyPaths = _fileSystem.EnumerateFiles(_fileSystem.HostBin, "*.dll", SearchOption.TopDirectoryOnly);
            AddAssemblyPaths(hostAssemblyPaths);

            var globalPaths = _resolver.GetAssemblyPaths(_fileSystem.ModulesFolder);
            AddAssemblyPaths(globalPaths);

            var scriptAssemblyPaths = _resolver.GetAssemblyPaths(_fileSystem.CurrentDirectory);
            AddAssemblyPaths(scriptAssemblyPaths);
        }

        public void AddAssemblyPaths(IEnumerable<string> assemblyPaths)
        {
            foreach (var assemblyPath in assemblyPaths)
            {
                var info = new AssemblyInfo {Path = assemblyPath};
                var name = AssemblyName.GetAssemblyName(assemblyPath);
                info.Version = name.Version;

                AssemblyInfo foundInfo = null;
                var found = _assemblyInfoMap.TryGetValue(name.Name, out foundInfo );

                if (!found || foundInfo.Version.CompareTo(info.Version) < 0)
                {
                    _logger.DebugFormat("Mapping Assembly {0} to version:{1}", name.Name, name.Version);
                    _assemblyInfoMap[name.Name] = info;
                }
            }
        }

        public class AssemblyInfo
        {
            public string Path { get; set; }
            public Assembly Assembly { get; set; }
            public Version Version { get; set; }
        }
    }
}
