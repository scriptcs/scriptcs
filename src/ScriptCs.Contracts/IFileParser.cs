using System.Collections.Generic;

namespace ScriptCs.Contracts
{
    public interface IFileParser
    {
        void ParseFile(string path, FileParserContext context);

        void ParseScript(List<string> scriptLines, FileParserContext context);
    }
}