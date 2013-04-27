using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Roslyn.Scripting;
using ScriptCs.Exceptions;

namespace ScriptCs.Engine.Roslyn
{
    public class RoslynScriptDebuggerEngine : RoslynScriptEngine
    {
        public RoslynScriptDebuggerEngine(IScriptHostFactory scriptHostFactory)
            : base(scriptHostFactory)
        {
        }

        protected override void Execute(string code, Session session)
        {
            CompileAndExecute(code, CacheAssembly, AssemblyCacheDate, AssemblyName, session);
        }
    }
}