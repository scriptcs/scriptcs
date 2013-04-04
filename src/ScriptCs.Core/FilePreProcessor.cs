using System.Collections.Generic;
using System.Linq;

namespace ScriptCs
{
    public class FilePreProcessor : IFilePreProcessor
    {
        private const string LoadString = "#load ";
        private const string UsingString = "using ";
        private const string RString = "#r ";

        protected readonly IFileSystem FileSystem;

        public FilePreProcessor(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }

        public FilePreProcessingResult ProcessFile(string path)
        {
            var entryFile = FileSystem.ReadFileLines(path);
            var usings = new List<string>();
            var references = new List<string>();
            var loads = new List<string>();

            var parsed = ParseFile(path, entryFile, ref usings, ref references, ref loads);

            var processedReferences = ProcessReferences(references).ToList();

            var code = GenerateUsings(usings);
            if (!string.IsNullOrWhiteSpace(code))
            {
                code += FileSystem.NewLine;
            }

            code += parsed;

            return new FilePreProcessingResult { Code = code, References = processedReferences };
        }

        private static IEnumerable<string> ProcessReferences(IEnumerable<string> references)
        {
            return references.Select(reference => reference.Replace(RString, string.Empty).Replace("\"", string.Empty));
        }

        protected virtual string GenerateUsings(ICollection<string> usingLines)
        {
            return string.Join(FileSystem.NewLine, usingLines.Distinct());
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
                        var filepath = line.Trim(' ').Replace(LoadString, string.Empty).Replace("\"", string.Empty).Replace(";", string.Empty);
                        var filecontent = FileSystem.IsPathRooted(filepath)
                                              ? FileSystem.ReadFileLines(filepath)
                                              : FileSystem.ReadFileLines(FileSystem.CurrentDirectory + @"\" + filepath);

                        if (filecontent != null)
                        {
                            loads.Add(line);
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

            var result = string.Join(FileSystem.NewLine, fileList.Where(line => !IsUsingLine(line) && !IsRLine(line)));
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