extern alias MonoCSharp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private readonly ILog _log;

        public string BaseDirectory { get; set; }
        public string CacheDirectory { get; set; }
        public string FileName { get; set; }

        [Obsolete("Support for Common.Logging types was deprecated in version 0.15.0 and will soon be removed.")]
        public MonoScriptEngine(IScriptHostFactory scriptHostFactory, Common.Logging.ILog logger)
            : this(scriptHostFactory, new CommonLoggingLogProvider(logger))
        {
        }

        public MonoScriptEngine(IScriptHostFactory scriptHostFactory, ILogProvider logProvider)
        {
            Guard.AgainstNullArgument("logProvider", logProvider);

            _scriptHostFactory = scriptHostFactory;
            _log = logProvider.ForCurrentType();
#pragma warning disable 618
            Logger = new ScriptCsLogger(_log);
#pragma warning restore 618
        }

        [Obsolete("Support for Common.Logging types was deprecated in version 0.15.0 and will soon be removed.")]
        public Common.Logging.ILog Logger { get; set; }

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

        public ScriptResult Execute(
            string code,
            string[] scriptArgs,
            AssemblyReferences references,
            IEnumerable<string> namespaces,
            ScriptPackSession scriptPackSession)
        {
            Guard.AgainstNullArgument("references", references);
            Guard.AgainstNullArgument("scriptPackSession", scriptPackSession);

            references = references.Union(scriptPackSession.References);

            SessionState<Evaluator> sessionState;
            var isFirstExecution = !scriptPackSession.State.ContainsKey(SessionKey);

            if (isFirstExecution)
            {
                code = code.DefineTrace();
                _log.Debug("Creating session");
                var context = new CompilerContext(
                    new CompilerSettings { AssemblyReferences = references.Paths.ToList() },
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
                _log.Debug("Reusing existing session");
                sessionState = (SessionState<Evaluator>)scriptPackSession.State[SessionKey];

                var newReferences = sessionState.References == null
                    ? references
                    : references.Except(sessionState.References);

                foreach (var reference in newReferences.Paths)
                {
                    _log.DebugFormat("Adding reference to {0}", reference);
                    sessionState.Session.LoadAssembly(reference);
                }

                sessionState.References = references;

                var newNamespaces = sessionState.Namespaces == null
                    ? namespaces
                    : namespaces.Except(sessionState.Namespaces);

                ImportNamespaces(newNamespaces, sessionState);
            }

            _log.Debug("Starting execution");
            var result = Execute(code, sessionState.Session);
            _log.Debug("Finished execution");

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
                _log.DebugFormat(ns);
                builder.AppendLine(string.Format("using {0};", ns));
                sessionState.Namespaces.Add(ns);
            }

            sessionState.Session.Compile(builder.ToString());
        }
    }
}
