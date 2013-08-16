using System.Collections.Generic;

namespace ScriptCs.Contracts
{
    public interface IFileParser
    {
        void ParseFile(string path, FileParserContext context);

        void ParseCode(string code, FileParserContext context);
    }
}