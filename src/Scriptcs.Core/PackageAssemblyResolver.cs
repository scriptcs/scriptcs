using NuGet;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;

namespace Scriptcs
{
    public class PackageAssemblyResolver : IPackageAssemblyResolver
    {
        private readonly IFileSystem _fileSystem;

        [ImportingConstructor]
        public PackageAssemblyResolver(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public IEnumerable<string> GetAssemblyNames()
        {
            var packageDir = _fileSystem.CurrentDirectory + @"\" + "packages";

            if (!_fileSystem.DirectoryExists(packageDir))
                return Enumerable.Empty<string>();

            var folders = new List<string>();
            var files = new List<string>();
            foreach (var packageFileGroup in _fileSystem.EnumerateFiles(packageDir, @"*.dll")
                .Select(p => new NugetAssemblyPath(p, packageDir)).GroupBy(f => f.PackageName))
            {
                IEnumerable<NugetAssemblyPath> compatibleItems;
                VersionUtility.TryGetCompatibleItems(VersionUtility.DefaultTargetFramework, packageFileGroup, out compatibleItems);
                files.AddRange(compatibleItems.Select(x => x.Path));
                var path = Path.GetDirectoryName(compatibleItems.First().Path);
                Console.WriteLine("Found package reference: " + path);
            }
            return files;
        }

        public class NugetAssemblyPath : IFrameworkTargetable
        {
            public NugetAssemblyPath(string path, string packageDir)
            {
                Path = path;
                PathWithoutPackageDir = path.Replace(packageDir + @"\", "");
            }
            public string Path { get; set; }

            public string PackageName
            {
                get
                {
                    return PathWithoutPackageDir.Split(new[] { System.IO.Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries).First();
                }
            }

            public IEnumerable<FrameworkName> SupportedFrameworks
            {
                get 
                {
                    var pathWithoutPackageName = PathWithoutPackageDir.Remove(0, PackageName.Length + 1);
                    string effectivePath;
                    return new[] { VersionUtility.ParseFrameworkNameFromFilePath(pathWithoutPackageName, out effectivePath) };
                }
            }

            public string PathWithoutPackageDir { get; set; }
        }
    }
}
