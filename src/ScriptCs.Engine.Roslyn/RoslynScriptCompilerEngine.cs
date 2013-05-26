using System;
using System.IO;
using System.Linq;
using Common.Logging;
using Roslyn.Scripting;

namespace ScriptCs.Engine.Roslyn
{
    public abstract class RoslynScriptCompilerEngine : RoslynScriptEngine
    {
        protected const string CompiledScriptClass = "Submission#0";
        protected const string CompiledScriptMethod = "<Factory>";
        protected readonly ILog _logger;

        public RoslynScriptCompilerEngine(IScriptHostFactory scriptHostFactory, ILog logger)
            : base(scriptHostFactory, logger)
        {
            _logger = logger;
        }

        protected override ScriptResult Execute(string code, Session session)
        {
            Guard.AgainstNullArgument("session", session);

            var scriptResult = new ScriptResult();
            Submission<object> submission = null;

            _logger.Debug("Compiling submission");
            try
            {
                submission = session.CompileSubmission<object>(code);
            }
            catch (Exception compileException)
            {
                scriptResult.CompileException = compileException;
            }

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
                ExecuteScriptInSession(session, exeBytes, pdbBytes, scriptResult);
            }

            return scriptResult;
        }

        protected abstract void ExecuteScriptInSession(Session session, byte[] exeBytes, byte[] pdbBytes, ScriptResult scriptResult);
    }
}