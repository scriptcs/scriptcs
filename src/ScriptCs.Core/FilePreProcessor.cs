using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Common.Logging;

namespace ScriptCs
{
    public class FilePreProcessor : IFilePreProcessor
    {
        private readonly ILog _logger;

        private readonly IFileSystem _fileSystem;

        public FilePreProcessor(IFileSystem fileSystem, ILog logger)
        {
            _fileSystem = fileSystem;
            _logger = logger;
        }

        public FilePreProcessorResult ProcessFile(string path)
        {
            var context = new FilePreProcessContext();

            ParseFile(path, context);

            var code = GenerateScript(context);

            return new FilePreProcessorResult
            {
                Usings = context.Usings,
                LoadedFiles = context.LoadedScripts,
                References = context.References,
                Code = code
            };
        }

        private static string GenerateScript(FilePreProcessContext context)
        {
            var stringBuilder = new StringBuilder();

            AppendUsings(stringBuilder, context.Usings);

            stringBuilder.Append(string.Join(Environment.NewLine, context.Body));

            return stringBuilder.ToString();
        }

        private static void AppendUsings(StringBuilder stringBuilder, IEnumerable<string> items)
        {
            var lines = items.Distinct().Select(item => string.Format("using {0};", item)).ToList();

            if (lines.Count == 0) return;

            stringBuilder.AppendLine(string.Join(Environment.NewLine, lines));
            stringBuilder.AppendLine(); // Insert a blank separator line
        }

        private void ParseFile(string path, FilePreProcessContext context)
        {
            var fileLines = _fileSystem.ReadFileLines(path).ToList();

            InsertLineDirective(path, fileLines);

            var codeIndex = fileLines.FindIndex(PreProcessorUtil.IsNonDirectiveLine);

            for (var index = 0; index < fileLines.Count; index++)
            {
                ProcessLine(context, fileLines[index], index < codeIndex || codeIndex < 0);
            }

            context.LoadedScripts.Add(path);
        }

        private static void InsertLineDirective(string path, List<string> fileLines)
        {
            var bodyIndex = fileLines.FindIndex(line => PreProcessorUtil.IsNonDirectiveLine(line) && !PreProcessorUtil.IsUsingLine(line));
            if (bodyIndex == -1) return;

            var directiveLine = string.Format("#line {0} \"{1}\"", bodyIndex + 1, path);
            fileLines.Insert(bodyIndex, directiveLine);
        }

        private void ProcessLine(FilePreProcessContext context, string line, bool isBeforeCode)
        {
            if (PreProcessorUtil.IsUsingLine(line))
            {
                var @using = GetUsing(line);
                if (!context.Usings.Contains(@using))
                {
                    context.Usings.Add(@using);
                }
                
                return;
            }

            if (PreProcessorUtil.IsRLine(line))
            {
                if (isBeforeCode)
                {
                    var reference = GetReference(line);
                    if (!context.References.Contains(reference))
                    {
                        context.References.Add(reference);
                    }
                }

                return;
            }

            if (PreProcessorUtil.IsLoadLine(line))
            {
                var filePath = PreProcessorUtil.GetPath(PreProcessorUtil.LoadString, line);

                if (isBeforeCode && !context.LoadedScripts.Contains(filePath))
                {
                    ParseFile(filePath, context);
                }

                return;
            }

            // If we've reached this, the line is part of the body...
            context.Body.Add(line);
        }

        private static string GetUsing(string line)
        {
            return line.TrimStart(' ').Replace(PreProcessorUtil.UsingString, string.Empty).Replace(";", string.Empty);
        }

        private static string GetReference(string line)
        {
            return line.TrimStart(' ').Replace(PreProcessorUtil.RString, string.Empty).Replace("\"", string.Empty);
        }

        private class FilePreProcessContext
        {
            public FilePreProcessContext()
            {
                Usings = new List<string>();
                References = new List<string>();
                LoadedScripts = new List<string>();
                Body = new List<string>();
            }

            public List<string> Usings { get; private set; }

            public List<string> References { get; private set; }

            public List<string> LoadedScripts { get; private set; }

            public List<string> Body { get; private set; }
        }
    }
}