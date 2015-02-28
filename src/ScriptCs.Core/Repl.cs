using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Common.Logging;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class Repl : ScriptExecutor, IRepl
    {
        private readonly string[] _scriptArgs;

        private readonly IObjectSerializer _serializer;

        public Repl(
            string[] scriptArgs,
            IFileSystem fileSystem,
            IScriptEngine scriptEngine,
            IObjectSerializer serializer,
            ILog logger,
            IScriptLibraryComposer composer,
            IConsole console,
            IFilePreProcessor filePreProcessor,
            IEnumerable<IReplCommand> replCommands)
            : base(fileSystem, filePreProcessor, scriptEngine, logger, composer)
        {
            _scriptArgs = scriptArgs;
            _serializer = serializer;
            Console = console;
            Commands = replCommands != null ? replCommands.Where(x => x.CommandName != null).ToDictionary(x => x.CommandName, x => x) : new Dictionary<string, IReplCommand>();
        }

        public string Buffer { get; set; }

        public IConsole Console { get; private set; }

        public Dictionary<string, IReplCommand> Commands { get; private set; }

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
                if (script.StartsWith(":"))
                {
                    var tokens = script.Split(' ');
                    if (tokens[0].Length > 1)
                    {
                        var command = Commands.FirstOrDefault(x => x.Key == tokens[0].Substring(1));

                        if (command.Value != null)
                        {
                            var argsToPass = new List<object>();
                            foreach (var argument in tokens.Skip(1))
                            {
                                var argumentResult = ScriptEngine.Execute(
                                    argument, _scriptArgs, References, Namespaces, ScriptPackSession);

                                if (argumentResult.CompileExceptionInfo != null)
                                {
                                    throw new Exception(
                                        GetInvalidCommandArgumentMessage(argument),
                                        argumentResult.CompileExceptionInfo.SourceException);
                                }

                                if (argumentResult.ExecuteExceptionInfo != null)
                                {
                                    throw new Exception(
                                        GetInvalidCommandArgumentMessage(argument),
                                        argumentResult.ExecuteExceptionInfo.SourceException);
                                }

                                if (!argumentResult.IsCompleteSubmission)
                                {
                                    throw new Exception(GetInvalidCommandArgumentMessage(argument));
                                }

                                argsToPass.Add(argumentResult.ReturnValue);
                            }

                            var commandResult = command.Value.Execute(this, argsToPass.ToArray());
                            return ProcessCommandResult(commandResult);
                        }
                    }
                }

                var preProcessResult = FilePreProcessor.ProcessScript(script);

                ImportNamespaces(preProcessResult.Namespaces.ToArray());

                foreach (var reference in preProcessResult.References)
                {
                    var referencePath = FileSystem.GetFullPath(Path.Combine(FileSystem.BinFolder, reference));
                    AddReferences(FileSystem.FileExists(referencePath) ? referencePath : reference);
                }

                Console.ForegroundColor = ConsoleColor.Cyan;
                
                InjectScriptLibraries(FileSystem.CurrentDirectory, preProcessResult, ScriptPackSession.State);

                Buffer = (Buffer == null)
                    ? preProcessResult.Code
                    : Buffer + Environment.NewLine + preProcessResult.Code;

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

                if (result.InvalidNamespaces.Any())
                {
                    RemoveNamespaces(result.InvalidNamespaces.ToArray());
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

        private static string GetInvalidCommandArgumentMessage(string argument)
        {
            return string.Format(CultureInfo.InvariantCulture, "Argument is not a valid expression: {0}", argument);
        }

        private ScriptResult ProcessCommandResult(object commandResult)
        {
            Buffer = null;

            if (commandResult != null)
            {
                if (commandResult is ScriptResult)
                {
                    var scriptCommandResult = commandResult as ScriptResult;
                    if (scriptCommandResult.ReturnValue != null)
                    {
                        Console.WriteLine(_serializer.Serialize(scriptCommandResult.ReturnValue));
                    }
                    return scriptCommandResult;
                }

                //if command has a result, print it
                Console.WriteLine(_serializer.Serialize(commandResult));

                return new ScriptResult(returnValue: commandResult);
            }

            return ScriptResult.Empty;
        }
    }
}
