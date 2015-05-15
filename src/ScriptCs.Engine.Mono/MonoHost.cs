extern alias MonoCSharp;

using ScriptCs.Contracts;

namespace ScriptCs.Engine.Mono
{
    public class MonoHost : IScriptHost
    {
        private static ScriptHost _scriptHost;

        public static void SetHost(ScriptHost scriptHost)
        {
            _scriptHost = scriptHost;
        }

        public static IScriptEnvironment Env { get { return _scriptHost.Env; } }

        IScriptEnvironment IScriptHost.Env
        {
            get { return _scriptHost.Env; }
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