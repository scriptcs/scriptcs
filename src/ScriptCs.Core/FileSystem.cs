using System;
using System.Collections.Generic;
using System.IO;

using ScriptCs.Contracts;

namespace ScriptCs
{
    public class FileSystem : IFileSystem
    {
        public virtual IEnumerable<string> EnumerateFiles(string dir, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return Directory.EnumerateFiles(dir, searchPattern, searchOption);
        }

        public virtual IEnumerable<string> EnumerateDirectories(string dir, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return Directory.EnumerateDirectories(dir, searchPattern, searchOption);
        }

        public virtual IEnumerable<string> EnumerateFilesAndDirectories(string dir, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return Directory.EnumerateFileSystemEntries(dir, searchPattern, searchOption);
        }

        public virtual void Copy(string source, string dest, bool overwrite)
        {
            File.Copy(source, dest, overwrite);
        }

        public virtual bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public virtual void CreateDirectory(string path, bool hidden)
        {
            var directory = Directory.CreateDirectory(path);

            if (hidden)
            {
                directory.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
        }

        public virtual void DeleteDirectory(string path)
        {
            Directory.Delete(path, true);
        }

        public virtual string ReadFile(string path)
        {
            return File.ReadAllText(path);
        }

        public virtual string[] ReadFileLines(string path)
        {
            return File.ReadAllLines(path);
        }

        public virtual bool IsPathRooted(string path)
        {
            return Path.IsPathRooted(path);
        }

        public virtual string CurrentDirectory
        {
            get { return Environment.CurrentDirectory; }
            set { Environment.CurrentDirectory = value; }
        }

        public virtual string NewLine
        {
            get { return Environment.NewLine; }
        }

        public virtual DateTime GetLastWriteTime(string file)
        {
            return File.GetLastWriteTime(file);
        }

        public virtual void Move(string source, string dest)
        {
            File.Move(source, dest);
        }

        public virtual bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public virtual void FileDelete(string path)
        {
            File.Delete(path);
        }

        public virtual IEnumerable<string> SplitLines(string value)
        {
            Guard.AgainstNullArgument("value", value);

            return value.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        }

        public virtual void WriteToFile(string path, string text)
        {
            File.WriteAllText(path, text);
        }

        public virtual Stream CreateFileStream(string filePath, FileMode mode)
        {
            return new FileStream(filePath, mode);
        }

        public virtual void WriteAllBytes(string filePath, byte[] bytes)
        {
            File.WriteAllBytes(filePath, bytes);
        }

        public virtual string ModulesFolder
        {
            get 
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "scriptcs");
            }
        }

        public virtual string GetWorkingDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return CurrentDirectory;
            }

            var realPath = GetFullPath(path);

            if (FileExists(realPath) || DirectoryExists(realPath))
            {
                var attributes = File.GetAttributes(realPath);

                if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    return realPath;
                }

                return Path.GetDirectoryName(realPath);
            }

            return Path.GetDirectoryName(realPath);
        }

        public virtual string GetFullPath(string path)
        {
            return Path.GetFullPath(path);
        }
    }
}
