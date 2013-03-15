﻿using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            var entryFile = _fileSystem.ReadFileLines(path);
            var usings = new List<string>();
            var rs = new List<string>();
            var loads = new List<string>();

            var parsed = ParseFile(path, entryFile, ref usings, ref rs, ref loads);

            var result = GenerateRs(rs);
            if (!string.IsNullOrWhiteSpace(result))
            {
                result += _fileSystem.NewLine;
            }

            result += GenerateUsings(usings);
            if (!string.IsNullOrWhiteSpace(result))
            {
                result += _fileSystem.NewLine;
            }

            result += parsed;

            return result;
        }

        protected virtual string GenerateUsings(ICollection<string> usingLines)
        {
            return string.Join(_fileSystem.NewLine, usingLines.Distinct());
        }

        protected virtual string GenerateRs(ICollection<string> rLines)
        {
            return string.Join(_fileSystem.NewLine, rLines.Distinct());
        }

        private string ParseFile(string path, IEnumerable<string> file, ref List<string> usings, ref List<string> rs, ref List<string> loads)
        {
            var fileList = file.ToList();
            var firstCode = fileList.FindIndex(l => IsNonDirectiveLine(l));

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
                    usings.Add(line);
                } 
                else if (IsRLine(line))
                {
                    if (i < firstCode)
                    {
                        rs.Add(line);
                    }
                    else
                    {
                        fileList[i] = string.Empty;
                    }
                } 
                else if (IsLoadLine(line))
                {
                    if (i < firstCode && !loads.Contains(line))
                    {
                        var filepath = line.Trim(' ').Replace(LoadString, string.Empty).Replace("\"", string.Empty).Replace(";", string.Empty);
                        var filecontent = _fileSystem.IsPathRooted(filepath)
                                              ? _fileSystem.ReadFileLines(filepath)
                                              : _fileSystem.ReadFileLines(_fileSystem.CurrentDirectory + Path.DirectorySeparatorChar + filepath);

                        if (filecontent != null)
                        {
                            loads.Add(line);
                            var parsed = ParseFile(filepath, filecontent, ref usings, ref rs, ref loads);
                            fileList[i] =/* string.Format("//{0}{1}", filepath, _fileSystem.NewLine) + */parsed;
                        }
                    }
                    else
                    {
                        fileList[i] = string.Empty;
                    }
                }
            }

            var result = string.Join(_fileSystem.NewLine, fileList.Where(line => !IsUsingLine(line) && !IsRLine(line)));
            return result;
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
    }
}