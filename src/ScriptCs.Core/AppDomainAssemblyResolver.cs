using Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ScriptCs.Contracts;
using System.IO;

namespace ScriptCs
{
    public class AppDomainAssemblyResolver : IAppDomainAssemblyResolver
    {
        private readonly ILog _logger;
        private readonly IFileSystem _fileSystem;
        private readonly IAssemblyResolver _resolver;
        private readonly IAssemblyUtility _assemblyUtility;
        private IDictionary<string, AssemblyInfo> _assemblyInfoMap;

        public AppDomainAssemblyResolver(ILog logger, IFileSystem fileSystem, IAssemblyResolver resolver, IDictionary<string, AssemblyInfo> assemblyInfoMap = null, IAssemblyUtility assemblyUtility = null )
        {
            if (_assemblyUtility == null)
            {
                _assemblyUtility = new AssemblyUtility();
            }

            _assemblyInfoMap = assemblyInfoMap;
            if (_assemblyInfoMap == null)
            {
                _assemblyInfoMap = new Dictionary<string, AssemblyInfo>();
            }
            
            _logger = logger;
            _fileSystem = fileSystem;
            _resolver = resolver;
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
        }

        internal Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {     
            AssemblyInfo assemblyInfo = null;
            var name = new AssemblyName(args.Name);
            
            if (_assemblyInfoMap.TryGetValue(name.Name, out assemblyInfo))
            {                
                lock(assemblyInfo)
                {                    
                    if (assemblyInfo.Assembly == null)
                        assemblyInfo.Assembly = _assemblyUtility.LoadFile(assemblyInfo.Path);
                }
                _logger.DebugFormat("Resolving: from: {0} to: {1}", args.Name, assemblyInfo.Assembly.GetName());
                return assemblyInfo.Assembly;
            }
            return null;
        }

        public void Initialize()
        {
            var hostAssemblyPaths = _fileSystem.EnumerateBinaries(_fileSystem.HostBin, SearchOption.TopDirectoryOnly);
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
                var name = _assemblyUtility.GetAssemblyName(assemblyPath);
                info.Version = name.Version;

                AssemblyInfo foundInfo = null;
                var found = _assemblyInfoMap.TryGetValue(name.Name, out foundInfo );

                if (!found || foundInfo.Version.CompareTo(info.Version) < 0)
                {
                    //if the assembly being passed is a higher version and an assembly with it's name has already been resolved
                    if (found && foundInfo.Assembly != null)
                    {
                        _logger.Warn(string.Format("Conflict: Assembly {0} with version {1} cannot be added as it has already been resolved", assemblyPath));
                        continue;
                    }
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
