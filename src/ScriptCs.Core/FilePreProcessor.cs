using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptCs
{
    public class FilePreProcessor : IFilePreProcessor
    {
        private const string LoadString = "#load ";
        private const string UsingString = "using ";

        protected readonly IFileSystem _fileSystem;

        public FilePreProcessor(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public string ProcessFile(string path)
        {
            var lines = _fileSystem.ReadFileLines(path);
            var parsed = ParseFile(path, lines);

            // item1 === usings; item2 === code
            var result = GenerateUsings(parsed.Item1);
            result += _fileSystem.NewLine + parsed.Item2;

            return result;
        }

        protected virtual string GenerateUsings(ICollection<string> usingLines)
        {
            return string.Join(_fileSystem.NewLine, usingLines);
        }

        private Tuple<List<string>, string> ParseFile(string path, IEnumerable<string> lines)
        {
            var usings = new List<string>();
            var linesList = lines.ToList();

            var parsingStatus = ParsingStatus.NoRestrictions;

            for (var i = 0; i < linesList.Count; i++)
            {
                var line = linesList[i];
                if (IsUsingLine(line))
                {
                    usings.Add(line);
                }
                else if (IsLoadLine(line))
                {
                    if (parsingStatus == ParsingStatus.NoRestrictions)
                    {
                        parsingStatus = ParsingStatus.ForbidUsings;
                    }

                    var filepath =
                        line.Trim(' ')
                            .Replace(LoadString, string.Empty)
                            .Replace("\"", string.Empty)
                            .Replace(";", string.Empty);
                    var filecontent = _fileSystem.IsPathRooted(filepath)
                                          ? _fileSystem.ReadFileLines(filepath)
                                          : _fileSystem.ReadFileLines(_fileSystem.CurrentDirectory + @"\" + filepath);

                    if (filecontent != null)
                    {
                        var parsed = ParseFile(filepath, filecontent);
                        linesList[i] = parsed.Item2;
                        usings.AddRange(parsed.Item1);
                    }
                }
                else if (parsingStatus != ParsingStatus.ForbidLoads)
                {
                    // we are in the first body line (until we add support for #r)
                    // add #line statement
                    parsingStatus = ParsingStatus.ForbidLoads;

                    // +1 because we are in a zero indexed list, but line numbers are 1 indexed
                    // we need to keep the original position of the actual line 
                    linesList.Insert(i, string.Format(@"#line {0} ""{1}""", i + 1, path));
                }
            }

            var result = string.Join(_fileSystem.NewLine, linesList.Where(line => !IsUsingLine(line)));
            var tuple = new Tuple<List<string>, string>(usings.Distinct().ToList(), result);

            return tuple;
        }        

        private static bool IsUsingLine(string line)
        {
            return line.TrimStart(' ').StartsWith(UsingString) && !line.Contains("{") && line.Contains(";");
        }

        private static bool IsLoadLine(string line)
        {
            return line.TrimStart(' ').StartsWith(LoadString);
        }

        [Flags]
        private enum ParsingStatus
        {
            ForbidLoads,

            ForbidUsings,

            NoRestrictions
        }
    }
}