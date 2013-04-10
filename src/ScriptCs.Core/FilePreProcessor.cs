using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NuGet;

namespace ScriptCs
{
    public class FilePreProcessor : IFilePreProcessor
    {
        private const string LoadString = "#load ";
        private const string UsingString = "using ";
        private const string RString = "#r ";

        protected readonly IFileSystem _fileSystem;

        public FilePreProcessor(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public string ProcessFile(string path)
        {
            //var entryFile = _fileSystem.ReadFileLines(path);
            var parseResult = ParseFile(_fileSystem, path);

            var result = GenerateRs(parseResult.Rs);
            if (!string.IsNullOrWhiteSpace(result))
            {
                result += _fileSystem.NewLine;
            }

            result += GenerateUsings(parseResult.Usings);
            if (!string.IsNullOrWhiteSpace(result))
            {
                result += _fileSystem.NewLine;
            }

            result += parseResult.Parsed;

            return result.TrimEnd(_fileSystem.NewLine.ToCharArray());
        }

        protected virtual string GenerateUsings(ICollection<string> usingLines)
        {
            return string.Join(_fileSystem.NewLine, usingLines.Distinct());
        }

        protected virtual string GenerateRs(ICollection<string> rLines)
        {
            return string.Join(_fileSystem.NewLine, rLines.Distinct());
        }

        private static ParseResult ParseFile(IFileSystem fileSystem, string path) //, IEnumerable<string> file
        {
            var parseResult = new ParseResult();

            var file = GetFileContentFromPath(fileSystem, path);
            if (file == null) return parseResult;

            var fileList = file.ToList();
            var firstCode = fileList.FindIndex(IsNonDirectiveLine);

            var firstBody = fileList.FindIndex(l => IsNonDirectiveLine(l) && !IsUsingLine(l));

            // add #line before the actual code begins
            // +1 because we are in a zero indexed list, but line numbers are 1 indexed
            // we need to keep the original position of the actual line 
            if (firstBody != -1)
            {
                fileList.Insert(firstBody, string.Format(@"#line {0} ""{1}""", firstBody + 1, path));
            }
            
            for (var i = 0; i < fileList.Count; i++)
            {
                var line = fileList[i];
                if (IsUsingLine(line))
                {
                    parseResult.Usings.Add(line);
                    continue;
                }

                if (IsRLine(line) && i < firstCode)
                {
                    parseResult.Rs.Add(line);
                    continue;
                }

                if (IsLoadLine(line) && (i < firstCode || firstCode < 0) && !parseResult.Loads.Contains(line))
                {
                    var filepath = GetFilePathFromLine(line);

                    var result = ParseFile(fileSystem, filepath);
                    parseResult.Combine(result);

                    continue;
                }
                
                parseResult.Code.Add(line + fileSystem.NewLine);
            }

            return parseResult;
        }

        private static string GetFilePathFromLine(string line)
        {
            return line.Trim(' ')
                       .Replace(LoadString, string.Empty)
                       .Replace("\"", string.Empty)
                       .Replace(";", string.Empty);
        }

        private static IEnumerable<string> GetFileContentFromPath(IFileSystem fileSystem, string filepath)
        {
            var isPathRooted = fileSystem.IsPathRooted(filepath);
            return isPathRooted
                       ? fileSystem.ReadFileLines(filepath)
                       : fileSystem.ReadFileLines(fileSystem.CurrentDirectory + @"\" + filepath);
        }

        private static bool IsNonDirectiveLine(string line)
        {
            return !IsRLine(line) && !IsLoadLine(line) && line.Trim() != string.Empty;
        }

        private static bool IsUsingLine(string line)
        {
            return line.TrimStart(' ').StartsWith(UsingString) && !line.Contains("{") && line.Contains(";");
        }

        private static bool IsRLine(string line)
        {
            return line.TrimStart(' ').StartsWith(RString);
        }

        private static bool IsLoadLine(string line)
        {
            return line.TrimStart(' ').StartsWith(LoadString);
        }

        private enum ParsingStatus
        {
            ForbidReferences,
            
            ForbidLoads,

            ForbidUsings,

            NoRestrictions
        }

        private class ParseResult
        {
            public ParseResult()
            {
                Usings = new HashSet<string>();
                Rs = new HashSet<string>();
                Loads = new List<string>();
                Code = new List<string>();
            }

            public string Parsed
            {
                get
                {
                    var stringBuilder = new StringBuilder();

                    Code.ForEach(l => stringBuilder.Append(l));

                    return stringBuilder.ToString();
                }
            }

            public HashSet<string> Usings { get; private set; }
            public HashSet<string> Rs { get; private set; }
            public List<string> Loads { get; private set; }
            public List<string> Code { get; private set; }

            public void Combine(ParseResult parseResult)
            {
                Usings.AddRange(parseResult.Usings);
                Rs.AddRange(parseResult.Rs);
                Loads.AddRange(parseResult.Loads);
                Code.AddRange(parseResult.Code);
            }
        }
    }
}