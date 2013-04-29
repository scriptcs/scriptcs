using System.Collections.Generic;
using System.Linq;
using Common.Logging;

namespace ScriptCs
{
    public class FilePreProcessor : IFilePreProcessor
    {
        private const string UsingString = "using ";

        private const string LoadDirective = "#load ";

        private const string ReferenceDirective = "#r ";

        private readonly ILog _logger;

        private readonly IFileSystem _fileSystem;

        public FilePreProcessor(IFileSystem fileSystem, ILog logger)
        {
            _fileSystem = fileSystem;
            _logger = logger;
        }

        public FilePreProcessingResult ProcessFile(string path)
        {
            _logger.DebugFormat("{0} - Reading lines", path);
            var entryFile = _fileSystem.ReadFileLines(path);
            var usings = new List<string>();
            var references = new List<string>();
            var loads = new List<string>();

            _logger.DebugFormat("{0} - Parsing ", path);
            var parsedCode = ParseFile(path, entryFile, ref usings, ref references, ref loads);

            _logger.DebugFormat("{0} - Generating references (#r)", path);
            var processedReferences = ProcessReferences(references).ToList();

            _logger.DebugFormat("{0} - Generating using statements", path);
            var code = GenerateUsings(usings);
            if (!string.IsNullOrWhiteSpace(code))
            {
                code += _fileSystem.NewLine;
            }

            code += parsedCode;

            return new FilePreProcessingResult { Code = code, References = processedReferences };
        }

        private static IEnumerable<string> ProcessReferences(IEnumerable<string> references)
        {
            return references.Select(reference => reference.Replace(ReferenceDirective, string.Empty).Replace("\"", string.Empty));
        }

        private string GenerateUsings(IEnumerable<string> usingLines)
        {
            return string.Join(_fileSystem.NewLine, usingLines.Distinct());
        }

        private string ParseFile(string path, IEnumerable<string> file, ref List<string> usings, ref List<string> rs, ref List<string> loads)
        {
            var fileList = file.ToList();
            var firstCode = fileList.FindIndex(IsNonDirectiveLine);

            var firstBody = fileList.FindIndex(l => IsNonDirectiveLine(l) && !IsUsingLine(l));

            // add #line before the actual code begins
            // +1 because we are in a zero indexed list, but line numbers are 1 indexed
            // we need to keep the original position of the actual line 
            if (firstBody != -1)
            {   
                _logger.DebugFormat("Added #line statement for file {0} at line {1}", path, firstBody);
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
                    if ((i < firstCode || firstCode < 0) && !loads.Contains(line))
                    {
                        var filepath = line.Trim(' ').Replace(LoadDirective, string.Empty).Replace("\"", string.Empty).Replace(";", string.Empty);
                        var filecontent = _fileSystem.IsPathRooted(filepath)
                                              ? _fileSystem.ReadFileLines(filepath)
                                              : _fileSystem.ReadFileLines(_fileSystem.CurrentDirectory + @"\" + filepath);

                        if (filecontent != null)
                        {
                            loads.Add(line);
                            _logger.DebugFormat("Parsing file {0}", path);
                            var parsed = ParseFile(filepath, filecontent, ref usings, ref rs, ref loads);
                            fileList[i] = parsed;
                        }
                    }
                    else
                    {
                        fileList[i] = string.Empty;
                    }
                }
            }

            return string.Join(_fileSystem.NewLine, fileList.Where(line => !IsUsingLine(line) && !IsRLine(line)));
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
            return line.TrimStart(' ').StartsWith(ReferenceDirective);
        }

        private static bool IsLoadLine(string line)
        {
            return line.TrimStart(' ').StartsWith(LoadDirective);
        }
    }

    public class FilePreProcessingResult
    {
        public FilePreProcessingResult()
        {
            References = new List<string>();
        }

        public string Code { get; set; }

        public List<string> References { get; set; }
    }
}
