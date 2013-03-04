using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            if (!File.Exists(packageDir))
                return Enumerable.Empty<string>();

            var folders = new List<string>();
            var files = new List<string>();
            foreach (var file in Directory.EnumerateFiles(packageDir, @"*.dll", SearchOption.AllDirectories))
            {
                if (file.IndexOf(@"\net35") > -1 || file.IndexOf(@"\net40") > -1)
                {
                    var path = Path.GetDirectoryName(file);
                    files.Add(file);
                    if (!folders.Contains(path))
                    {
                        folders.Add(path);
                        Console.WriteLine("Found package reference: " + path);
                    }
                }
            }
            return files;
        }  
    }
}
