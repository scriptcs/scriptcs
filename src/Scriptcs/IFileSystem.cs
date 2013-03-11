using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scriptcs
{
    [InheritedExport]
    public interface IFileSystem
    {
        FileStream CreateFileStream(string path, FileMode mode);
        IEnumerable<string> EnumerateFiles(string dir, string search);
        void Copy(string source, string dest, bool overwrite);
        bool DirectoryExists(string path);
        void CreateDirectory(string path);
        string ReadFile(string path);
        string CurrentDirectory { get; }
        DateTime GetLastWriteTime(string file);
    }
}
