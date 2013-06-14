using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Common.Logging;

namespace ScriptCs
{
    public class FileSystem : IFileSystem
    {
        private readonly ILog _logger;
        private Dictionary<string, FileSystemWatcher> watchers =  new Dictionary<string,FileSystemWatcher>();

        public FileSystem(ILog logger = null)
        {
            _logger = logger;
        }

        public void OnChanged(string path, string searchPattern, Action handler)
        {
            const int Delay = 1000;
            var fullPath = Path.Combine(path, searchPattern).ToString();
            var timer = new Timer(state =>
            {
                var self = (Timer) state;
                try
                {
                    self.Change(Timeout.Infinite, Timeout.Infinite);
                    _logger.Info("File changed handler for " + fullPath);
                    handler();
                }
                catch(Exception ex)
                {
                    _logger.Error(ex);
                }
            });
            FileSystemWatcher watcher;
            var key = path + "?" + searchPattern;
            if(watchers.TryGetValue(key, out watcher))
            {
                _logger.Info("There is already a watcher for " + fullPath);
                return;
            }
            watcher = new FileSystemWatcher(path, searchPattern) { NotifyFilter = NotifyFilters.LastWrite, EnableRaisingEvents = true };
            watcher.Error += (sender, args) => _logger.Error(args.GetException());
            watcher.Changed += (sender, args) =>
            {
                _logger.Info("File changed notification for " + args.FullPath);
                timer.Change(Delay, Timeout.Infinite);
            };
            watchers.Add(key, watcher);
        }

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
            var realPath = GetFullPath(path);

            var attributes = File.GetAttributes(realPath);

            if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
                return realPath;
            else
                return Path.GetDirectoryName(realPath);
        }

        public string GetFullPath(string path)
        {
            return Path.GetFullPath(path);
        }
    }
}
