using System;
using System.IO;
using System.Runtime.ExceptionServices;
using Common.Logging;
using ScriptCs.Contracts;
using ServiceStack.Text;

namespace ScriptCs
{
    public class Repl : ScriptExecutor
    {
        private readonly string[] _scriptArgs;

        public Repl(
            string[] scriptArgs,
            IFileSystem fileSystem,
            IScriptEngine scriptEngine,
            ILog logger,
            IConsole console,
            IFilePreProcessor filePreProcessor, ICompilationExceptionWriter compilationExceptionWriter)
            : base(fileSystem, filePreProcessor, scriptEngine, logger)
        {
            _scriptArgs = scriptArgs;
            Console = console;
            CompilationExceptionWriter = compilationExceptionWriter;
        }

        public string Buffer { get; set; }

        public IConsole Console { get; private set; }

        public ICompilationExceptionWriter CompilationExceptionWriter { get; private set; }

        public override void Terminate()
        {
            base.Terminate();
            Logger.Debug("Exiting console");
            Console.Exit();
        }

        public override ScriptResult Execute(string script, params string[] scriptArgs)
        {
            try
            {
                var preProcessResult = FilePreProcessor.ProcessScript(script);

                ImportNamespaces(preProcessResult.Namespaces.ToArray());

                foreach (var reference in preProcessResult.References)
                {
                    var referencePath = FileSystem.GetFullPath(Path.Combine(Constants.BinFolder, reference));
                    AddReferences(FileSystem.FileExists(referencePath) ? referencePath : reference);
                }

                Console.ForegroundColor = ConsoleColor.Cyan;

                Buffer += preProcessResult.Code;

                var result = ScriptEngine.Execute(Buffer, _scriptArgs, References, DefaultNamespaces, ScriptPackSession);
                if (result == null) return new ScriptResult();

                if (result.CompileExceptionInfo != null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    
                    if (CompilationExceptionWriter != null)
                    {
                        try
                        {
                            using (var outfile = new StreamWriter(CompilationExceptionWriter.CompilationExceptionFilePath, true))
                            {
                                outfile.WriteLine(result.CompileExceptionInfo.SourceException.ToString() + "\r\n");
                                Console.WriteLine("Compilation exception written to log.");
                            }
                        }
                        catch
                        {
                            Console.WriteLine(result.CompileExceptionInfo.SourceException.ToString());
                        }
                    }
                    else
                    {
                        Console.WriteLine(result.CompileExceptionInfo.SourceException.ToString());
                    }
                }

                if (result.ExecuteExceptionInfo != null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(result.ExecuteExceptionInfo.SourceException.ToString());
                }

                if (result.IsPendingClosingChar)
                {
                    return result;
                }

                if (result.ReturnValue != null)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(result.ReturnValue.ToJsv());
                }

                Buffer = null;
                return result;
            }
            catch (FileNotFoundException fileEx)
            {
                RemoveReferences(fileEx.FileName);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\r\n" + fileEx + "\r\n");
                return new ScriptResult { CompileExceptionInfo = ExceptionDispatchInfo.Capture(fileEx) };
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\r\n" + ex + "\r\n");
                return new ScriptResult { ExecuteExceptionInfo = ExceptionDispatchInfo.Capture(ex) };
            }
            finally
            {
                Console.ResetColor();
            }
        }
    }
}
