using ScriptCs.Contracts;

namespace ScriptCs
{
    public interface ILoadLineProcessor : ILineProcessor
    {
        string GetArgumentFullPath(string line);
    }

    public class LoadLineProcessor : DirectiveLineProcessor, ILoadLineProcessor
    {
        public LoadLineProcessor(IFileSystem fileSystem) :  base(fileSystem)
        {
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
            var fullPath = GetArgumentFullPath(line);
            if (!string.IsNullOrWhiteSpace(fullPath))
            {
                parser.ParseFile(fullPath, context);
            }

            return true;
        }
    }
}