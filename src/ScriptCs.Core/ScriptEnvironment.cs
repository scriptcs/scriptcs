using System;
using System.Collections.Generic;

namespace ScriptCs
{
    public class ScriptEnvironment
    {
        public ScriptEnvironment(string[] scriptArgs)
        {
            ScriptArgs = scriptArgs;
            WorkingDirectory = Environment.CurrentDirectory;
        }

        public IReadOnlyList<string> ScriptArgs { get; private set; }
        public static string WorkingDirectory { get; private set; }
    }
}