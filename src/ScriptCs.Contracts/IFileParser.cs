using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScriptCs.Contracts
{
    public interface IFileParser
    {
        Task ParseScriptSource(IScriptSource scriptSource, FileParserContext context);

        Task ParseScript(List<string> scriptLines, FileParserContext context);
    }
}