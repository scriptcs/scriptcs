using System;
using System.IO;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public interface IReferenceLineProcessor : ILineProcessor
    {
    }

    public class ReferenceLineProcessor : DirectiveLineProcessor, IReferenceLineProcessor
    {
        private readonly IFileSystem _fileSystem;

        public ReferenceLineProcessor(IFileSystem fileSystem)
        {
            Guard.AgainstNullArgument("fileSystem", fileSystem);

            _fileSystem = fileSystem;
        }

        protected override string DirectiveName
        {
            get { return "r"; }
        }

        protected override BehaviorAfterCode BehaviorAfterCode
        {
            get { return BehaviorAfterCode.Throw; }
        }

        protected override bool ProcessLine(IFileParser parser, FileParserContext context, string line)
        {
            Guard.AgainstNullArgument("context", context);

            var argument = GetDirectiveArgument(line);
            var assemblyPath = Environment.ExpandEnvironmentVariables(argument);

            var referencePath = _fileSystem.GetFullPath(assemblyPath);
            var referencePathOrName = _fileSystem.FileExists(referencePath) ? referencePath : argument;

            if (!string.IsNullOrWhiteSpace(referencePathOrName) && !context.References.Contains(referencePathOrName))
            {
                context.References.Add(referencePathOrName);
            }

            return true;
        }
    }
}