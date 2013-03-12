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

        [ImportingConstructor]
        public FilePreProcessor(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public string ProcessFile(string path)
        {
            var fileLines = _fileSystem.ReadFileLines(path);
            var parsedFileResult = ParseFile(fileLines);

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

            foreach (var bodyLine in parsedFileResult.Body)
            {
                builder.AppendLine(bodyLine);
            }

            return builder.ToString();
        }

        private ParsedFileResult ParseFile(IEnumerable<string> lines)
        {
            var script = new ParsedFileResult();

            foreach (var line in lines)
            {
                if (IsLoadLine(line))
                {
                    var fileContent = LoadFileContents(line);
                    if (fileContent != null)
                    {
                        var subScript = ParseFile(fileContent);
                        script.SubScripts.Add(subScript);
                    }

                    // TODO: Raise an exception?

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

        private IEnumerable<string> LoadFileContents(string line)
        {
            var filePath = GetFilePath(line);

            var fileContent = !_fileSystem.IsPathRooted(filePath)
                ? _fileSystem.ReadFileLines(_fileSystem.CurrentDirectory + "\\" + filePath)
                : _fileSystem.ReadFileLines(filePath);

            return fileContent;
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

        private static string GetFilePath(string line)
        {
            return line.TrimStart(' ').Replace(LoadString, string.Empty).Replace("\"", string.Empty).Replace(";", string.Empty);
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
                get { return Usings.Union(SubScripts.SelectMany(script => script.Usings)).Distinct(); }
            }

            public IEnumerable<string> RecursiveReferences
            {
                get { return References.Union(SubScripts.SelectMany(script => script.References)).Distinct(); }
            }
        }
    }
}