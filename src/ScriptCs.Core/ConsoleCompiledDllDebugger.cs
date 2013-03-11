using System;
using System.Diagnostics;
using System.Reflection;

namespace ScriptCs
{
    public class ConsoleCompiledDllDebugger : ICompiledDllDebugger
    {
        private const string CompiledScriptClass = "Submission#0";
        private const string CompiledScriptMethod = "<Factory>";
        private const string AttachMessageTemplate =
            "Attach to process {0} and press ENTER. Then use the 'go' command in the debugger.";

        public void Run(string dllPath, ISession session)
        {
            var assembly = Assembly.LoadFrom(dllPath);
            var type = assembly.GetType(CompiledScriptClass);
            var method = type.GetMethod(CompiledScriptMethod, BindingFlags.Static | BindingFlags.Public);

            var pid = Process.GetCurrentProcess().Id;
            Console.WriteLine(AttachMessageTemplate, pid);
            Console.ReadLine();

            method.Invoke(null, new[] { session.WrappedSession });
        }
    }
}
