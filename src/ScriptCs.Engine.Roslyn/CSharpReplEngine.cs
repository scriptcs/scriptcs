using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Scripting;
using ScriptCs.Contracts;

namespace ScriptCs.Engine.Roslyn
{
    public class CSharpReplEngine : CSharpScriptEngine, IReplEngine
    {
        public CSharpReplEngine(IScriptHostFactory scriptHostFactory, ILogProvider logProvider)
            : base(scriptHostFactory, logProvider)
        {
        }

        public ICollection<string> GetLocalVariables(ScriptPackSession scriptPackSession)
        {
            return this.GetLocalVariables(SessionKey, scriptPackSession);
        }

        protected override ScriptResult Execute(string code, object globals, SessionState<ScriptState> sessionState)
        {
            if (string.IsNullOrWhiteSpace(FileName) && !IsCompleteSubmission(code))
                return ScriptResult.Incomplete;

            if (sessionState.Session != null)
            {
                try
                {
                    Log.Debug("Starting subsequent REPL execution");
                    var result = sessionState.Session.ContinueWithAsync(code, ScriptOptions).GetAwaiter().GetResult();
                    Log.Debug("Finished subsequent REPL execution");
                    sessionState.Session = result;
                    return new ScriptResult(returnValue: result.ReturnValue);
                }
                catch (AggregateException ex)
                {
                    return new ScriptResult(executionException: ex.InnerException);
                }
                catch (CompilationErrorException ex)
                {
                    return new ScriptResult(compilationException: ex);
                }
                catch (Exception ex)
                {
                    return new ScriptResult(executionException: ex);
                }
            }

            return base.Execute(code, globals, sessionState);
        }
    }
}