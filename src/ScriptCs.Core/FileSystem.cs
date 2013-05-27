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

        public void DeleteDirectory(string path)
        {
            Directory.Delete(path, true);
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

        public void Move(string source, string dest)
        {
            File.Move(source, dest);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public void FileDelete(string path)
        {
            File.Delete(path);
        }

        public IEnumerable<string> SplitLines(string value)
        {
            return value.Split(new[] { NewLine }, StringSplitOptions.None);
        }

        public Stream CreateFileStream(string filePath, FileMode mode)
        {
            return new FileStream(filePath, mode);
        }

        public string GetWorkingDirectory(string path)
        {
            return IsPathRooted(path) ? Path.GetDirectoryName(path) : CurrentDirectory;
        }

        public string GetFullPath(string path)
        {
            return Path.GetFullPath(path);
        }
    }
}
