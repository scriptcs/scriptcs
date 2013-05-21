using System;
using System.Collections.Generic;
using System.IO;
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
            return Parse(context => ParseFile(path, context));
        }

        public FilePreProcessorResult ProcessScript(string script)
        {
            var scriptLines = _fileSystem.SplitLines(script).ToList();
            return Parse(context => ParseScript(scriptLines, context));
        }

        private FilePreProcessorResult Parse(Action<FilePreProcessorContext> parseAction)
        {
            var context = new FilePreProcessorContext();

            _logger.DebugFormat("Starting pre-processing");

            parseAction(context);

            var code = GenerateCode(context);

            _logger.DebugFormat("Pre-processing finished successfully");

            return new FilePreProcessorResult
            {
                UsingStatements = context.UsingStatements,
                LoadedScripts = context.LoadedScripts,
                References = context.References,
                Code = code
            };
        }

        private string GenerateCode(FilePreProcessorContext context)
        {
            var stringBuilder = new StringBuilder();

            var usingLines = context.UsingStatements
                .Select(item => string.Format("using {0};", item))
                .ToList();

            if (usingLines.Count > 0)
            {
                stringBuilder.AppendLine(string.Join(_fileSystem.NewLine, usingLines));
                stringBuilder.AppendLine(); // Insert a blank separator line
            }

            stringBuilder.Append(string.Join(_fileSystem.NewLine, context.Body));

            return stringBuilder.ToString();
        }

        private void ParseFile(string path, FilePreProcessorContext context)
        {
            _logger.DebugFormat("Processing {0}...", Path.GetFileName(path));

            var scriptLines = _fileSystem.ReadFileLines(path).ToList();

            ParseScript(scriptLines, context, path);
        }

        private void ParseScript(List<string> scriptLines, FilePreProcessorContext context, string path = null)
        {
            // Insert line directive if there's a path
            if (path != null) InsertLineDirective(path, scriptLines);

            var codeIndex = scriptLines.FindIndex(PreProcessorUtil.IsNonDirectiveLine);

            for (var index = 0; index < scriptLines.Count; index++)
            {
                ProcessLine(context, scriptLines[index], index < codeIndex || codeIndex < 0);
            }

            if (path != null) context.LoadedScripts.Add(path);
        }

        private static void InsertLineDirective(string path, List<string> fileLines)
        {
            var bodyIndex = fileLines.FindIndex(line => PreProcessorUtil.IsNonDirectiveLine(line) && !PreProcessorUtil.IsUsingLine(line));
            if (bodyIndex == -1) return;

            var directiveLine = string.Format("#line {0} \"{1}\"", bodyIndex + 1, path);
            fileLines.Insert(bodyIndex, directiveLine);
        }

        private void ProcessLine(FilePreProcessorContext context, string line, bool isBeforeCode)
        {
            if (PreProcessorUtil.IsUsingLine(line))
            {
                var @using = PreProcessorUtil.GetPath(PreProcessorUtil.UsingString, line);
                if (!context.UsingStatements.Contains(@using))
                {
                    context.UsingStatements.Add(@using);
                }

                return;
            }

            if (PreProcessorUtil.IsRLine(line))
            {
                if (isBeforeCode)
                {
                    var reference = PreProcessorUtil.GetPath(PreProcessorUtil.RString, line);
                    if (!string.IsNullOrWhiteSpace(reference) && !context.References.Contains(reference))
                    {
                        context.References.Add(reference);
                    }
                }

                return;
            }

            if (PreProcessorUtil.IsLoadLine(line))
            {
                if (isBeforeCode)
                {
                    var filePath = PreProcessorUtil.GetPath(PreProcessorUtil.LoadString, line);
                    if (!string.IsNullOrWhiteSpace(filePath) && !context.LoadedScripts.Contains(filePath))
                    {
                        ParseFile(filePath, context);
                    }
                }

                return;
            }

            // If we've reached this, the line is part of the body...
            context.Body.Add(line);
        }

        private class FilePreProcessorContext
        {
            public FilePreProcessorContext()
            {
                UsingStatements = new List<string>();
                References = new List<string>();
                LoadedScripts = new List<string>();
                Body = new List<string>();
            }

            public List<string> UsingStatements { get; private set; }

            public List<string> References { get; private set; }

            public List<string> LoadedScripts { get; private set; }

            public List<string> Body { get; private set; }
        }
    }
}
