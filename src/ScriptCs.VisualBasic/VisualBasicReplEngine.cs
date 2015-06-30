using System.Collections.Generic;
using Common.Logging;
using Microsoft.CodeAnalysis.Scripting;
using ScriptCs.Contracts;
using ScriptCs.Engine.Common;

namespace ScriptCs.VisualBasic
{
    public class VisualBasicReplEngine : VisualBasicScriptEngine, IReplEngine
    {
        public VisualBasicReplEngine(IScriptHostFactory scriptHostFactory, ILog logger)
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