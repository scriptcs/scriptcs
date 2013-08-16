namespace ScriptCs.Contracts
{
    public interface IFileParser
    {
        void ParseFile(string path, FileParserContext context);

        void ParseScript(string script, FileParserContext context);
    }
}