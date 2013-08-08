using System;

namespace ScriptCs
{
    public class LoadLineProcessor : DirectiveLineProcessor
    {
        private readonly IFileSystem _fileSystem;

        public LoadLineProcessor(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        protected override string DirectiveName
        {
            get { return "load"; }
        }

        protected override bool IgnoreAfterCode
        {
            get { return true; }
        }

        protected override bool ProcessLine(IFileParser parser, FileParserContext context, string line)
        {
            var argument = GetDirectiveArgument(line);
            var filePath = Environment.ExpandEnvironmentVariables(argument);

            var fullPath = _fileSystem.GetFullPath(filePath);
            if (!string.IsNullOrWhiteSpace(fullPath) && !context.LoadedScripts.Contains(fullPath))
            {
                parser.ParseFile(fullPath, context);
            }

            return true;
        }
    }
}