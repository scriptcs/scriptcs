using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Common.Logging;
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

                try
                {
                    var result = submission.Execute();

                    var submissionObjectField = session.GetType().GetField("submissions", BindingFlags.Instance | BindingFlags.NonPublic);
                    var submissionObject = (submissionObjectField.GetValue(session) as object[]).LastOrDefault(x => x != null);

                    if (submissionObject != null)
                    {
                        var fields = submissionObject.GetType().GetFields().Where(x => x.Name.ToLowerInvariant() != "<host-object>");
                        foreach (var field in fields)
                        {
                            LocalVariables.Add(string.Format("{0} {1} = {2}", field.FieldType, field.Name, field.GetValue(submissionObject)));
                        }
                    }

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