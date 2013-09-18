﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Common.Logging;
using Roslyn.Compilers;
using Roslyn.Scripting;
using ScriptCs.Contracts;
using ScriptCs.Exceptions;

namespace ScriptCs.Engine.Roslyn
{
    public abstract class RoslynScriptCompilerEngine : RoslynScriptEngine
    {
        protected const string CompiledScriptClass = "Submission#0";
        protected const string CompiledScriptMethod = "<Factory>";
        
        protected RoslynScriptCompilerEngine(IScriptHostFactory scriptHostFactory, ILog logger)
            : base(scriptHostFactory, logger)
        {
        }

        protected override ScriptResult Execute(string code, Session session)
        {
            Guard.AgainstNullArgument("session", session);

            var scriptResult = new ScriptResult();
            Submission<object> submission = null;

            this.Logger.Debug("Compiling submission");
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

                    Logger.Debug("Retrieving compiled script class (reflection).");

                    // the following line can throw NullReferenceException, if that happens it's useful to notify that an error ocurred
                    var type = assembly.GetType(CompiledScriptClass);
                    Logger.Debug("Retrieving compiled script method (reflection).");
                    var method = type.GetMethod(CompiledScriptMethod, BindingFlags.Static | BindingFlags.Public);

                    try
                    {
                        this.Logger.Debug("Invoking method.");
                        scriptResult.ReturnValue = method.Invoke(null, new[] {session});
                    }
                    catch (Exception executeException)
                    {
                        var ex = executeException.InnerException ?? executeException;
                        scriptResult.ExecuteExceptionInfo = ExceptionDispatchInfo.Capture(ex);
                        this.Logger.Error("An error occurred when executing the scripts.");
                    }
                }
            }
            catch (Exception compileException)
            {
                //we catch Exception rather than CompilationErrorException because there might be issues with assembly loading too
                scriptResult.CompileExceptionInfo = ExceptionDispatchInfo.Capture(new ScriptCompilationException(compileException.Message, compileException));
            }

            return scriptResult;
        }

        protected abstract Assembly LoadAssembly(byte[] exeBytes, byte[] pdbBytes);
    }
}