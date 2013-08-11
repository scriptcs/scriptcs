using System;
using System.Linq;
using System.Reflection;
using System.Xml.Schema;

using Common.Logging;
using Roslyn.Scripting;
using Roslyn.Scripting.CSharp;

namespace ScriptCs.Engine.Roslyn
{
    using System.Runtime.ExceptionServices;

    public class RoslynScriptEngine : IScriptEngine
    {
        private readonly ScriptEngine _scriptEngine;

        private readonly IScriptHostFactory _scriptHostFactory;

        private readonly ILog _logger;

        public const string SessionKey = "Session";

        public RoslynScriptEngine(IScriptHostFactory scriptHostFactory, ILog logger)
        {
            _scriptEngine = new ScriptEngine();
            _scriptEngine.AddReference(typeof(ScriptExecutor).Assembly);
            _scriptHostFactory = scriptHostFactory;
            _logger = logger;
        }
        
        public string BaseDirectory
        {
            get {  return _scriptEngine.BaseDirectory;  }
            set {  _scriptEngine.BaseDirectory = value; }
        }

        public ScriptResult Execute(ScriptEnvironment environment, ScriptPackSession scriptPackSession)
        {
            Guard.AgainstNullArgument("scriptPackSession", scriptPackSession);

            _logger.Debug("Starting to create execution components");
            _logger.Debug("Creating script host");
            
            var distinctReferences = environment.References.Union(scriptPackSession.References).Distinct().ToList();
            var distinctNamespaces = environment.Namespaces.Union(scriptPackSession.Namespaces).Distinct().ToList();

            SessionState<Session> sessionState;
            if (!scriptPackSession.State.ContainsKey(SessionKey))
            {
                var host = _scriptHostFactory.CreateScriptHost(new ScriptPackManager(scriptPackSession.Contexts), environment);
                _logger.Debug("Creating session");
                var session = _scriptEngine.CreateSession(host);

                foreach (var reference in distinctReferences)
                {
                    _logger.DebugFormat("Adding reference to {0}", reference);
                    session.AddReference(reference);
                }

                foreach (var @namespace in distinctNamespaces)
                {
                    _logger.DebugFormat("Importing namespace {0}", @namespace);
                    session.ImportNamespace(@namespace);
                }

                sessionState = new SessionState<Session>
                {
                    References = distinctReferences, 
                    Session = session,
                    Environment = environment
                };

                scriptPackSession.State[SessionKey] = sessionState;
            }
            else
            {
                _logger.Debug("Reusing existing session");
                sessionState = (SessionState<Session>) scriptPackSession.State[SessionKey];

                var newReferences = sessionState.References == null || !sessionState.References.Any() ? distinctReferences : distinctReferences.Except(sessionState.References);
                if (newReferences.Any())
                {
                    foreach (var reference in newReferences)
                    {
                        _logger.DebugFormat("Adding reference to {0}", reference);
                        sessionState.Session.AddReference(reference);
                    }
                    sessionState.References = newReferences;
                }
            }

            _logger.Debug("Starting execution");

            Assembly currentAssembly;
            var result = Execute(environment.Script, sessionState.Session, out currentAssembly);

            var sessionEnvironment = sessionState.Environment;
            
            if (currentAssembly != null)
                sessionEnvironment.Assembly = currentAssembly;

            if (result.ExecuteExceptionInfo != null)
                sessionEnvironment.LastException = result.ExecuteExceptionInfo.SourceException;

            if (result.CompileExceptionInfo != null)
                sessionEnvironment.LastException = result.CompileExceptionInfo.SourceException;

            _logger.Debug("Finished execution");
            return result;
        }

        protected virtual ScriptResult Execute(string code, Session session, out Assembly currentAssembly)
        {
            Guard.AgainstNullArgument("session", session);

            try
            {
                var submission = CompileSubmission(code, session, out currentAssembly);
                return ExecuteSubmission(submission);
            }
            catch (Exception ex)
            {
                var result = new ScriptResult();

                result.UpdateClosingExpectation(ex);
                if (!result.IsPendingClosingChar)
                {
                    result.CompileExceptionInfo = ExceptionDispatchInfo.Capture(ex);
                }

                currentAssembly = null;
                return result;
            }
        }

        private static ScriptResult ExecuteSubmission(Submission<object> submission)
        {
            var result = new ScriptResult();

            try
            {
                result.ReturnValue = submission.Execute();
            }
            catch (Exception ex)
            {
                result.ExecuteExceptionInfo = ExceptionDispatchInfo.Capture(ex);
            }

            return result;
        }

        protected static Submission<object> CompileSubmission(string code, Session session, out Assembly currentAssembly)
        {
            var oldAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            var submission = session.CompileSubmission<object>(code);

            var newAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            currentAssembly = newAssemblies.Except(oldAssemblies).SingleOrDefault();

            return submission;
        }
    }
}