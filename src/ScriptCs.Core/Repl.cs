using System;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;
using Common.Logging;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class Repl : ScriptExecutor
    {
        private readonly string[] _scriptArgs;

        private readonly IObjectSerializer _serializer;

        public Repl(
            string[] scriptArgs,
            IFileSystem fileSystem,
            IScriptEngine scriptEngine,
            IObjectSerializer serializer,
            ILog logger,
            IConsole console,
            IFilePreProcessor filePreProcessor) : base(fileSystem, filePreProcessor, scriptEngine, logger)
        {
            _scriptArgs = scriptArgs;
            _serializer = serializer;
            Console = console;
        }

        public string Buffer { get; set; }

        public IConsole Console { get; private set; }

        public override void Terminate()
        {
            base.Terminate();
            Logger.Debug("Exiting console");
            Console.Exit();
        }

        public override ScriptResult Execute(string script, params string[] scriptArgs)
        {
            Guard.AgainstNullArgument("script", script);

            try
            {
                if (script.StartsWith("#clear", StringComparison.OrdinalIgnoreCase))
                {
                    Console.Clear();
                    return ScriptResult.Empty;
                }

                if (script.StartsWith("#reset"))
                {
                    Reset();
                    return ScriptResult.Empty;
                }

                if (script.StartsWith(":cd", StringComparison.OrdinalIgnoreCase))
                {
                    var m = Regex.Match(script, @":cd\s+(.*)");

                    var relativePath = m.Groups[1].Value;

                    FileSystem.CurrentDirectory = Path.Combine(FileSystem.CurrentDirectory, relativePath);

                    return ScriptResult.Empty;
                }

                if (script.StartsWith(":cwd", StringComparison.OrdinalIgnoreCase))
                {
                    var dir = FileSystem.CurrentDirectory;

                    Console.ForegroundColor = ConsoleColor.Yellow;

                    Console.WriteLine(dir);

                    return ScriptResult.Empty;
                }

                var preProcessResult = FilePreProcessor.ProcessScript(script);

                ImportNamespaces(preProcessResult.Namespaces.ToArray());

                foreach (var reference in preProcessResult.References)
                {
                    var referencePath = FileSystem.GetFullPath(Path.Combine(Constants.BinFolder, reference));
                    AddReferences(FileSystem.FileExists(referencePath) ? referencePath : reference);
                }

                Console.ForegroundColor = ConsoleColor.Cyan;

                Buffer += preProcessResult.Code;

                var result = ScriptEngine.Execute(Buffer, _scriptArgs, References, Namespaces, ScriptPackSession);
                if (result == null) return ScriptResult.Empty;

                if (result.CompileExceptionInfo != null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(result.CompileExceptionInfo.SourceException.Message);
                }

                if (result.ExecuteExceptionInfo != null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(result.ExecuteExceptionInfo.SourceException.Message);
                }

                if (!result.IsCompleteSubmission)
                {
                    return result;
                }

                if (result.ReturnValue != null)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;

                    var serializedResult = _serializer.Serialize(result.ReturnValue);

                    Console.WriteLine(serializedResult);
                }

                Buffer = null;
                return result;
            }
            catch (FileNotFoundException fileEx)
            {
                RemoveReferences(fileEx.FileName);

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Environment.NewLine + fileEx + Environment.NewLine);

                return new ScriptResult(compilationException: fileEx);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Environment.NewLine + ex + Environment.NewLine);

                return new ScriptResult(executionException: ex);
            }
            finally
            {
                Console.ResetColor();
            }
        }
    }
}
