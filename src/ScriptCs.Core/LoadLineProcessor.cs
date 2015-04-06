using System;

using ScriptCs.Contracts;

namespace ScriptCs
{
    public interface ILoadLineProcessor : ILineProcessor
    {
    }

    public class LoadLineProcessor : DirectiveLineProcessor, ILoadLineProcessor
    {
        private readonly IFileSystem _fileSystem;

        public LoadLineProcessor(IFileSystem fileSystem)
        {
            Guard.AgainstNullArgument("fileSystem", fileSystem);

            _fileSystem = fileSystem;
        }

        protected override string DirectiveName
        {
            get { return "load"; }
        }

        protected override BehaviorAfterCode BehaviorAfterCode
        {
            get { return BehaviorAfterCode.Throw; }
        }

        protected override bool ProcessLine(IFileParser parser, FileParserContext context, string line)
        {
            Guard.AgainstNullArgument("parser", parser);

            var argument = GetDirectiveArgument(line);
            var filePath = Environment.ExpandEnvironmentVariables(argument);

            var fullPath = _fileSystem.GetFullPath(filePath);
            if (!string.IsNullOrWhiteSpace(fullPath))
            {
                parser.ParseFile(fullPath, context);
            }

            return true;
        }
    }
}