using System;
using System.Collections.Generic;
using System.Linq;
using Common.Logging;
using Roslyn.Scripting;
using Roslyn.Scripting.CSharp;

using ScriptCs.Contracts;

namespace ScriptCs.Engine.Roslyn
{
    using System.Runtime.ExceptionServices;

    public class RoslynScriptEngine : IScriptEngine
    {
        protected readonly ScriptEngine ScriptEngine;
        private readonly IScriptHostFactory _scriptHostFactory;
        public const string SessionKey = "Session";

        public RoslynScriptEngine(IScriptHostFactory scriptHostFactory, ILog logger)
        {
            ScriptEngine = new ScriptEngine();
            ScriptEngine.AddReference(typeof(ScriptExecutor).Assembly);
            _scriptHostFactory = scriptHostFactory;
            Logger = logger;
        }

        protected ILog Logger { get; private set; }
        
        public string BaseDirectory
        {
            get { return ScriptEngine.BaseDirectory;  }
            set { ScriptEngine.BaseDirectory = value; }
        }

        public string FileName { get; set; }

        public ScriptResult Execute(string code, string[] scriptArgs, IEnumerable<string> references, IEnumerable<string> namespaces, ScriptPackSession scriptPackSession)
        {
            Guard.AgainstNullArgument("scriptPackSession", scriptPackSession);

            Logger.Debug("Starting to create execution components");
            Logger.Debug("Creating script host");
            
            var distinctReferences = references.Union(scriptPackSession.References).Distinct().ToList();
            SessionState<Session> sessionState;

            if (!scriptPackSession.State.ContainsKey(SessionKey))
            {
                var host = _scriptHostFactory.CreateScriptHost(new ScriptPackManager(scriptPackSession.Contexts), scriptArgs);
                Logger.Debug("Creating session");

                var hostType = host.GetType();
                ScriptEngine.AddReference(hostType.Assembly);
                var session = ScriptEngine.CreateSession(host, hostType);

                foreach (var reference in distinctReferences)
                {
                    Logger.DebugFormat("Adding reference to {0}", reference);
                    session.AddReference(reference);
                }

                foreach (var @namespace in namespaces.Union(scriptPackSession.Namespaces).Distinct())
                {
                    Logger.DebugFormat("Importing namespace {0}", @namespace);
                    session.ImportNamespace(@namespace);
                }

                sessionState = new SessionState<Session> { References = distinctReferences, Session = session };
                scriptPackSession.State[SessionKey] = sessionState;
            }
            else
            {
                Logger.Debug("Reusing existing session");
                sessionState = (SessionState<Session>)scriptPackSession.State[SessionKey];

                var newReferences = sessionState.References == null || !sessionState.References.Any() ? distinctReferences : distinctReferences.Except(sessionState.References);
                if (newReferences.Any())
                {
                    foreach (var reference in newReferences)
                    {
                        Logger.DebugFormat("Adding reference to {0}", reference);
                        sessionState.Session.AddReference(reference);
                    }

                    sessionState.References = newReferences;
                }
            }

            Logger.Debug("Starting execution");
            var result = Execute(code, sessionState.Session);
            Logger.Debug("Finished execution");
            return result;
        }

        protected virtual ScriptResult Execute(string code, Session session)
        {
            Guard.AgainstNullArgument("session", session);

            var result = new ScriptResult();
            try
            {
                var submission = session.CompileSubmission<object>(code);
                try
                {
                    result.ReturnValue = submission.Execute();
                }
                catch (Exception ex)
                {
                    result.ExecuteExceptionInfo = ExceptionDispatchInfo.Capture(ex);
                }
            }
            catch (Exception ex)
            {
                 result.UpdateClosingExpectation(ex);
                if (!result.IsPendingClosingChar)
                {
                    result.CompileExceptionInfo = ExceptionDispatchInfo.Capture(ex);
                }
            }

            return result;
        }
    }
}