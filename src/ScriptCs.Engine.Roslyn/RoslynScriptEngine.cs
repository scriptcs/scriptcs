using System;
using System.Collections.Generic;
using System.Linq;
using Common.Logging;
using Roslyn.Scripting;
using Roslyn.Scripting.CSharp;

using ScriptCs.Contracts;
using ScriptCs.Exceptions;

namespace ScriptCs.Engine.Roslyn
{
    using System.Runtime.ExceptionServices;

    public class RoslynScriptEngine : IScriptEngine
    {
        protected readonly ScriptEngine ScriptEngine;
        private IScriptHost _host;

        public const string SessionKey = "Session";

        public RoslynScriptEngine(ILog logger)
        {
            ScriptEngine = new ScriptEngine();
            ScriptEngine.AddReference(typeof(ScriptExecutor).Assembly);
            Logger = logger;
        }

        protected ILog Logger { get; private set; }
        
        public string BaseDirectory
        {
            get { return ScriptEngine.BaseDirectory;  }
            set { ScriptEngine.BaseDirectory = value; }
        }

        public string CacheDirectory { get; set; }

        public string FileName { get; set; }

        public void Initialize(IScriptHost host)
        {
            _host = host;
        }

        public ScriptResult Execute(string code, string[] scriptArgs, AssemblyReferences references, IEnumerable<string> namespaces, ScriptPackSession scriptPackSession)
        {
            Guard.AgainstNullArgument("scriptPackSession", scriptPackSession);
            Guard.AgainstNullArgument("references", references);

            Logger.Debug("Starting to create execution components");
            Logger.Debug("Creating script host");
            
            references.PathReferences.UnionWith(scriptPackSession.References);
            SessionState<Session> sessionState;

            if (!scriptPackSession.State.ContainsKey(SessionKey))
            {
                if (_host == null)
                    throw new ScriptSetupException("Please call Initialize first");

                Logger.Debug("Creating session");

                var hostType = _host.GetType();
                ScriptEngine.AddReference(hostType.Assembly);
                var session = ScriptEngine.CreateSession(_host, hostType);

                foreach (var reference in references.PathReferences)
                {
                    Logger.DebugFormat("Adding reference to {0}", reference);
                    session.AddReference(reference);
                }

                foreach (var assembly in references.Assemblies)
                {
                    Logger.DebugFormat("Adding reference to {0}", assembly.FullName);
                    session.AddReference(assembly);
                }

                foreach (var @namespace in namespaces.Union(scriptPackSession.Namespaces).Distinct())
                {
                    Logger.DebugFormat("Importing namespace {0}", @namespace);
                    session.ImportNamespace(@namespace);
                }

                sessionState = new SessionState<Session> { References = references, Session = session };
                scriptPackSession.State[SessionKey] = sessionState;
            }
            else
            {
                Logger.Debug("Reusing existing session");
                sessionState = (SessionState<Session>)scriptPackSession.State[SessionKey];

                var newReferences = sessionState.References == null ? references : references.Except(sessionState.References);
                foreach (var reference in newReferences.PathReferences)
                {
                    Logger.DebugFormat("Adding reference to {0}", reference);
                    sessionState.Session.AddReference(reference);
                }

                foreach (var assembly in newReferences.Assemblies)
                {
                    Logger.DebugFormat("Adding reference to {0}", assembly.FullName);
                    sessionState.Session.AddReference(assembly);
                }

                sessionState.References = newReferences;
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