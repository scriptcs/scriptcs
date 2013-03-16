using System;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using Roslyn.Scripting;
using ScriptCs.Exceptions;

namespace ScriptCs.Engine.Roslyn
{
    public class RoslynScriptDebuggerEngine : RoslynScriptEngine
    {
        private const string CompiledScriptClass = "Submission#0";
        private const string CompiledScriptMethod = "<Factory>";
        private readonly ILog _logger;

        public RoslynScriptDebuggerEngine(IScriptHostFactory scriptHostFactory, ILog logger)
            : base(scriptHostFactory, logger)
        {
            this._logger = logger;
        }

        protected override void Execute(string code, Session session)
        {
            _logger.Debug("Compiling submission");
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
                    _logger.Debug("Compilation was successful.");
                    exeBytes = exeStream.ToArray();
                    pdbBytes = pdbStream.ToArray();
                }
                else 
                {
                    var errors = String.Join(Environment.NewLine, result.Diagnostics.Select(x => x.ToString()));
                    _logger.ErrorFormat("Error occurred when compiling: {0})", errors);
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
                    _logger.Error("An error occurred when executing the scripts.");
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