﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class Repl : ScriptExecutor, IRepl
    {
        private readonly string[] _scriptArgs;

        private readonly IObjectSerializer _serializer;
        private readonly ILog _log;
        private readonly Dictionary<Type, Func<object, string>> _printers = new  Dictionary<Type, Func<object, string>>();

        [Obsolete("Support for Common.Logging types was deprecated in version 0.15.0 and will soon be removed.")]
        public Repl(
            string[] scriptArgs,
            IFileSystem fileSystem,
            IScriptEngine scriptEngine,
            IObjectSerializer serializer,
            Common.Logging.ILog logger,
            IScriptLibraryComposer composer,
            IConsole console,
            IFilePreProcessor filePreProcessor,
            IEnumerable<IReplCommand> replCommands)
            : this(
                scriptArgs,
                fileSystem,
                scriptEngine,
                serializer,
                new CommonLoggingLogProvider(logger),
                composer,
                console,
                filePreProcessor,
                replCommands)
        {
        }

        public Repl(
            string[] scriptArgs,
            IFileSystem fileSystem,
            IScriptEngine scriptEngine,
            IObjectSerializer serializer,
            ILogProvider logProvider,
            IScriptLibraryComposer composer,
            IConsole console,
            IFilePreProcessor filePreProcessor,
            IEnumerable<IReplCommand> replCommands)
            : base(fileSystem, filePreProcessor, scriptEngine, logProvider, composer)
        {
            Guard.AgainstNullArgument("serializer", serializer);
            Guard.AgainstNullArgument("logProvider", logProvider);
            Guard.AgainstNullArgument("console", console);

            _scriptArgs = scriptArgs;
            _serializer = serializer;
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

        public void AddCustomPrinter<T>(Func<T, string> printer) {
            Console.WriteLine("adding custom printer");
            _printers[typeof(T)] = x => printer((T) x);
        }

        public override ScriptResult Execute(string script, params string[] scriptArgs)
        {
            Guard.AgainstNullArgument("script", script);
            ScriptEngine.ScriptHostFactory.SetRepl(this);
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

                var namespaces = Namespaces.Union(preProcessResult.Namespaces);
                var references = References.Union(preProcessResult.References);

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
                    Func<object, string> printer;
                    if(_printers.TryGetValue(result.ReturnValue.GetType(), out printer)) {
                        Console.WriteLine(printer(result.ReturnValue));
                    } else {
                        var serializedResult = _serializer.Serialize(result.ReturnValue);
                        Console.WriteLine(serializedResult);
                    }
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
