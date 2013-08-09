using System;

namespace ScriptCs
{
    public interface IReferenceLineProcessor : ILineProcessor { }

    public class ReferenceLineProcessor : DirectiveLineProcessor, IReferenceLineProcessor
    {
        private readonly IFileSystem _fileSystem;

        public ReferenceLineProcessor(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        protected override string DirectiveName
        {
            get { return "r"; }
        }

        protected override bool IgnoreAfterCode
        {
            get { return true; }
        }

        protected override bool ProcessLine(IFileParser parser, FileParserContext context, string line)
        {
            var argument = GetDirectiveArgument(line);
            var assemblyPath = Environment.ExpandEnvironmentVariables(argument);

            var referencePath = _fileSystem.GetFullPath(assemblyPath);
            if (!string.IsNullOrWhiteSpace(referencePath) && !context.References.Contains(referencePath))
            {
                context.References.Add(referencePath);
            }

            return true;
        }
    }
}