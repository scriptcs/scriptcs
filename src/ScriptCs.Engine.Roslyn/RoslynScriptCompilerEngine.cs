using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Roslyn.Scripting;
using ScriptCs.Contracts;
using ScriptCs.Exceptions;

namespace ScriptCs.Engine.Roslyn
{
    public abstract class RoslynScriptCompilerEngine : RoslynScriptEngine
    {
        private const string CompiledScriptClass = "Submission#0";

        private const string CompiledScriptMethod = "<Factory>";

        private readonly ILog _log;

        [Obsolete("Support for Common.Logging types was deprecated in version 0.15.0 and will soon be removed.")]
        protected RoslynScriptCompilerEngine(IScriptHostFactory scriptHostFactory, Common.Logging.ILog logger)
            : this(scriptHostFactory, new CommonLoggingLogProvider(logger))
        {
        }

        protected RoslynScriptCompilerEngine(IScriptHostFactory scriptHostFactory, ILogProvider logProvider)
            : base(scriptHostFactory, logProvider)
        {
            Guard.AgainstNullArgument("logProvider", logProvider);

            _log = logProvider.ForCurrentType();

        }

        protected abstract bool ShouldCompile();

        protected abstract Assembly LoadAssembly(byte[] exeBytes, byte[] pdbBytes);

        protected abstract Assembly LoadAssemblyFromCache();

        protected override ScriptResult Execute(string code, Session session)
        {
            Guard.AgainstNullArgument("session", session);

            return ShouldCompile() 
                ? CompileAndExecute(code, session) 
                : InvokeEntryPointMethod(session, LoadAssemblyFromCache());
        }

        private ScriptResult CompileAndExecute(string code, Session session)
        {
            _log.Debug("Compiling submission");
            try
            {
                var submission = session.CompileSubmission<object>(code);

                using (var exeStream = new MemoryStream())
                using (var pdbStream = new MemoryStream())
                {
                    var result = submission.Compilation.Emit(exeStream, pdbStream: pdbStream);

                    if (result.Success)
                    {
                        _log.Debug("Compilation was successful.");

                        var assembly = LoadAssembly(exeStream.ToArray(), pdbStream.ToArray());

                        return InvokeEntryPointMethod(session, assembly);
                    }

                    var errors = string.Join(Environment.NewLine, result.Diagnostics.Select(x => x.ToString()));
                    
                    _log.ErrorFormat("Error occurred when compiling: {0})", errors);

                    return new ScriptResult(compilationException: new ScriptCompilationException(errors));
                }
            }
            catch (Exception compileException)
            {
                //we catch Exception rather than CompilationErrorException because there might be issues with assembly loading too
                return new ScriptResult(compilationException: new ScriptCompilationException(compileException.Message, compileException));
            }
        }

        private ScriptResult InvokeEntryPointMethod(Session session, Assembly assembly)
        {
            _log.Debug("Retrieving compiled script class (reflection).");

            // the following line can throw NullReferenceException, if that happens it's useful to notify that an error ocurred
            var type = assembly.GetType(CompiledScriptClass);
            _log.Debug("Retrieving compiled script method (reflection).");
            var method = type.GetMethod(CompiledScriptMethod, BindingFlags.Static | BindingFlags.Public);

            try
            {
                _log.Debug("Invoking method.");

                return new ScriptResult(returnValue: method.Invoke(null, new object[] { session }));
            }
            catch (Exception executeException)
            {
                _log.Error("An error occurred when executing the scripts.");

                var ex = executeException.InnerException ?? executeException;

                return new ScriptResult(executionException: ex);
            }
        }
    }
}