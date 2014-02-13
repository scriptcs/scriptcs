using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using ScriptCs.Contracts;
using ScriptCs.Exceptions;
using ScriptCs.Package;

namespace ScriptCs
{
    public class ScriptedScriptPackFinder : IScriptedScriptPackFinder
    {
        private readonly IFileSystem _fileSystem;
        private readonly IPackageContainer _packageContainer;
        private readonly ILog _logger;
        private IList<string> _scripts;
        private List<IPackageReference> _topLevelPackages;

        public ScriptedScriptPackFinder(IFileSystem fileSystem, IPackageContainer packageContainer, ILog logger)
        {
            _fileSystem = fileSystem;
            _packageContainer = packageContainer;
            _logger = logger;
            _scripts = new List<string>();
        }

        private IEnumerable<IPackageReference> GetPackages(string workingDirectory)
        {
            var packageFile = Path.Combine(workingDirectory, Constants.PackagesFile);
            var packages = _packageContainer.FindReferences(packageFile).ToList();

            _topLevelPackages = packages;

            return packages;
        }

        public IEnumerable<string> GetScriptedScriptPacks(string workingDirectory)
        {
            var packages = GetPackages(workingDirectory).ToList();
            if (!packages.Any())
            {
                return null;
            }

            var packageFile = Path.Combine(workingDirectory, Constants.PackagesFile);
            var packageDir = Path.Combine(workingDirectory, Constants.PackagesFolder);

            var foundScriptPacks = new List<string>();
            var missingPackages = new List<IPackageReference>();

            FindScripts(packageDir, packages, missingPackages, foundScriptPacks, _fileSystem.FileExists(packageFile));

            if (missingPackages.Count > 0)
            {
                var missingPackagesString = string.Join(",", missingPackages.Select(i => i.PackageId + " " + i.FrameworkName.FullName));
                throw new MissingAssemblyException(string.Format("Missing: {0}", missingPackagesString));
            }
            return foundScriptPacks;
        }

        private void FindScripts(string packageDir, IEnumerable<IPackageReference> packageReferences, List<IPackageReference> missingPackages, List<string> foundScriptPacks, bool strictLoad = true)
        {
            foreach (var packageRef in packageReferences)
            {
                var nugetPackage = _packageContainer.FindPackage(packageDir, packageRef);
                if (nugetPackage == null)
                {
                    missingPackages.Add(packageRef);
                    _logger.Info("Cannot find: " + packageRef.PackageId + " " + packageRef.Version);

                    continue;
                }

                var scriptPack = nugetPackage.GetScriptBasedScriptPack();
                if (scriptPack != null && !foundScriptPacks.Contains(scriptPack))
                {
                    foundScriptPacks.Add(string.Format(@"{0}\{1}", packageDir, scriptPack));
                    _logger.Debug("Found: " + scriptPack);

                }

                if (nugetPackage.Dependencies == null || !nugetPackage.Dependencies.Any() || !strictLoad)
                {
                    continue;
                }

                var dependencyReferences = nugetPackage.Dependencies
                    .Where(i => _topLevelPackages.All(x => x.PackageId != i.Id))
                    .Select(i => new PackageReference(i.Id, i.FrameworkName, i.Version));

                FindScripts(packageDir, dependencyReferences, missingPackages, foundScriptPacks, true);
            }
        }
 
    }

}
