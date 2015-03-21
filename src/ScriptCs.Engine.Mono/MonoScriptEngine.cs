﻿extern alias MonoCSharp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;
using Mono.Collections.Generic;
using MonoCSharp::Mono.CSharp;
using ScriptCs.Contracts;
using ScriptCs.Engine.Mono.Segmenter;

namespace ScriptCs.Engine.Mono
{
    public class MonoScriptEngine : IReplEngine
    {
        public const string SessionKey = "MonoSession";

        private readonly IScriptHostFactory _scriptHostFactory;

        public string BaseDirectory { get; set; }
        public string CacheDirectory { get; set; }
        public string FileName { get; set; }

        public MonoScriptEngine(IScriptHostFactory scriptHostFactory, ILog logger)
        {
            _scriptHostFactory = scriptHostFactory;
            Logger = logger;
        }

        public ILog Logger { get; set; }

        public ICollection<string> GetLocalVariables(ScriptPackSession scriptPackSession)
        {
            if (scriptPackSession != null && scriptPackSession.State.ContainsKey(SessionKey))
            {
                var sessionState = (SessionState<Evaluator>)scriptPackSession.State[SessionKey];
                var vars = sessionState.Session.GetVars();
                if (!string.IsNullOrWhiteSpace(vars) && vars.Contains(Environment.NewLine))
                {
                    return vars.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                }
            }

            return new Collection<string>();
        }

#pragma warning disable 618
        public ScriptResult Execute(
            string code,
            string[] scriptArgs,
            AssemblyReferences references,
            IEnumerable<string> namespaces,
            ScriptPackSession scriptPackSession)
#pragma warning restore 618
        {
            Guard.AgainstNullArgument("references", references);
            Guard.AgainstNullArgument("scriptPackSession", scriptPackSession);

            var sensibleReferences = new References(references.Assemblies, references.PathReferences)
                .Union(scriptPackSession.References);

            SessionState<Evaluator> sessionState;
            var isFirstExecution = !scriptPackSession.State.ContainsKey(SessionKey);

            if (isFirstExecution)
            {
                code = code.DefineTrace();
                Logger.Debug("Creating session");
                var context = new CompilerContext(
                    new CompilerSettings { AssemblyReferences = sensibleReferences.Paths.ToList() },
                    new ConsoleReportPrinter());

                var evaluator = new Evaluator(context);
                var allNamespaces = namespaces.Union(scriptPackSession.Namespaces).Distinct();

                var host = _scriptHostFactory.CreateScriptHost(
                    new ScriptPackManager(scriptPackSession.Contexts), scriptArgs);

                MonoHost.SetHost((ScriptHost)host);
                ScriptLibraryWrapper.SetHost(host);

                evaluator.ReferenceAssembly(typeof(MonoHost).Assembly);
                evaluator.InteractiveBaseClass = typeof(MonoHost);

                sessionState = new SessionState<Evaluator>
                {
                    References = references,
                    Namespaces = new HashSet<string>(),
                    Session = evaluator,
                };

                ImportNamespaces(allNamespaces, sessionState);
                scriptPackSession.State[SessionKey] = sessionState;
            }
            else
            {
                Logger.Debug("Reusing existing session");
                sessionState = (SessionState<Evaluator>)scriptPackSession.State[SessionKey];

                var newReferences = sessionState.References == null
                    ? sensibleReferences
                    : sensibleReferences
                        .Except(sessionState.References.Assemblies)
                        .Except(sessionState.References.PathReferences);

                foreach (var reference in newReferences.Paths)
                {
                    Logger.DebugFormat("Adding reference to {0}", reference);
                    sessionState.Session.LoadAssembly(reference);
                }

                sessionState.References = references;

                var newNamespaces = sessionState.Namespaces == null
                    ? namespaces
                    : namespaces.Except(sessionState.Namespaces);

                ImportNamespaces(newNamespaces, sessionState);
            }

            Logger.Debug("Starting execution");
            var result = Execute(code, sessionState.Session);
            Logger.Debug("Finished execution");

            return result;
        }

        protected virtual ScriptResult Execute(string code, Evaluator session)
        {
            Guard.AgainstNullArgument("session", session);

            try
            {
                object scriptResult = null;
                var segmenter = new ScriptSegmenter();
                foreach (var segment in segmenter.Segment(code))
                {
                    bool resultSet;
                    session.Evaluate(segment.Code, out scriptResult, out resultSet);
                }

                return new ScriptResult(returnValue: scriptResult);
            }
            catch (AggregateException ex)
            {
                return new ScriptResult(executionException: ex.InnerException);
            }
            catch (Exception ex)
            {
                return new ScriptResult(executionException: ex);
            }
        }

        private void ImportNamespaces(IEnumerable<string> namespaces, SessionState<Evaluator> sessionState)
        {
            var builder = new StringBuilder();
            foreach (var ns in namespaces)
            {
                Logger.DebugFormat(ns);
                builder.AppendLine(string.Format("using {0};", ns));
                sessionState.Namespaces.Add(ns);
            }

            sessionState.Session.Compile(builder.ToString());
        }
    }
}
