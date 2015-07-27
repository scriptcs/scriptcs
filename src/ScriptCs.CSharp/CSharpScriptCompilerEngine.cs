using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.CSharp;
using ScriptCs.Contracts;
using ScriptCs.Engine.Common;
using ScriptCs.Exceptions;

namespace ScriptCs.CSharp
{
    public abstract class CSharpScriptCompilerEngine : CommonScriptEngine
    {
        private const string CompiledScriptClass = "Submission#0";

        private const string CompiledScriptMethod = "<Factory>";

        protected CSharpScriptCompilerEngine(IScriptHostFactory scriptHostFactory, ILogProvider logProvider)
            : base(scriptHostFactory, logProvider)
        {
        }

        protected abstract bool ShouldCompile();

        protected abstract Assembly LoadAssembly(byte[] exeBytes, byte[] pdbBytes);

        protected abstract Assembly LoadAssemblyFromCache();

        protected override ScriptResult Execute(string code, object globals, SessionState<ScriptState> sessionState)
        {
            return ShouldCompile()
                ? CompileAndExecute(code, globals)
                : InvokeEntryPointMethod(globals, LoadAssemblyFromCache());
        }

        protected override ScriptState GetScriptState(string code, object globals)
        {
            return null;
        }

        protected ScriptResult CompileAndExecute(string code, object globals)
        {
            Logger.Debug("Compiling submission");
            try
            {
                var script = CSharpScript.Create(code, ScriptOptions);
                var compilation = script.GetCompilation();

                using (var exeStream = new MemoryStream())
                using (var pdbStream = new MemoryStream())
                {
                    var result = compilation.Emit(exeStream, pdbStream: pdbStream);

                    if (result.Success)
                    {
                        Logger.Debug("Compilation was successful.");

                        var assembly = LoadAssembly(exeStream.ToArray(), pdbStream.ToArray());
                        return InvokeEntryPointMethod(globals, assembly);
                    }

                    var errors = string.Join(Environment.NewLine, result.Diagnostics.Select(x => x.ToString()));

                    Logger.ErrorFormat("Error occurred when compiling: {0})", errors);

                    return new ScriptResult(compilationException: new ScriptCompilationException(errors));
                }
            }
            catch (Exception compileException)
            {
                //we catch Exception rather than CompilationErrorException because there might be issues with assembly loading too
                return new ScriptResult(compilationException: new ScriptCompilationException(compileException.Message, compileException));
            }
        }

        private ScriptResult InvokeEntryPointMethod(object globals, Assembly assembly)
        {
            Logger.Debug("Retrieving compiled script class (reflection).");

            // the following line can throw NullReferenceException, if that happens it's useful to notify that an error ocurred
            var type = assembly.GetType(CompiledScriptClass);
            Logger.Debug("Retrieving compiled script method (reflection).");
            var method = type.GetMethod(CompiledScriptMethod, BindingFlags.Static | BindingFlags.Public);

            try
            {
                Logger.Debug("Invoking method.");
                var submissionStates = new object[2];
                submissionStates[0] = globals;
                return new ScriptResult(returnValue: method.Invoke(null, new[] { submissionStates }));
            }
            catch (Exception executeException)
            {
                Logger.Error("An error occurred when executing the scripts.");

                var ex = executeException.InnerException ?? executeException;

                return new ScriptResult(executionException: ex);
            }
        }
    }
}