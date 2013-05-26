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

        public string FileName { get; set; }

        public ScriptResult Execute(string code, string[] scriptArgs, IEnumerable<string> references, IEnumerable<string> namespaces, ScriptPackSession scriptPackSession)
        {
            Guard.AgainstNullArgument("scriptPackSession", scriptPackSession);

            _logger.Debug("Starting to create execution components");
            _logger.Debug("Creating script host");
            
            var distinctReferences = references.Union(scriptPackSession.References).Distinct().ToList();
            SessionState<Session> sessionState;

            if (!scriptPackSession.State.ContainsKey(SessionKey))
            {
                var host = _scriptHostFactory.CreateScriptHost(new ScriptPackManager(scriptPackSession.Contexts), scriptArgs);
                _logger.Debug("Creating session");
                var session = _scriptEngine.CreateSession(host);

                foreach (var reference in distinctReferences)
                {
                    _logger.DebugFormat("Adding reference to {0}", reference);
                    session.AddReference(reference);
                }

                foreach (var @namespace in namespaces.Union(scriptPackSession.Namespaces).Distinct())
                {
                    _logger.DebugFormat("Importing namespace {0}", @namespace);
                    session.ImportNamespace(@namespace);
                }

                sessionState = new SessionState<Session> {References = distinctReferences, Session = session};
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
            var result = Execute(code, sessionState.Session);
            _logger.Debug("Finished execution");
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
                    result.CompileExceptionInfo = ExceptionDispatchInfo.Capture(ex);
            }
            return result;
        }
    }
}