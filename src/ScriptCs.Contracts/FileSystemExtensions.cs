using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCs.Contracts
{
    public static class FileSystemExtensions
    {
        [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1019:MemberAccessSymbolsMustBeSpacedCorrectly", Justification = "Fluent API")]
        public static IEnumerable<string> EnumerateBinaries(this IFileSystem fileSystem, string path, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return fileSystem.EnumerateFiles(path, "*.dll", searchOption).
                Union(fileSystem.EnumerateFiles(path, "*.exe", searchOption)).
                Where(f=>!f.Equals("scriptcs.exe", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
