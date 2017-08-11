using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using ScriptCs.Contracts;

namespace ScriptCs.Engine.Roslyn
{
    // note this class is a base for future VB engine
    public abstract class CommonScriptEngine : IScriptEngine
    {
        protected ScriptOptions ScriptOptions { get; set; }

        protected ScriptMetadataResolver ScriptMetadataResolver { get; private set; }

        private readonly IScriptHostFactory _scriptHostFactory;
        protected ILog Log;

        public const string SessionKey = "Session";

        protected CommonScriptEngine(IScriptHostFactory scriptHostFactory, ILogProvider logProvider)
        {
            Guard.AgainstNullArgument("logProvider", logProvider);
            ScriptMetadataResolver = ScriptMetadataResolver.Default;
            ScriptOptions = ScriptOptions.Default.
                WithReferences(typeof(object).Assembly, typeof(TupleElementNamesAttribute).Assembly). // System.ValueTuple
                WithMetadataResolver(ScriptMetadataResolver);
            _scriptHostFactory = scriptHostFactory;
            Log = logProvider.ForCurrentType();
            SetCSharpVersionToLatest();
        }

        public string BaseDirectory
        {
            get { return ScriptMetadataResolver.BaseDirectory; }
            set { ScriptMetadataResolver = ScriptMetadataResolver.WithBaseDirectory(value); }
        }

        public string CacheDirectory { get; set; }

        public string FileName { get; set; }

        public string ScriptPath { get; set; }

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

            Log.Debug("Starting to create execution components");
            Log.Debug("Creating script host");

            var executionReferences = new AssemblyReferences(references.Assemblies, references.Paths);
            executionReferences.Union(scriptPackSession.References);

            ScriptResult scriptResult;
            SessionState<ScriptState> sessionState;

            var isFirstExecution = !scriptPackSession.State.ContainsKey(SessionKey);

            if (isFirstExecution)
            {
                var host = _scriptHostFactory.CreateScriptHost(
                    new ScriptPackManager(scriptPackSession.Contexts), scriptArgs);

                ScriptLibraryWrapper.SetHost(host);
                Log.Debug("Creating session");

                var hostType = host.GetType();

                ScriptOptions = ScriptOptions.AddReferences(hostType.Assembly);
                
                var allNamespaces = namespaces.Union(scriptPackSession.Namespaces).Distinct();

                foreach (var reference in executionReferences.Paths)
                {
                    Log.DebugFormat("Adding reference to {0}", reference);
                    ScriptOptions = ScriptOptions.AddReferences(reference);
                }

                foreach (var assembly in executionReferences.Assemblies)
                {
                    Log.DebugFormat("Adding reference to {0}", assembly.FullName);
                    ScriptOptions = ScriptOptions.AddReferences(assembly);
                }

                foreach (var @namespace in allNamespaces)
                {
                    Log.DebugFormat("Importing namespace {0}", @namespace);
                    ScriptOptions = ScriptOptions.AddImports(@namespace);
                }

                sessionState = new SessionState<ScriptState> { References = executionReferences, Namespaces = new HashSet<string>(allNamespaces) };
                scriptPackSession.State[SessionKey] = sessionState;

                scriptResult = Execute(code, host, sessionState);
            }
            else
            {
                Log.Debug("Reusing existing session");
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
                    Log.DebugFormat("Adding reference to {0}", reference);
                    ScriptOptions = ScriptOptions.AddReferences(reference);
                    sessionState.References = sessionState.References.Union(new[] { reference });
                }

                foreach (var assembly in newReferences.Assemblies)
                {
                    Log.DebugFormat("Adding reference to {0}", assembly.FullName);
                    ScriptOptions = ScriptOptions.AddReferences(assembly);
                    sessionState.References = sessionState.References.Union(new[] { assembly });
                }

                var newNamespaces = namespaces.Except(sessionState.Namespaces);

                foreach (var @namespace in newNamespaces)
                {
                    Log.DebugFormat("Importing namespace {0}", @namespace);
                    ScriptOptions = ScriptOptions.AddImports(@namespace);
                    sessionState.Namespaces.Add(@namespace);
                }

                if (string.IsNullOrWhiteSpace(code))
                {
                    return ScriptResult.Empty;
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
                Log.Debug("Starting execution");
                var result = GetScriptState(code, globals);
                Log.Debug("Finished execution");
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

        protected abstract ScriptState GetScriptState(string code, object globals);

        private void SetCSharpVersionToLatest()
        {
            try
            {
                // reset default scripting mode to latest language version to enable C# 7.1 features
                // this is not needed once https://github.com/dotnet/roslyn/pull/21331 ships
                var csharpScriptCompilerType = typeof(CSharpScript).GetTypeInfo().Assembly.GetType("Microsoft.CodeAnalysis.CSharp.Scripting.CSharpScriptCompiler");
                var parseOptionsField = csharpScriptCompilerType?.GetField("s_defaultOptions", BindingFlags.Static | BindingFlags.NonPublic);
                parseOptionsField?.SetValue(null, new CSharpParseOptions(LanguageVersion.Latest, kind: SourceCodeKind.Script));
            }
            catch (Exception)
            {
                Log.Warn("Unable to set C# language version to latest, will use the default version.");
            }
        }
    }
}