using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace ScriptCs
{
    [Export(Constants.RunContractName, typeof(IFilePreProcessor))]
    public class FilePreProcessor : IFilePreProcessor
    {
        private const string ReferenceString = "#r ";
        private const string LoadString = "#load ";
        private const string UsingString = "using ";

        private readonly IFileSystem _fileSystem;
        private readonly List<string> _loadedPaths;
            
        [ImportingConstructor]
        public FilePreProcessor(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            _loadedPaths = new List<string>();
        }

        public string ProcessFile(string path)
        {
            var parsedFileResult = ParseFile(path);

            var generatedScript = GenerateScript(parsedFileResult);

            return generatedScript;
        }

        protected virtual string GenerateScript(ParsedFileResult parsedFileResult)
        {
            var builder = new StringBuilder();

            foreach (var reference in parsedFileResult.RecursiveReferences)
            {
                builder.AppendLine("#r " + reference);
            }

            foreach (var @using in parsedFileResult.RecursiveUsings)
            {
                builder.AppendLine("using " + @using + ";");
            }

            foreach (var bodyLine in parsedFileResult.RecursiveBody)
            {
                builder.AppendLine(bodyLine);
            }

            return builder.ToString();
        }

        private ParsedFileResult ParseFile(string path)
        {
            // Check if we've already loaded this file...
            if (_loadedPaths.Contains(path)) return null;

            _loadedPaths.Add(path);

            var script = new ParsedFileResult();

            foreach (var line in _fileSystem.ReadFileLines(path))
            {
                if (IsLoadLine(line))
                {
                    var filePath = GetFilePath(line);

                    var subScript = ParseFile(filePath);
                    if (subScript == null)
                    {
                        // File has already been loaded, skip
                        continue;
                    }

                    script.SubScripts.Add(subScript);
                    continue;
                }

                if (IsUsingLine(line))
                {
                    var @using = GetUsing(line);
                    script.Usings.Add(@using);
                    continue;
                }

                if (IsReferenceLine(line))
                {
                    var reference = GetReference(line);
                    script.References.Add(reference);
                    continue;
                }

                // If we've reached this, the line is part of the body...
                script.Body.Add(line);
            }

            return script;
        }

        private static bool IsUsingLine(string line)
        {
            return line.TrimStart(' ').StartsWith(UsingString) && !line.Contains("{") && line.Contains(";");
        }

        private static bool IsReferenceLine(string line)
        {
            return line.TrimStart(' ').StartsWith(ReferenceString);
        }

        private static bool IsLoadLine(string line)
        {
            return line.TrimStart(' ').StartsWith(LoadString);
        }

        private static string GetUsing(string line)
        {
            return line.TrimStart(' ').Replace(UsingString, string.Empty).Replace(";", string.Empty);
        }

        private static string GetReference(string line)
        {
            return line.TrimStart(' ').Replace(ReferenceString, string.Empty).Replace("\"", string.Empty);
        }

        private string GetFilePath(string line)
        {
            var path = line.TrimStart(' ')
                .Replace(LoadString, string.Empty)
                .Replace("\"", string.Empty)
                .Replace(";", string.Empty);

            if (!_fileSystem.IsPathRooted(path))
                path = _fileSystem.CurrentDirectory + "\\" + path;

            return path;
        }

        protected class ParsedFileResult
        {
            public ParsedFileResult()
            {
                SubScripts = new List<ParsedFileResult>();
                Usings = new List<string>();
                References = new List<string>();
                Body = new List<string>();
            }

            public List<ParsedFileResult> SubScripts { get; private set; }

            public List<string> Usings { get; private set; }

            public List<string> References { get; private set; }

            public List<string> Body { get; private set; }

            public IEnumerable<string> RecursiveUsings
            {
                get { return GetRecursive(Usings, script => script.RecursiveUsings); }
            }

            public IEnumerable<string> RecursiveReferences
            {
                get { return GetRecursive(References, script => script.RecursiveReferences); }
            }

            public IEnumerable<string> RecursiveBody
            {
                get { return GetRecursive(Body, script => script.RecursiveBody); }
            }

            private IEnumerable<string> GetRecursive(
                IEnumerable<string> list, Func<ParsedFileResult, IEnumerable<string>> selector)
            {
                return list.Union(SubScripts.SelectMany(selector));
            }
        }
    }
}