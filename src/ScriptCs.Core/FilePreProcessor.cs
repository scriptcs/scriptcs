﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Common.Logging;

using ScriptCs.Contracts;

namespace ScriptCs
{
    public class FilePreProcessor : IFilePreProcessor
    {
        private readonly ILog _logger;

        private readonly IEnumerable<ILineProcessor> _lineProcessors;

        private readonly IFileSystem _fileSystem;

        public FilePreProcessor(IFileSystem fileSystem, ILog logger, IEnumerable<ILineProcessor> lineProcessors)
        {
            _fileSystem = fileSystem;
            _logger = logger;
            _lineProcessors = lineProcessors;
        }

        public virtual FilePreProcessorResult ProcessFile(string path)
        {
            return Process(context => ParseFile(path, context));
        }

        public virtual FilePreProcessorResult ProcessScript(string script)
        {
            var scriptLines = _fileSystem.SplitLines(script).ToList();
            return Process(context => ParseScript(scriptLines, context));
        }

        protected virtual FilePreProcessorResult Process(Action<FileParserContext> parseAction)
        {
            Guard.AgainstNullArgument("parseAction", parseAction);

            var context = new FileParserContext();

            _logger.DebugFormat("Starting pre-processing");

            parseAction(context);

            var code = GenerateCode(context);

            _logger.DebugFormat("Pre-processing finished successfully");

            return new FilePreProcessorResult
            {
                Namespaces = context.Namespaces,
                LoadedScripts = context.LoadedScripts,
                References = context.References,
                Code = code
            };
        }

        protected virtual string GenerateCode(FileParserContext context)
        {
            Guard.AgainstNullArgument("context", context);

            var stringBuilder = new StringBuilder();

            var usingLines = context.Namespaces
                .Where(ns => !string.IsNullOrWhiteSpace(ns))
                .Select(ns => string.Format("using {0};", ns))
                .ToList();

            if (usingLines.Count > 0)
            {
                stringBuilder.AppendLine(string.Join(_fileSystem.NewLine, usingLines));
                stringBuilder.AppendLine(); // Insert a blank separator line
            }

            stringBuilder.Append(string.Join(_fileSystem.NewLine, context.BodyLines));

            return stringBuilder.ToString();
        }

        public virtual void ParseFile(string path, FileParserContext context)
        {
            Guard.AgainstNullArgument("path", path);
            Guard.AgainstNullArgument("context", context);

            var fullPath = _fileSystem.GetFullPath(path);
            var filename = Path.GetFileName(path);

            if (context.LoadedScripts.Contains(fullPath))
            {
                _logger.DebugFormat("Skipping {0} because it's already been loaded.", filename);
                return;
            }

            _logger.DebugFormat("Processing {0}...", filename);

            // Add script to loaded collection before parsing to avoid loop.
            context.LoadedScripts.Add(fullPath);

            var scriptLines = _fileSystem.ReadFileLines(fullPath).ToList();
            
            InsertLineDirective(fullPath, scriptLines);
            InDirectory(fullPath, () => ParseScript(scriptLines, context));
        }

        public virtual void ParseScript(List<string> scriptLines, FileParserContext context)
        {
            Guard.AgainstNullArgument("scriptLines", scriptLines);
            Guard.AgainstNullArgument("context", context);

            var codeIndex = scriptLines.FindIndex(IsNonDirectiveLine);

            for (var index = 0; index < scriptLines.Count; index++)
            {
                var line = scriptLines[index];
                var isBeforeCode = index < codeIndex || codeIndex < 0;

                var wasProcessed = _lineProcessors.Any(x => x.ProcessLine(this, context, line, isBeforeCode));

                if (!wasProcessed)
                {
                    context.BodyLines.Add(line);
                }
            }
        }

        protected virtual void InsertLineDirective(string path, List<string> fileLines)
        {
            Guard.AgainstNullArgument("fileLines", fileLines);

            var bodyIndex = fileLines.FindIndex(line => IsNonDirectiveLine(line) && !IsUsingLine(line));
            if (bodyIndex == -1)
            {
                return;
            }

            var directiveLine = string.Format("#line {0} \"{1}\"", bodyIndex + 1, path);
            fileLines.Insert(bodyIndex, directiveLine);
        }

        private void InDirectory(string path, Action action)
        {
            var oldCurrentDirectory = _fileSystem.CurrentDirectory;
            _fileSystem.CurrentDirectory = _fileSystem.GetWorkingDirectory(path);

            action();

            _fileSystem.CurrentDirectory = oldCurrentDirectory;
        }

        private static bool IsNonDirectiveLine(string line)
        {
            var trimmedLine = line.TrimStart(' ');
            return !trimmedLine.StartsWith("#r ") && !trimmedLine.StartsWith("#load ") && line.Trim() != string.Empty;
        }

        private static bool IsUsingLine(string line)
        {
            return line.TrimStart(' ').StartsWith("using ") && !line.Contains("{") && line.Contains(";");
        }
    }
}
