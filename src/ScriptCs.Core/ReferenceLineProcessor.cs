using ScriptCs.Contracts;
using System.IO;
namespace ScriptCs
{
    public interface IReferenceLineProcessor : ILineProcessor
    {
        string GetArgumentFullPath(string line);
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
            var referencePath = GetArgumentFullPath(line);
            var referencePathOrName = _fileSystem.FileExists(referencePath) ? referencePath : argument;

            if (!string.IsNullOrWhiteSpace(referencePathOrName) && !context.References.Contains(referencePathOrName))
            {
                context.References.Add(referencePathOrName);
            }

            return true;
        }
    }
}