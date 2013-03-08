using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace ScriptCs
{
    [InheritedExport]
    public interface IFileSystem
    {
        IEnumerable<string> EnumerateFiles(string dir, string search);

        void Copy(string source, string dest, bool overwrite);

        bool DirectoryExists(string path);

        void CreateDirectory(string path);

        string ReadFile(string path);

        string[] ReadFileLines(string path);

        DateTime GetLastWriteTime(string file);
        
        bool IsPathRooted(string path);

        string CurrentDirectory { get; }

        string NewLine { get; }

        string GetWorkingDirectory(string path);

        void Move(string source, string dest);

        bool FileExists(string path);
    }
}