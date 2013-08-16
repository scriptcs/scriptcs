namespace ScriptCs.Contracts
{
    public interface ILineProcessor
    {
        bool ProcessLine(IFileParser parser, FileParserContext context, string line, bool isBeforeCode);
    }
}