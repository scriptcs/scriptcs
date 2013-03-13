﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Roslyn.Scripting;

namespace ScriptCs.Engine.Roslyn
{
    public class RoslynScriptDebuggerEngine : RoslynScriptEngine
    {
        private const string CompiledScriptClass = "Submission#0";
        private const string CompiledScriptMethod = "<Factory>";
        private const string AttachMessageTemplate =
            "Attach to process {0} and press ENTER. Then use the 'go' command in the debugger.";

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
            using (var pdbStream = new MemoryStream()) {
                var result = submission.Compilation.Emit(exeStream, pdbStream: pdbStream);
                compileSuccess = result.Success;

                if (result.Success) {
                    exeBytes = exeStream.ToArray();
                    pdbBytes = pdbStream.ToArray();
                } else {
                    var errors = String.Join(Environment.NewLine, result.Diagnostics.Select(x => x.ToString()));
                    Console.WriteLine(errors);
                }
            }

            if (compileSuccess) {
                var assembly = AppDomain.CurrentDomain.Load(exeBytes, pdbBytes);
                var type = assembly.GetType(CompiledScriptClass);
                var method = type.GetMethod(CompiledScriptMethod, BindingFlags.Static | BindingFlags.Public);

                var pid = Process.GetCurrentProcess().Id;
                Console.WriteLine(AttachMessageTemplate, pid);
                Console.ReadLine();

                method.Invoke(null, new[] { session });
            }
        }
    }
}