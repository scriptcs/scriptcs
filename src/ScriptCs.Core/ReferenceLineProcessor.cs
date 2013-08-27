using ScriptCs.Contracts;

namespace ScriptCs
{
    public interface IReferenceLineProcessor : ILineProcessor
    {
        string GetArgumentFullPath(string argument);
        string GetDirectiveArgument(string line);
    }

    public class ReferenceLineProcessor : DirectiveLineProcessor, IReferenceLineProcessor
    {
        public ReferenceLineProcessor(IFileSystem fileSystem) : base(fileSystem)
        {
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
            var argument = this.GetDirectiveArgument(line);
            var referencePath = GetArgumentFullPath(argument);

            var referencePathOrName = this.FileSystem.FileExists(referencePath) ? referencePath : argument;

            if (!string.IsNullOrWhiteSpace(referencePathOrName) && !context.References.Contains(referencePathOrName))
            {
                context.References.Add(referencePathOrName);
            }

            return true;
        }
    }
}