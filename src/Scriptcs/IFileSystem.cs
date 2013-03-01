using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scriptcs
{
    [InheritedExport]
    public interface IFileSystem
    {
        IEnumerable<string> EnumerateFiles(string dir, string search);
        void Copy(string source, string dest);
        bool DirectoryExists(string path);
        void CreateDirectory(string path);
        string ReadFile(string path);
        string CurrentDirectory { get; }
        DateTime GetLastWriteTime(string file);
    }
}
