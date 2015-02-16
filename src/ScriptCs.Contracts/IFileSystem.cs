using System;
using System.Collections.Generic;
using System.IO;

namespace ScriptCs.Contracts
{
    public interface IFileSystem
    {
        IEnumerable<string> EnumerateFiles(
            string path, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories);

        IEnumerable<string> EnumerateDirectories(
            string path, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories);

        IEnumerable<string> EnumerateFilesAndDirectories(
            string path, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories);

        void CopyFile(string source, string dest, bool overwrite);

        void CopyDirectory(string source, string dest, bool overwrite);

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

        void MoveFile(string source, string dest);

        void MoveDirectory(string source, string dest);

        bool FileExists(string path);

        void DeleteFile(string path);

        IEnumerable<string> SplitLines(string value);

        void WriteToFile(string path, string text);

        Stream CreateFileStream(string path, FileMode mode);

        void WriteToFile(string path, byte[] bytes);

        string GlobalFolder { get; }

        string HostBin { get; }

        string BinFolder { get; }

        string DllCacheFolder { get; }

        string PackagesFile { get; }

        string PackagesFolder { get; }

        string NugetFile { get; }

        string GlobalOptsFile { get; }
    }
}
