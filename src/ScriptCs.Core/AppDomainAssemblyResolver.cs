using System.IO;
using Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCs
{
    public class AppDomainAssemblyResolver
    {
        private readonly ILog _logger;
        private IDictionary<string, AssemblyInfo> _assemblyInfoMap = new Dictionary<string, AssemblyInfo>();

        public AppDomainAssemblyResolver(ILog logger)
        {
            _logger = logger;
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
        }

        private Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            AssemblyInfo assemblyInfo = null;
            var name = new AssemblyName(args.Name);
            
            if (_assemblyInfoMap.TryGetValue(name.Name, out assemblyInfo) && _assemblyInfoMap.TryGetValue(name.Name, out assemblyInfo))
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

        public void AddAssemblyPaths(IEnumerable<string> assemblyPaths)
        {
            foreach (var assemblyPath in assemblyPaths)
            {
                var info = new AssemblyInfo {Path = assemblyPath};
                var name = AssemblyName.GetAssemblyName(assemblyPath);
                if (!_assemblyInfoMap.ContainsKey(name.Name))
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
        }
    }
}
