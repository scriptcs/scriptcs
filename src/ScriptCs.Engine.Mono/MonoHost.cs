extern alias MonoCSharp;

using ScriptCs.Contracts;

namespace ScriptCs.Engine.Mono
{
    public class MonoHost : IScriptHost
    {
        private static ScriptHost _scriptHost;
        private static IRepl _repl;

        public static void SetHost(ScriptHost scriptHost)
        {
            _scriptHost = scriptHost;
        }

        public static void SetRepl(IRepl repl)
        {
            _repl = repl;
        }

        public static IScriptEnvironment Env { get { return _scriptHost.Env; } }

        IScriptEnvironment IScriptHost.Env
        {
            get { return _scriptHost.Env; }
        }

        public static IRepl Repl { get { return _repl; }}

        IRepl IScriptHost.Repl
        {
            get { return _scriptHost.Repl; }
        }

        public static T Require<T>() where T : IScriptPackContext
        {
            return _scriptHost.Require<T>();
        }

        T IScriptHost.Require<T>()
        {
            return _scriptHost.Require<T>();
        }
    }
}