using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCs.Contracts
{
    public interface IScriptResult
    {
        object ReturnValue { get; set; }
        Exception ExecuteException { get; set; }
        Exception CompileException { get; set; }
        bool ContinueBuffering { get; set; }
        bool IsPendingClosingChar { get; set; }
        char? ExpectingClosingChar { get; set; }
        void UpdateClosingExpectation(Exception ex);
    }
}
