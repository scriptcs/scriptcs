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

        public virtual FilePreProcessorResult ProcessFile(string path)
        {
            return Parse(context => ParseFile(path, context));
        }

        public virtual FilePreProcessorResult ProcessScript(string script)
        {
            var scriptLines = _fileSystem.SplitLines(script).ToList();
            return Parse(context => ParseScript(scriptLines, context));
        }

        protected virtual FilePreProcessorResult Parse(Action<FilePreProcessorContext> parseAction)
        {
            Guard.AgainstNullArgument("parseAction", parseAction);

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

        protected virtual string GenerateCode(FilePreProcessorContext context)
        {
            Guard.AgainstNullArgument("context", context);

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

        protected virtual void ParseFile(string path, FilePreProcessorContext context)
        {
            _logger.DebugFormat("Processing {0}...", Path.GetFileName(path));

            var currentDirectory = _fileSystem.CurrentDirectory;
            var fileToLoad = _fileSystem.GetFullPath(path);
            var scriptLines = _fileSystem.ReadFileLines(fileToLoad).ToList();

            _fileSystem.CurrentDirectory = _fileSystem.GetWorkingDirectory(fileToLoad);

            ParseScript(scriptLines, context, fileToLoad);

            _fileSystem.CurrentDirectory = currentDirectory;
        }

        protected virtual void ParseScript(List<string> scriptLines, FilePreProcessorContext context, string path = null)
        {
            Guard.AgainstNullArgument("scriptLines", scriptLines);
            Guard.AgainstNullArgument("context", context);

            // Insert line directive if there's a path
            if (path != null) InsertLineDirective(path, scriptLines);

            var codeIndex = scriptLines.FindIndex(PreProcessorUtil.IsNonDirectiveLine);

            for (var index = 0; index < scriptLines.Count; index++)
            {
                ProcessLine(context, scriptLines[index], index < codeIndex || codeIndex < 0);
            }

            if (path != null) context.LoadedScripts.Add(path);
        }

        protected virtual void InsertLineDirective(string path, List<string> fileLines)
        {
            Guard.AgainstNullArgument("fileLines", fileLines);

            var bodyIndex = fileLines.FindIndex(line => PreProcessorUtil.IsNonDirectiveLine(line) && !PreProcessorUtil.IsUsingLine(line));
            if (bodyIndex == -1) return;

            var directiveLine = string.Format("#line {0} \"{1}\"", bodyIndex + 1, path);
            fileLines.Insert(bodyIndex, directiveLine);
        }

        protected virtual void ProcessLine(FilePreProcessorContext context, string line, bool isBeforeCode)
        {
            Guard.AgainstNullArgument("context", context);

            if (PreProcessorUtil.IsUsingLine(line))
            {
                var @using = PreProcessorUtil.GetPath(PreProcessorUtil.UsingString, line, _fileSystem);
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
                    var reference = PreProcessorUtil.GetPath(PreProcessorUtil.RString, line, _fileSystem);
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
                    var filePath = PreProcessorUtil.GetPath(PreProcessorUtil.LoadString, line, _fileSystem);
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

        public class FilePreProcessorContext
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
