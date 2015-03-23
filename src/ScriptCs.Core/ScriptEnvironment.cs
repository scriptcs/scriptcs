using System.Collections.Generic;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ScriptEnvironment : IScriptEnvironment
    {
        public ScriptEnvironment(string[] scriptArgs)
        {
            ScriptArgs = scriptArgs;
        }

        public IReadOnlyList<string> ScriptArgs { get; private set; }
    }
}