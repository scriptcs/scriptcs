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

        public string[] FindPossibleFilePaths(string pathFragment, IFileSystem fileSystem)
        {
            return FindPossiblePaths(pathFragment, "", new[] { fileSystem.CurrentDirectory }, fileSystem);
        }

        public string[] FindPossibleAssemblyNames(string nameFragment, IFileSystem fileSystem)
        {
            var roots = new List<string>
            {
                fileSystem.CurrentDirectory,                
            };

            AddGACRoots(@"C:\Windows\Microsoft.Net\assembly", roots, fileSystem);

            return FindPossiblePaths(nameFragment, ".dll", roots.Distinct(), fileSystem);
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