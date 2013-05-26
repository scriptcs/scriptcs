using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Common.Logging;

namespace ScriptCs.Command
{
    public abstract class ScriptCommand : IScriptCommand
    {
        protected readonly IFileSystem FileSystem;

        protected readonly ILog Logger;

        private readonly IPackageAssemblyResolver _packageAssemblyResolver;

        private readonly IAssemblyName _assemblyName;

        protected ScriptCommand(
            IFileSystem fileSystem,
            IPackageAssemblyResolver packageAssemblyResolver,
            IAssemblyName assemblyName,
            ILog logger)
        {
            FileSystem = fileSystem;
            _packageAssemblyResolver = packageAssemblyResolver;
            _assemblyName = assemblyName;
            Logger = logger;
        }

        public string[] ScriptArgs { get; protected set; }

        public abstract CommandResult Execute();

        protected IEnumerable<string> GetAssemblyPaths(string workingDirectory)
        {
            var assemblyPaths = new List<string>();

            var packagesFolder = Path.Combine(workingDirectory, Constants.PackagesFolder);
            if (FileSystem.DirectoryExists(packagesFolder))
            {
                var packageAssemblies = _packageAssemblyResolver.GetAssemblyNames(workingDirectory);
                assemblyPaths.AddRange(packageAssemblies);
            }

            var looseAssemblies = FileSystem.EnumerateFiles(workingDirectory, "*.dll", SearchOption.TopDirectoryOnly)
                    .Union(FileSystem.EnumerateFiles(workingDirectory, "*.exe", SearchOption.TopDirectoryOnly))
                    .Where(IsManagedAssembly);

            assemblyPaths.AddRange(looseAssemblies);

            foreach (var path in assemblyPaths)
            {
                Logger.DebugFormat("Found assembly reference: {0}", Path.GetFileName(path));
            }

            return assemblyPaths;
        }

        private bool IsManagedAssembly(string path)
        {
            try
            {
                _assemblyName.GetAssemblyName(path);
            }
            catch (BadImageFormatException)
            {
                return false;
            }
            return true;
        }
    }
}