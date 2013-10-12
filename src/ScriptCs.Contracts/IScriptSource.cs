using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScriptCs.Contracts
{
    public interface IScriptSource
    {
        Task<List<string>> GetLines();

        string Path { get; }
    }
}
