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
        }

        public ICollection<string> LocalVariables
        {
            get
            {
                var submissionObjectField = Session.GetType().GetField("submissions", BindingFlags.Instance | BindingFlags.NonPublic);
                var submissionObjects = (submissionObjectField.GetValue(Session) as object[]).Where(x => x != null);
                var variables = new Collection<string>();

                if (submissionObjects.Any())
                {
                    foreach (var submissionObject in submissionObjects)
                    {
                        var fields = submissionObject.GetType().GetFields().Where(x => x.Name.ToLowerInvariant() != "<host-object>");
                        foreach (var field in fields)
                        {
                            variables.Add(string.Format("{0} {1} = {2}", field.FieldType, field.Name, field.GetValue(submissionObject)));
                        }                        
                    }
                }

                return variables;
            }
        }

        protected override ScriptResult Execute(string code, Session session)
        {
            Guard.AgainstNullArgument("session", session);

            if (string.IsNullOrWhiteSpace(FileName) && !IsCompleteSubmission(code))
            {
                return ScriptResult.Incomplete;
            }

            return base.Execute(code, session);
        }
    }
}