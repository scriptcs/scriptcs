using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Common.Logging;

using Roslyn.Scripting;
using ScriptCs.Contracts;
using ScriptCs.Exceptions;

namespace ScriptCs.Engine.Roslyn
{
    public class RoslynScriptCompilerEngine : RoslynScriptEngine
    {
        private const string CompiledScriptClass = "Submission#0";
        private const string CompiledScriptMethod = "<Factory>";
        private readonly IAssemblyLoader _assemblyLoader;
        
        public RoslynScriptCompilerEngine(IScriptHostFactory scriptHostFactory, ILog logger, IAssemblyLoader assemblyLoader)
            : base(scriptHostFactory, logger)
        {
            _assemblyLoader = assemblyLoader;
        }

        protected override ScriptResult Execute(string code, Session session)
        {
            Guard.AgainstNullArgument("session", session);

            var scriptResult = new ScriptResult();

            _assemblyLoader.SetContext(FileName, CacheDirectory);

            if (_assemblyLoader.ShouldCompile())
            {
                CompileAndExecute(code, session, scriptResult);
            }
            else
            {
                var assembly = _assemblyLoader.LoadFromCache();
                InvokeEntryPointMethod(session, assembly, scriptResult);
            }
            
            return scriptResult;
        }

        private void CompileAndExecute(string code, Session session, ScriptResult scriptResult)
        {
            Submission<object> submission = null;

            Logger.Debug("Compiling submission");
            try
            {
                submission = session.CompileSubmission<object>(code);

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
                        Logger.Debug("Compilation was successful.");
                        exeBytes = exeStream.ToArray();
                        pdbBytes = pdbStream.ToArray();
                    }
                    else
                    {
                        var errors = string.Join(Environment.NewLine, result.Diagnostics.Select(x => x.ToString()));
                        Logger.ErrorFormat("Error occurred when compiling: {0})", errors);
                    }
                }

                if (compileSuccess)
                {
                    var assembly = _assemblyLoader.Load(exeBytes, pdbBytes);

                    InvokeEntryPointMethod(session, assembly, scriptResult);
                }
            }
            catch (Exception compileException)
            {
                //we catch Exception rather than CompilationErrorException because there might be issues with assembly loading too
                scriptResult.CompileExceptionInfo =
                    ExceptionDispatchInfo.Capture(new ScriptCompilationException(compileException.Message, compileException));
            }
        }

        private void InvokeEntryPointMethod(Session session, Assembly assembly, ScriptResult scriptResult)
        {
            Logger.Debug("Retrieving compiled script class (reflection).");

            // the following line can throw NullReferenceException, if that happens it's useful to notify that an error ocurred
            var type = assembly.GetType(CompiledScriptClass);
            Logger.Debug("Retrieving compiled script method (reflection).");
            var method = type.GetMethod(CompiledScriptMethod, BindingFlags.Static | BindingFlags.Public);

            try
            {
                Logger.Debug("Invoking method.");
                scriptResult.ReturnValue = method.Invoke(null, new[] { session });
            }
            catch (Exception executeException)
            {
                var ex = executeException.InnerException ?? executeException;
                scriptResult.ExecuteExceptionInfo = ExceptionDispatchInfo.Capture(ex);
                Logger.Error("An error occurred when executing the scripts.");
            }
        }
    }
}