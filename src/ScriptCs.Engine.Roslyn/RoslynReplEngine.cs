using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Common.Logging;
using Roslyn.Compilers;
using Roslyn.Compilers.Common;
using Roslyn.Scripting;
using ScriptCs.Contracts;

namespace ScriptCs.Engine.Roslyn
{
    public class RoslynReplEngine : RoslynScriptEngine, IReplEngine
    {
        public RoslynReplEngine(IScriptHostFactory scriptHostFactory, ILog logger) : base(scriptHostFactory, logger)
        {
            LocalVariables = new Collection<string>();
        }

        public ICollection<string> LocalVariables { get; private set; }

        protected override ScriptResult Execute(string code, Session session)
        {
            Guard.AgainstNullArgument("session", session);

            if (string.IsNullOrWhiteSpace(FileName) && !IsCompleteSubmission(code))
            {
                return ScriptResult.Incomplete;
            }

            try
            {
                var submission = session.CompileSubmission<object>(code);
                var fields = submission.Compilation.ScriptClass.GetMembers().Where(x => x.Kind == CommonSymbolKind.Field);

                foreach (var field in fields)
                {
                    LocalVariables.Add(field.Name);
                }

                try
                {
                    var result = submission.Execute();
                    return new ScriptResult(returnValue: result);
                }
                catch (Exception ex)
                {
                    return new ScriptResult(executionException: ex);
                }
            }
            catch (Exception ex)
            {
                return new ScriptResult(compilationException: ex);
            }
        }
    }
}