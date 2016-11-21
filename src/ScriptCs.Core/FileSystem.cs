using System;
using System.Collections.Generic;
using System.IO;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class FileSystem : IFileSystem
    {
        public virtual IEnumerable<string> EnumerateFiles(
            string dir, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return Directory.EnumerateFiles(dir, searchPattern, searchOption);
        }

        public virtual IEnumerable<string> EnumerateDirectories(
            string dir, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return Directory.EnumerateDirectories(dir, searchPattern, searchOption);
        }

        public virtual IEnumerable<string> EnumerateFilesAndDirectories(
            string dir, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return Directory.EnumerateFileSystemEntries(dir, searchPattern, searchOption);
        }

        public virtual void Copy(string source, string dest, bool overwrite)
        {
            File.Copy(source, dest, overwrite);
        }

        public virtual void CopyDirectory(string source, string dest, bool overwrite)
        {
            // NOTE: adding guards since the exceptions thrown by System.IO would be confusing
            Guard.AgainstNullArgument("source", source);
            Guard.AgainstNullArgument("dest", dest);

            if (!Directory.Exists(dest))
            {
                Directory.CreateDirectory(dest);
            }

            foreach (var file in Directory.GetFiles(source))
            {
                File.Copy(file, Path.Combine(dest, Path.GetFileName(file)), overwrite);
            }

            foreach (var directory in Directory.GetDirectories(source))
            {
                CopyDirectory(directory, Path.Combine(dest, Path.GetFileName(directory)), overwrite);
            }
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

        public virtual void MoveDirectory(string source, string dest)
        {
            Directory.Move(source, dest);
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

        public virtual string GlobalFolder
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "scriptcs");
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
                if ((File.GetAttributes(realPath) & FileAttributes.Directory) == FileAttributes.Directory)
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


        public virtual string TempPath
        {
            get
            {
                return Path.GetTempPath();
            }
        }

        public virtual string HostBin
        {
            get { return AppDomain.CurrentDomain.BaseDirectory; }
        }

        public virtual string BinFolder
        {
            get { return "scriptcs_bin"; }
        }

        public virtual string DllCacheFolder
        {
            get { return ".scriptcs_cache"; }
        }

        public virtual string PackagesFile
        {
            get { return "scriptcs_packages.config"; }
        }

        public virtual string PackagesFolder
        {
            get { return "scriptcs_packages"; }
        }

        public virtual string NugetFile
        {
            get { return "scriptcs_nuget.config"; }
        }

        public virtual string GlobalOptsFile
        {
            get { return Path.Combine(GlobalFolder, Constants.ConfigFilename); }
        }
    }
}
