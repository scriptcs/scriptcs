using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;
using Mono.CSharp;
using Newtonsoft.Json.Linq;
using ScriptCs.Contracts;
using ScriptCs.SyntaxTreeParser;

namespace ScriptCs.Engine.Mono
{
    public class MonoScriptEngine : IScriptEngine
    {
        private IScriptHostFactory _scriptHostFactory;
        private readonly IFileSystem _fs;
        public string BaseDirectory { get; set; }
        public string CacheDirectory { get; set; }
        public string FileName { get; set; }

        public const string SessionKey = "MonoSession";

        public MonoScriptEngine(IScriptHostFactory scriptHostFactory, ILog logger, IFileSystem fs)
        {
            _scriptHostFactory = scriptHostFactory;
            _fs = fs;
            Logger = logger;
        }

        public ILog Logger { get; set; }

        public ScriptResult Execute(string code, string[] scriptArgs, AssemblyReferences references, IEnumerable<string> namespaces,
            ScriptPackSession scriptPackSession)
        {
            references.PathReferences.UnionWith(scriptPackSession.References);

            SessionState<Evaluator> sessionState;
            if (!scriptPackSession.State.ContainsKey(SessionKey))
            {
                Logger.Debug("Creating session");
                var context = new CompilerContext(new CompilerSettings
                {
                    AssemblyReferences = references.PathReferences.ToList()
                }, new ConsoleReportPrinter());

                var evaluator = new Evaluator(context);
                var builder = new StringBuilder();

                foreach (var ns in namespaces.Union(scriptPackSession.Namespaces).Distinct())
                {
                    builder.AppendLine(string.Format("using {0};", ns));
                }

                evaluator.Compile(builder.ToString());

                var parser = new SyntaxParser();
                var parseResult = parser.Parse(code);

                var host = _scriptHostFactory.CreateScriptHost(new ScriptPackManager(scriptPackSession.Contexts), scriptArgs);
                MonoHost.SetHost((ScriptHost)host);

                evaluator.ReferenceAssembly(typeof(MonoHost).Assembly);
                evaluator.InteractiveBaseClass = typeof(MonoHost);

                //if (code.Contains("//exec"))
                //{
                //evaluator.Compile(parseResult.Declarations);
                //code = parseResult.Evaluations;
                //  evaluator.Compile(code.Split(new[] { "//exec" }, StringSplitOptions.RemoveEmptyEntries)[0]);
                //code = code.Split(new[] { "//exec" }, StringSplitOptions.RemoveEmptyEntries)[1];
                //}

                if (parseResult.Declarations != null)
                {
                    evaluator.Compile(parseResult.Declarations);
                    code = null;
                }

                if (parseResult.Evaluations != null)
                {
                    code = parseResult.Evaluations;
                }

                sessionState = new SessionState<Evaluator>
                {
                    References = references,
                    Session = evaluator
                };
                scriptPackSession.State[SessionKey] = sessionState;
            }
            else
            {
                Logger.Debug("Reusing existing session");
                sessionState = (SessionState<Evaluator>)scriptPackSession.State[SessionKey];

                var newReferences = sessionState.References == null ? references : references.Except(sessionState.References);
                foreach (var reference in newReferences.PathReferences)
                {
                    Logger.DebugFormat("Adding reference to {0}", reference);
                    sessionState.Session.LoadAssembly(reference);
                }

                sessionState.References = newReferences;

                var parser = new SyntaxParser();
                var parseResult = parser.Parse(code);

                if (parseResult.Declarations != null)
                {
                    var compiledMethod = sessionState.Session.Compile(parseResult.Declarations);
                    return new ScriptResult();
                    //code = parseResult.Declarations;
                }
                //var newUsings = sessionState.References == null || !sessionState.References.Any() ? distinctReferences : distinctReferences.Except(sessionState.References);
            }

            Logger.Debug("Starting execution");

            try
            {
                if (code != null)
                {
                    object scriptResult;
                    bool resultSet;
                    var result = sessionState.Session.Evaluate(code, out scriptResult, out resultSet);

                    Logger.Debug("Finished execution");
                    return new ScriptResult { ReturnValue = scriptResult };
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }

            return new ScriptResult();
        }
    }
}
