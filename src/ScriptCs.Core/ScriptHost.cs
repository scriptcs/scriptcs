using System;
using System.Collections.Generic;
using System.Reflection;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ScriptHost : IScriptHost
    {
        private readonly IScriptPackManager _scriptPackManager;

        public ScriptHost(IScriptPackManager scriptPackManager, ScriptEnvironment environment)
        {
            _scriptPackManager = scriptPackManager;
            Env = environment;
        }

        public ScriptEnvironment Env { get; private set; }

        public T Require<T>() where T : IScriptPackContext
        {
            return _scriptPackManager.Get<T>();
        }

        public void GetVars()
        {
            var fields = this.GetType().GetFields(BindingFlags.Instance);

            foreach (var field in fields)
            {
                Console.WriteLine(field.Name + " - " + field.GetValue(this));
            }
        }
    }
}