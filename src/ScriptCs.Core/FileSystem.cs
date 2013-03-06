using System;
using System.Collections.Generic;
using System.IO;

namespace ScriptCs
{
    public class FileSystem : IFileSystem
    {
        public IEnumerable<string> EnumerateFiles(string dir, string searchPattern)
        {
            return Directory.EnumerateFiles(dir, searchPattern, SearchOption.AllDirectories);
        }

        public void Copy(string source, string dest, bool overwrite)
        {
            File.Copy(source, dest, overwrite);
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        public string ReadFile(string path)
        {
            return File.ReadAllText(path);
        }

        public string[] ReadFileLines(string path)
        {
            return File.ReadAllLines(path);
        }

        public bool IsPathRooted(string path)
        {
            return Path.IsPathRooted(path);
        }

        public string CurrentDirectory
        {
            get { return Environment.CurrentDirectory; }
        }

        public string NewLine
        {
            get { return Environment.NewLine; }
        }

        public DateTime GetLastWriteTime(string file)
        {
            return File.GetLastWriteTime(file);
        }
    }
}
