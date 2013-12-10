using System.Collections.Generic;

namespace ScriptCs
{
    public class ScriptEnvironment
    {
        public ScriptEnvironment(string[] scriptArgs)
        {
            ScriptArgs = scriptArgs;
        }

        public IReadOnlyList<string> ScriptArgs { get; private set; }
    }
}