using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ScriptCs.Contracts
{
    public static class FileSystemExtensions
    {
        public static IEnumerable<string> EnumerateBinaries(
            this IFileSystem fileSystem, string path, SearchOption searchOption = SearchOption.AllDirectories)
        {
            Guard.AgainstNullArgument("fileSystem", fileSystem);

            return fileSystem.EnumerateFiles(path, "*.dll", searchOption)
                .Union(fileSystem.EnumerateFiles(path, "*.exe", searchOption))
                .Where(f=>!f.Equals("scriptcs.exe", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
