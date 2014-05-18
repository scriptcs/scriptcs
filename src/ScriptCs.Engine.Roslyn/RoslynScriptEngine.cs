using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.Logging;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using Roslyn.Scripting;
using Roslyn.Scripting.CSharp;

using ScriptCs.Contracts;

namespace ScriptCs.Engine.Roslyn
{
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

        public string CacheDirectory { get; set; }

        public string FileName { get; set; }

        public ScriptResult Execute(string code, string[] scriptArgs, AssemblyReferences references, IEnumerable<string> namespaces, ScriptPackSession scriptPackSession)
        {
            Guard.AgainstNullArgument("scriptPackSession", scriptPackSession);
            Guard.AgainstNullArgument("references", references);

            Logger.Debug("Starting to create execution components");
            Logger.Debug("Creating script host");

            var executionReferences = new AssemblyReferences()
            {
                Assemblies = new HashSet<Assembly>(references.Assemblies),
                PathReferences = new HashSet<string>(references.PathReferences)
            };
            executionReferences.PathReferences.UnionWith(scriptPackSession.References);

            SessionState<Session> sessionState;

            if (!scriptPackSession.State.ContainsKey(SessionKey))
            {
                var host = _scriptHostFactory.CreateScriptHost(new ScriptPackManager(scriptPackSession.Contexts), scriptArgs);
                Logger.Debug("Creating session");

                var hostType = host.GetType();
                ScriptEngine.AddReference(hostType.Assembly);
                var session = ScriptEngine.CreateSession(host, hostType);
                var allNamespaces = namespaces.Union(scriptPackSession.Namespaces).Distinct();

                foreach (var reference in executionReferences.PathReferences)
                {
                    Logger.DebugFormat("Adding reference to {0}", reference);
                    session.AddReference(reference);
                }

                foreach (var assembly in executionReferences.Assemblies)
                {
                    Logger.DebugFormat("Adding reference to {0}", assembly.FullName);
                    session.AddReference(assembly);
                }

                foreach (var @namespace in allNamespaces)
                {
                    Logger.DebugFormat("Importing namespace {0}", @namespace);
                    session.ImportNamespace(@namespace);
                }

                sessionState = new SessionState<Session> { References = executionReferences, Session = session, Namespaces = new HashSet<string>(allNamespaces) };
                scriptPackSession.State[SessionKey] = sessionState;
            }
            else
            {
                Logger.Debug("Reusing existing session");
                sessionState = (SessionState<Session>)scriptPackSession.State[SessionKey];

                if (sessionState.References == null)
                {
                    sessionState.References = new AssemblyReferences();
                }

                if (sessionState.Namespaces == null)
                {
                    sessionState.Namespaces = new HashSet<string>();
                }

                var newReferences = executionReferences.Except(sessionState.References);
                
                foreach (var reference in newReferences.PathReferences)
                {
                    Logger.DebugFormat("Adding reference to {0}", reference);
                    sessionState.Session.AddReference(reference);
                    sessionState.References.PathReferences.Add(reference);
                }

                foreach (var assembly in newReferences.Assemblies)
                {
                    Logger.DebugFormat("Adding reference to {0}", assembly.FullName);
                    sessionState.Session.AddReference(assembly);
                    sessionState.References.Assemblies.Add(assembly);
                }

                var newNamespaces = namespaces.Except(sessionState.Namespaces);

                foreach (var @namespace in newNamespaces)
                {
                    Logger.DebugFormat("Importing namespace {0}", @namespace);
                    sessionState.Session.ImportNamespace(@namespace);
                    sessionState.Namespaces.Add(@namespace);
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

            if (!IsCompleteSubmission(code))
            {
                return ScriptResult.IncompleteSubmission;
            }

            try
            {
                var submission = session.CompileSubmission<object>(code);

                try
                {
                    return ScriptResult.FromReturnValue(submission.Execute());
                }
                catch (Exception ex)
                {
                    return ScriptResult.FromExecutionException(ex);
                }
            }
            catch (Exception ex)
            {
                return ScriptResult.FromCompilationException(ex);
            }
        }

        private static bool IsCompleteSubmission(string code)
        {
            var options = new ParseOptions(kind: SourceCodeKind.Interactive);

            var syntaxTree = SyntaxTree.ParseText(code, options: options);

            return Syntax.IsCompleteSubmission(syntaxTree);
        }
    }
}