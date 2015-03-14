using System.Collections.Generic;
using Common.Logging;
using Microsoft.CodeAnalysis.Scripting;
using ScriptCs.Contracts;

namespace ScriptCs.CSharp
{
    public class CSharpReplEngine : CSharpScriptEngine, IReplEngine
    {
        public CSharpReplEngine(IScriptHostFactory scriptHostFactory, ILog logger)
            : base(scriptHostFactory, logger)
        {
        }

        public ICollection<string> GetLocalVariables(ScriptPackSession scriptPackSession)
        {
            return this.GetLocalVariables(SessionKey, scriptPackSession);
        }

        protected override ScriptResult Execute(string code, object globals, SessionState<ScriptState> sessionState)
        {
            return string.IsNullOrWhiteSpace(FileName) && !IsCompleteSubmission(code)
                ? ScriptResult.Incomplete
                : base.Execute(code, globals, sessionState);
        }
    }
}