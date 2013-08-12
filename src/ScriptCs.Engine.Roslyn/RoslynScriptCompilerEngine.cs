using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Common.Logging;
using Roslyn.Scripting;
using ScriptCs.Exceptions;

namespace ScriptCs.Engine.Roslyn
{
    public abstract class RoslynScriptCompilerEngine : RoslynScriptEngine
    {
        protected const string CompiledScriptClass = "Submission#0";
        protected const string CompiledScriptMethod = "<Factory>";
        
        protected RoslynScriptCompilerEngine(IScriptHostFactory scriptHostFactory, ILog logger)
>>>>>>> # Added RoslynScriptDllGeneratorEngine.cs which saves generated file to .dll
            : base(scriptHostFactory, logger)
        {
            Logger = logger;
        }
        
        protected ILog Logger { get; private set; }

        protected override ScriptResult Execute(string code, Session session)
        {
            Guard.AgainstNullArgument("session", session);

            var scriptResult = new ScriptResult();
            Submission<object> submission = null;

            this.Logger.Debug("Compiling submission");
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
                    this.Logger.Debug("Compilation was successful.");
                    exeBytes = exeStream.ToArray();
                    pdbBytes = pdbStream.ToArray();
                }
                else
                {
                    var errors = String.Join(Environment.NewLine, result.Diagnostics.Select(x => x.ToString()));
                    this.Logger.ErrorFormat("Error occurred when compiling: {0})", errors);
                }
            }

            if (compileSuccess)
            {
                var assembly = this.LoadAssembly(exeBytes, pdbBytes);
                this.Logger.Debug("Retrieving compiled script class (reflection).");
                var type = assembly.GetType(CompiledScriptClass);
                this.Logger.Debug("Retrieving compiled script method (reflection).");
                var method = type.GetMethod(CompiledScriptMethod, BindingFlags.Static | BindingFlags.Public);

                try
                {
                    this.Logger.Debug("Invoking method.");
                    scriptResult.ReturnValue = method.Invoke(null, new[] { session });
                }
                catch (Exception executeException)
                {
                    scriptResult.ExecuteExceptionInfo = ExceptionDispatchInfo.Capture(executeException);
                    this.Logger.Error("An error occurred when executing the scripts.");
                    var message = string.Format(
                        "Exception Message: {0} {1}Stack Trace:{2}",
                        executeException.InnerException.Message,
                        Environment.NewLine,
                        executeException.InnerException.StackTrace);
                    throw new ScriptExecutionException(message);
                }
>>>>>>> # Added RoslynScriptDllGeneratorEngine.cs which saves generated file to .dll
            }

            return scriptResult;
        }

        protected abstract Assembly LoadAssembly(byte[] exeBytes, byte[] pdbBytes);
>>>>>>> # Added RoslynScriptDllGeneratorEngine.cs which saves generated file to .dll
    }
}