using ScriptCs.Contracts;

namespace ScriptCs
{
    public interface ILoadLineProcessor : ILineProcessor
    {
        string GetArgumentFullPath(string argument);
        string GetDirectiveArgument(string line);
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
            var argument = GetDirectiveArgument(line);
            var fullPath = GetArgumentFullPath(argument);
            if (!string.IsNullOrWhiteSpace(fullPath))
            {
                parser.ParseFile(fullPath, context);
            }

            return true;
        }
    }
}