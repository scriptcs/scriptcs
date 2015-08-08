using System;
using System.Collections.Generic;
using System.Reflection;
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
        private readonly IDictionary<string, AssemblyInfo> _assemblyInfoMap;

        [Obsolete("Support for Common.Logging types was deprecated in version 0.15.0 and will soon be removed.")]
        public AppDomainAssemblyResolver(
            Common.Logging.ILog logger,
            IFileSystem fileSystem,
            IAssemblyResolver resolver,
            IAssemblyUtility assemblyUtility,
            IDictionary<string, AssemblyInfo> assemblyInfoMap = null,
            Func<object, ResolveEventArgs, Assembly> resolveHandler = null)
            : this(new CommonLoggingLogProvider(logger), fileSystem, resolver, assemblyUtility, assemblyInfoMap, resolveHandler)
        {
        }

        public AppDomainAssemblyResolver(
            ILogProvider logProvider,
            IFileSystem fileSystem,
            IAssemblyResolver resolver,
            IAssemblyUtility assemblyUtility,
            IDictionary<string, AssemblyInfo> assemblyInfoMap = null,
            Func<object, ResolveEventArgs, Assembly> resolveHandler = null)
        {
            Guard.AgainstNullArgument("logProvider", logProvider);
            Guard.AgainstNullArgument("fileSystem", fileSystem);
            Guard.AgainstNullArgument("resolver", resolver);
            Guard.AgainstNullArgument("assemblyUtility", assemblyUtility);

            _assemblyInfoMap = assemblyInfoMap ?? new Dictionary<string, AssemblyInfo>();
            _assemblyUtility = assemblyUtility;
            _logger = logProvider.ForCurrentType();
            _fileSystem = fileSystem;
            _resolver = resolver;

            if (resolveHandler == null)
            {
                resolveHandler = AssemblyResolve;
            }

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(resolveHandler);
        }

        internal Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            AssemblyInfo assemblyInfo;
            var name = new AssemblyName(args.Name);

            if (_assemblyInfoMap.TryGetValue(name.Name, out assemblyInfo))
            {
                lock (assemblyInfo)
                {
                    if (assemblyInfo.Assembly == null)
                    {
                        assemblyInfo.Assembly = _assemblyUtility.LoadFile(assemblyInfo.Path);
                    }
                }

                _logger.DebugFormat("Resolving from: {0} to: {1}", args.Name, assemblyInfo.Assembly.GetName());
                return assemblyInfo.Assembly;
            }

            return null;
        }

        public void Initialize()
        {
            var hostAssemblyPaths = _fileSystem.EnumerateBinaries(_fileSystem.HostBin, SearchOption.TopDirectoryOnly);
            AddAssemblyPaths(hostAssemblyPaths);

            var globalPaths = _resolver.GetAssemblyPaths(_fileSystem.GlobalFolder, true);
            AddAssemblyPaths(globalPaths);

            var scriptAssemblyPaths = _resolver.GetAssemblyPaths(_fileSystem.CurrentDirectory, true);
            AddAssemblyPaths(scriptAssemblyPaths);
        }

        public virtual void AddAssemblyPaths(IEnumerable<string> assemblyPaths)
        {
            Guard.AgainstNullArgument("assemblyPaths", assemblyPaths);

            foreach (var assemblyPath in assemblyPaths)
            {
                if (_assemblyUtility.IsManagedAssembly(assemblyPath))
                {
                    var info = new AssemblyInfo { Path = assemblyPath };
                    var name = _assemblyUtility.GetAssemblyName(assemblyPath);
                    info.Version = name.Version;

                    AssemblyInfo foundInfo;
                    var found = _assemblyInfoMap.TryGetValue(name.Name, out foundInfo);

                    if (!found || foundInfo.Version.CompareTo(info.Version) < 0)
                    {
                        // if the assembly being passed is a higher version
                        // and an assembly with it's name has already been resolved
                        if (found && foundInfo.Assembly != null)
                        {
                            _logger.WarnFormat(
                                "Conflict: Assembly {0} with version {1} cannot be added as it has already been resolved",
                                assemblyPath,
                                info.Version);

                            continue;
                        }

                        _logger.DebugFormat("Mapping Assembly {0} to version:{1}", name.Name, name.Version);
                        _assemblyInfoMap[name.Name] = info;
                    }
                }
                else
                {
                    _logger.DebugFormat("Skipping Mapping Native Assembly {0}", assemblyPath);
                }
            }
        }
    }
}
