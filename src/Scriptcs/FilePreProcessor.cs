using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;

namespace Scriptcs
{
    public class FilePreProcessor : IFilePreProcessor
    {
        private const string LoadString = "#load ";
        private const string UsingString = "using ";

        private readonly IFileSystem _fileSystem;

        [ImportingConstructor]
        public FilePreProcessor(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public string ProcessFile(string path)
        {
            var entryFile = _fileSystem.ReadFileLines(path);
            var parsed = ParseFile(entryFile);

            var result = string.Join(Environment.NewLine, parsed.Item1);
            result += Environment.NewLine + parsed.Item2;

            return result;
        }

        private Tuple<List<string>, string> ParseFile(IEnumerable<string> file)
        {
            var usings = new List<string>();

            var fileList = file.ToList();
            for (var i = 0; i < fileList.Count; i++)
            {
                var line = fileList[i];
                if (IsUsingLine(line))
                {
                    usings.Add(line);
                }

                if (IsLoadLine(line))
                {
                    var filepath = line.Replace(LoadString, "").Replace("\"", "");
                    var filecontent = _fileSystem.IsPathRooted(filepath)
                                              ? _fileSystem.ReadFileLines(filepath)
                                              : _fileSystem.ReadFileLines(_fileSystem.CurrentDirectory + @"\" + filepath);

                    if (filecontent != null)
                    {
                        var parsed = ParseFile(filecontent);
                        fileList[i] = parsed.Item2;
                        usings.AddRange(parsed.Item1);
                    }
                }
            }

            var result = string.Join(Environment.NewLine, fileList.Where(line => !IsUsingLine(line)));
            var tuple = new Tuple<List<string>, string>(usings.Distinct().ToList(), result);

            return tuple;
        }        

        private static bool IsUsingLine(string line)
        {
            return line.StartsWith(UsingString) && !line.Contains("{") && line.Contains(";");
        }

        private static bool IsLoadLine(string line)
        {
            return line.StartsWith(LoadString);
        }
    }
}