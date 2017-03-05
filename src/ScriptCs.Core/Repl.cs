using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ScriptCs.Contracts;
using ScriptCs.PaketDirective;

namespace ScriptCs
{
    public class Repl : ScriptExecutor, IRepl
    {
        private readonly string[] _scriptArgs;

        private readonly IObjectSerializer _serializer;
        private readonly Printers _printers;
        private readonly ILog _log;

        public Repl(
            string[] scriptArgs,
            IFileSystem fileSystem,
            IScriptEngine scriptEngine,
            IObjectSerializer serializer,
            ILogProvider logProvider,
            IScriptLibraryComposer composer,
            IConsole console,
            IFilePreProcessor filePreProcessor,
            IEnumerable<IReplCommand> replCommands,
            Printers printers,
            IScriptInfo scriptInfo,
            IPaketLoader paketLoader)
            : base(fileSystem, filePreProcessor, scriptEngine, logProvider, composer, scriptInfo, paketLoader)
        {
            Guard.AgainstNullArgument("serializer", serializer);
            Guard.AgainstNullArgument("logProvider", logProvider);
            Guard.AgainstNullArgument("console", console);

            _scriptArgs = scriptArgs;
            _serializer = serializer;
            _printers = printers;
            _log = logProvider.ForCurrentType();
            Console = console;
            Commands = replCommands != null ? replCommands.Where(x => x.CommandName != null).ToDictionary(x => x.CommandName, x => x) : new Dictionary<string, IReplCommand>();
        }

        public string Buffer { get; set; }

        public IConsole Console { get; private set; }

        public Dictionary<string, IReplCommand> Commands { get; private set; }

        public override void Terminate()
        {
            base.Terminate();
            _log.Debug("Exiting console");
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

                AddAssemblyReferences(preProcessResult);

                Console.ForegroundColor = ConsoleColor.Cyan;

                InjectScriptLibraries(FileSystem.CurrentDirectory, preProcessResult, ScriptPackSession.State);

                HandleCustomReferences(script, preProcessResult);

                Buffer = (Buffer == null)
                    ? preProcessResult.Code
                    : Buffer + Environment.NewLine + preProcessResult.Code;

                var namespaces = Namespaces.Union(preProcessResult.Namespaces);
                var references = References.Union(preProcessResult.AssemblyReferences);

                var result = ScriptEngine.Execute(Buffer, _scriptArgs, references, namespaces, ScriptPackSession);
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

                    Console.WriteLine(_printers.GetStringFor(result.ReturnValue));
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

        private void HandleCustomReferences(string script, FilePreProcessorResult preProcessResult)
        {
            object tempCustomRefs = null;

            var found = ScriptPackSession.State.TryGetValue(Constants.SessionCustomReferences, out tempCustomRefs);
            if (!found)
            {
                tempCustomRefs = new List<string>();
                ScriptPackSession.State[Constants.SessionCustomReferences] = tempCustomRefs;
            }
            var customRefs = (List<string>) tempCustomRefs;

            //Keep capturing until there are no more directives
            if (script.StartsWith("#"))
            {
                customRefs.AddRange(preProcessResult.CustomReferences);
            }
            else
            {
                preProcessResult.CustomReferences.AddRange(customRefs);
                customRefs.Clear();
                PaketLoader.Load(preProcessResult);
            }
        }

        private void AddAssemblyReferences(FilePreProcessorResult preProcessResult)
        {
            foreach (var reference in preProcessResult.AssemblyReferences)
            {
                var referencePath = FileSystem.GetFullPath(Path.Combine(FileSystem.BinFolder, reference));
                AddReferences(FileSystem.FileExists(referencePath) ? referencePath : reference);
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
