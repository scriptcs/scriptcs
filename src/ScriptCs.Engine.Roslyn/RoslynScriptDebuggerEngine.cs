using Common.Logging;
using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Common.Logging;
using Roslyn.Scripting;

using ScriptCs.Contracts;
using ScriptCs.Exceptions;

namespace ScriptCs.Engine.Roslyn
{
    public class RoslynScriptDebuggerEngine : RoslynScriptCompilerEngine
    {
        public RoslynScriptDebuggerEngine(IScriptHostFactory scriptHostFactory, ILog logger)
            : base(scriptHostFactory, logger)
        {
        }
        
        protected override Assembly LoadAssembly(byte[] exeBytes, byte[] pdbBytes)
        {
            _logger.Debug("Loading assembly into appdomain.");
            var assembly = AppDomain.CurrentDomain.Load(exeBytes, pdbBytes);
            _logger.Debug("Retrieving compiled script class (reflection).");
            var type = assembly.GetType(CompiledScriptClass);
            _logger.Debug("Retrieving compiled script method (reflection).");
            var method = type.GetMethod(CompiledScriptMethod, BindingFlags.Static | BindingFlags.Public);

            try
            {
                this._logger.Debug("Invoking method.");
                scriptResult.ReturnValue = method.Invoke(null, new[] { session });
            }
            catch (Exception executeException)
            {
                scriptResult.CompileExceptionInfo = ExceptionDispatchInfo.Capture(compileException);
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
                _logger.Debug("Loading assembly into appdomain.");
                var assembly = AppDomain.CurrentDomain.Load(exeBytes, pdbBytes);
                _logger.Debug("Retrieving compiled script class (reflection).");
                var type = assembly.GetType(CompiledScriptClass);
                _logger.Debug("Retrieving compiled script method (reflection).");
                var method = type.GetMethod(CompiledScriptMethod, BindingFlags.Static | BindingFlags.Public);

                try
                {
                    _logger.Debug("Invoking method.");
                    scriptResult.ReturnValue = method.Invoke(null, new[] { session });
                }
                catch (Exception executeException)
                {
                    scriptResult.ExecuteExceptionInfo = ExceptionDispatchInfo.Capture(executeException);
                    _logger.Error("An error occurred when executing the scripts.");
                    var message = 
                        string.Format(
                        "Exception Message: {0} {1}Stack Trace:{2}",
                        executeException.InnerException.Message,
                        Environment.NewLine, 
                        executeException.InnerException.StackTrace);
                    throw new ScriptExecutionException(message, executeException);
                }
            }

            return scriptResult;
        }
    }
}