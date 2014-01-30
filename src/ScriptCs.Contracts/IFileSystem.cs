using System;
using System.Collections.Generic;
using System.IO;

namespace ScriptCs.Contracts
{
    public interface IFileSystem
    {
        void OnChanged(string path, string searchPattern, Action handler);

        IEnumerable<string> EnumerateFiles(string dir, string search, SearchOption searchOption = SearchOption.AllDirectories);

        void Copy(string source, string dest, bool overwrite);

        bool DirectoryExists(string path);

        void CreateDirectory(string path, bool hidden = false);

        void DeleteDirectory(string path);

        string ReadFile(string path);

        string[] ReadFileLines(string path);

        DateTime GetLastWriteTime(string file);
        
        bool IsPathRooted(string path);

        string GetFullPath(string path);

        string CurrentDirectory { get; set; }

        string NewLine { get; }

        string GetWorkingDirectory(string path);

        void Move(string source, string dest);

        bool FileExists(string path);

        void FileDelete(string path);

        IEnumerable<string> SplitLines(string value);

        void WriteToFile(string path, string text);
        
        Stream CreateFileStream(string filePath, FileMode mode);

        void WriteAllBytes(string filePath, byte[] bytes);

        string ModulesFolder { get; }
    }
}