using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCs.Contracts
{
    public interface IFilePreProcessorResult
    {
        List<string> UsingStatements { get; set; }
        List<string> LoadedScripts { get; set; }
        List<string> References { get; set; }
        string Code { get; set; }
    }
}
