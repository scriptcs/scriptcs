using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using NuGet;

namespace ScriptCs.Package
{
    internal class PackageObject : IPackageObject
    {
        private const string Dll = ".dll";
        private readonly IPackage _package;

        public PackageObject(IPackage package)
        {
            _package = package;
            Id = package.Id;
            Version = package.Version.ToString();
        }

        public string Id { get; private set; }
        public string Version { get; private set; }

        public string FullName
        {
            get { return Id + "." + Version; }
        }

        public IEnumerable<string> GetCompatibleDlls(FrameworkName frameworkName)
        {
            var dlls = _package.GetLibFiles().Where(i => i.EffectivePath.EndsWith(Dll));
            IEnumerable<IPackageFile> compatibleFiles;
            VersionUtility.TryGetCompatibleItems(frameworkName, dlls, out compatibleFiles);

            return compatibleFiles != null ? compatibleFiles.Select(i => i.Path) : null;
        }
    }
}