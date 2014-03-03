using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class FilePathFinder : IFilePathFinder
    {
        private readonly char[] SLASHES = {'\\', '/'};

        private readonly IFileSystem _fileSystem;

        public FilePathFinder(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public string[] FindPossibleFilePaths(string pathFragment)
        {
            return FindPossiblePaths(pathFragment, "", new[] { _fileSystem.CurrentDirectory }, _fileSystem);
        }

        public string[] FindPossibleAssemblyNames(string nameFragment)
        {
            var roots = new List<string>
            {
                _fileSystem.CurrentDirectory,                
            };

            AddGACRoots(@"C:\Windows\Microsoft.Net\assembly", roots, _fileSystem);

            return FindPossiblePaths(nameFragment, ".dll", roots.Distinct(), _fileSystem);
        }

        private void AddGACRoots(string node, List<string> roots, IFileSystem fileSystem)
        {
            if (fileSystem.EnumerateFiles(node, "*.dll", SearchOption.TopDirectoryOnly).Any()) roots.Add(node);

            var subDirs = fileSystem.EnumerateDirectories(node, "*", SearchOption.TopDirectoryOnly);

            foreach (var dir in subDirs)
            {
                AddGACRoots(dir, roots, fileSystem);
            }
       
        }

        private string[] FindPossiblePaths(string pathFragment, string suffix, IEnumerable<string> roots, IFileSystem fileSystem)
        {
            int lastSlashIndex = pathFragment.LastIndexOfAny(SLASHES);

            var possiblePaths = new List<string>();

            foreach (var r in roots)
            {
                string path;
                string pattern;
                var partialPath = pathFragment.Substring(0, lastSlashIndex + 1); 

                if (lastSlashIndex >= 0)
                {
                    path = Path.Combine(r, partialPath);
                    pattern = pathFragment.Substring(lastSlashIndex + 1);
                }
                else
                {
                    path = r;
                    pattern = pathFragment;
                }

                try
                {
                    possiblePaths.AddRange(fileSystem.EnumerateFilesAndDirectories(
                        path,
                        pattern + "*",
                        SearchOption.TopDirectoryOnly).Where(p => p.EndsWith(suffix)).Select<string, string>(p => AugmentPathFragment(partialPath, p)));
                }
                catch (Exception)
                {
                    //
                }
            }

            return possiblePaths.Any() ? possiblePaths.Distinct().ToArray() : new[] { pathFragment };
        }

        private string AugmentPathFragment(string partialPath, string completePath)
        {
            int lastSlashIndex = completePath.LastIndexOfAny(SLASHES);
            var name = completePath.Substring(lastSlashIndex + 1);

            return Path.Combine(partialPath, name);
        }
    }
}