using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.CSharp;
using ScriptCs.Contracts;

namespace ScriptCs.CSharp
{
    public class CSharpScriptEngine : IScriptEngine
    {
        private ScriptOptions _scriptOptions;
        private readonly IScriptHostFactory _scriptHostFactory;

        public const string SessionKey = "Session";
        private const string InvalidNamespaceError = "error CS0246";

        public CSharpScriptEngine(IScriptHostFactory scriptHostFactory, ILog logger)
        {
            _scriptOptions = new ScriptOptions().WithReferences(typeof(ScriptExecutor).Assembly, typeof(Object).Assembly);
            _scriptHostFactory = scriptHostFactory;
            Logger = logger;
        }

        protected ILog Logger { get; private set; }

        public string BaseDirectory
        {
            get { return _scriptOptions.BaseDirectory; }
            set { _scriptOptions = _scriptOptions.WithBaseDirectory(value); }
        }

        public string CacheDirectory { get; set; }

        public string FileName { get; set; }

        public ScriptResult Execute(string code, string[] scriptArgs, AssemblyReferences references, IEnumerable<string> namespaces, ScriptPackSession scriptPackSession)
        {
            if (scriptPackSession == null)
            {
                throw new ArgumentNullException("scriptPackSession");
            }

            if (references == null)
            {
                throw new ArgumentNullException("references");
            }

            Logger.Debug("Starting to create execution components");
            Logger.Debug("Creating script host");

            var executionReferences = new AssemblyReferences(references.Assemblies, references.Paths);
            executionReferences.Union(scriptPackSession.References);

            ScriptResult scriptResult;
            SessionState<ScriptState> sessionState;

            var isFirstExecution = !scriptPackSession.State.ContainsKey(SessionKey);

            var host = _scriptHostFactory.CreateScriptHost(new ScriptPackManager(scriptPackSession.Contexts), scriptArgs);
            Logger.Debug("Creating session");

            var hostType = host.GetType();

            if (isFirstExecution)
            {
                code = code.DefineTrace();
                _scriptOptions = _scriptOptions.AddReferences(hostType.Assembly);
                
                var allNamespaces = namespaces.Union(scriptPackSession.Namespaces).Distinct();

                foreach (var reference in executionReferences.Paths)
                {
                    Logger.DebugFormat("Adding reference to {0}", reference);
                    _scriptOptions = _scriptOptions.AddReferences(reference);
                }

                foreach (var assembly in executionReferences.Assemblies)
                {
                    Logger.DebugFormat("Adding reference to {0}", assembly.FullName);
                    _scriptOptions = _scriptOptions.AddReferences(assembly);
                }

                foreach (var @namespace in allNamespaces)
                {
                    Logger.DebugFormat("Importing namespace {0}", @namespace);
                    _scriptOptions = _scriptOptions.AddNamespaces(@namespace);
                }

                sessionState = new SessionState<ScriptState> { References = executionReferences, Namespaces = new HashSet<string>(allNamespaces) };
                scriptPackSession.State[SessionKey] = sessionState;

                //result = CSharpScript.Run(code, _scriptOptions, host);
                scriptResult = Execute(code, host, sessionState);
            }
            else
            {
                Logger.Debug("Reusing existing session");
                sessionState = (SessionState<ScriptState>)scriptPackSession.State[SessionKey];

                if (sessionState.References == null)
                {
                    sessionState.References = new AssemblyReferences();
                }

                if (sessionState.Namespaces == null)
                {
                    sessionState.Namespaces = new HashSet<string>();
                }

                var newReferences = executionReferences.Except(sessionState.References);

                foreach (var reference in newReferences.Paths)
                {
                    Logger.DebugFormat("Adding reference to {0}", reference);
                    _scriptOptions.AddReferences(reference);
                    sessionState.References = sessionState.References.Union(new[] { reference });
                }

                foreach (var assembly in newReferences.Assemblies)
                {
                    Logger.DebugFormat("Adding reference to {0}", assembly.FullName);
                    _scriptOptions.AddReferences(assembly);
                    sessionState.References = sessionState.References.Union(new[] { assembly });
                }

                var newNamespaces = namespaces.Except(sessionState.Namespaces);

                foreach (var @namespace in newNamespaces)
                {
                    Logger.DebugFormat("Importing namespace {0}", @namespace);
                    _scriptOptions.AddNamespaces(@namespace);
                    sessionState.Namespaces.Add(@namespace);
                }


                scriptResult = Execute(code, sessionState.Session, sessionState);
            }

            return scriptResult;

            //todo handle namespace failures
            //https://github.com/dotnet/roslyn/issues/1012
        }

        protected virtual ScriptResult Execute(string code, object globals, SessionState<ScriptState> sessionState)
        {
            try
            {
                Logger.Debug("Starting execution");
                var result = CSharpScript.Run(code, _scriptOptions, globals);
                Logger.Debug("Finished execution");
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

        protected static bool IsCompleteSubmission(string code)
        {
            //invalid REPL command
            if (code.StartsWith(":"))
            {
                return true;
            }

            var options = new CSharpParseOptions(LanguageVersion.CSharp6, DocumentationMode.Parse,
                SourceCodeKind.Interactive, null);

            var syntaxTree = SyntaxFactory.ParseSyntaxTree(code, options: options);
            return SyntaxFactory.IsCompleteSubmission(syntaxTree);
        }
    }
}