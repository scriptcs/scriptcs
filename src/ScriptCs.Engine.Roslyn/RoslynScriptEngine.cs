using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.Logging;
using Roslyn.Scripting;
using Roslyn.Scripting.CSharp;

namespace ScriptCs.Engine.Roslyn
{
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
            get { return _scriptEngine.BaseDirectory; }
            set { _scriptEngine.BaseDirectory = value; }
        }

        public ScriptExecutionResult Execute(string code, IEnumerable<string> references, IEnumerable<string> namespaces,
                                             ScriptPackSession scriptPackSession)
        {
            Guard.AgainstNullArgument("scriptPackSession", scriptPackSession);

            _logger.Info("Starting to create execution components");
            _logger.Debug("Creating script host");
            var distinctReferences = references.Union(scriptPackSession.References).Distinct().ToList();
            SessionState<Session> sessionState;

            if (!scriptPackSession.State.ContainsKey(SessionKey))
            {
                var host = _scriptHostFactory.CreateScriptHost(new ScriptPackManager(scriptPackSession.Contexts));
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

                var newReferences = sessionState.References == null || !sessionState.References.Any()
                                        ? distinctReferences
                                        : distinctReferences.Except(sessionState.References);
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

            _logger.Info("Starting execution");
            var result = Execute(code, sessionState.Session);
            _logger.Info("Finished execution");
            return result;
        }

        protected virtual ScriptExecutionResult Execute(string code, Session session)
        {
            Guard.AgainstNullArgument("session", session);

            var result = new ScriptExecutionResult();
            try
            {
                var submission = session.CompileSubmission<object>(code);
                try
                {
                    result.Result = submission.Execute();
                }
                catch (Exception runtimeException)
                {
                    result.RuntimeException = runtimeException;
                }
            }
            catch (Exception compilationException)
            {
                result.UpdateClosingExpectation(compilationException);
                if (!result.ScriptIsMissingClosingChar.HasValue)
                    result.CompilationException = compilationException;
            }
            return result;
        }
    }
}