﻿using System;
using System.Collections.Generic;
using System.IO;

namespace ScriptCs
{
    public class FileSystem : IFileSystem
    {
        public IEnumerable<string> EnumerateFiles(string dir, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return Directory.EnumerateFiles(dir, searchPattern, searchOption);
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
            Guard.AgainstNullArgument("value", value);

            return value.Split(new[] { NewLine }, StringSplitOptions.None);
        }

        public void WriteToFile(string path, string text)
        {
            File.WriteAllText(path, text);
        }

        public Stream CreateFileStream(string filePath, FileMode mode)
        {
            return new FileStream(filePath, mode);
        }

        public string GetWorkingDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return CurrentDirectory;

            var realPath = GetFullPath(path);

            if (FileExists(realPath) || DirectoryExists(realPath))
            {
                var attributes = File.GetAttributes(realPath);

                if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
                    return realPath;
                
                return Path.GetDirectoryName(realPath);
            }

            return Path.GetDirectoryName(realPath);
        }

        public string GetFullPath(string path)
        {
            return Path.GetFullPath(path);
        }
    }
}
