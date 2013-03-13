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
        private const string CompiledScriptClass = "Submission#0";
        private const string CompiledScriptMethod = "<Factory>";

        public RoslynScriptDebuggerEngine(IScriptHostFactory scriptHostFactory)
            : base(scriptHostFactory)
        {
        }

        protected override void Execute(string code, Session session)
        {
            var submission = session.CompileSubmission<object>(code);
            var exeBytes = new byte[0];
            var pdbBytes = new byte[0];
            var compileSuccess = false;

            using (var exeStream = new MemoryStream())
            using (var pdbStream = new MemoryStream()) 
            {
                var result = submission.Compilation.Emit(exeStream, pdbStream: pdbStream);
                compileSuccess = result.Success;

                if (result.Success) 
                {
                    exeBytes = exeStream.ToArray();
                    pdbBytes = pdbStream.ToArray();
                }
                else 
                {
                    var errors = String.Join(Environment.NewLine, result.Diagnostics.Select(x => x.ToString()));
                    Console.WriteLine(errors);
                }
            }

            if (compileSuccess) 
            {
                var assembly = AppDomain.CurrentDomain.Load(exeBytes, pdbBytes);
                var type = assembly.GetType(CompiledScriptClass);
                var method = type.GetMethod(CompiledScriptMethod, BindingFlags.Static | BindingFlags.Public);

                try
                {
                    method.Invoke(null, new[] { session });
                }
                catch (Exception e)
                {
                    var message = 
                        string.Format(
                        "Exception Message: {0} {1}Stack Trace:{2}",
                        e.InnerException.Message,
                        Environment.NewLine, 
                        e.InnerException.StackTrace);
                    throw new ScriptExecutionException(message);
                }
            }
        }
    }
}