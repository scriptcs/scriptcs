using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            IFilePreProcessor filePreProcessor, IEnumerable<IReplCommand> replCommands) : base(fileSystem, filePreProcessor, scriptEngine, logger)
        {
            _scriptArgs = scriptArgs;
            _serializer = serializer;
            Console = console;
            Commands = replCommands != null ? replCommands.ToList() : new List<IReplCommand>();
        }

        public string Buffer { get; set; }

        public IConsole Console { get; private set; }

        public List<IReplCommand> Commands { get; private set; } 

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
                    var arguments = script.Split(' ');
                    var command = Commands.FirstOrDefault(x => x.CommandName == arguments[0].Substring(1));

                    if (command != null)
                    {
                        var argsToPass = new List<object>();
                        foreach (var argument in arguments.Skip(1))
                        {
                            try
                            {
                                var argumentResult = ScriptEngine.Execute(argument, _scriptArgs, References, DefaultNamespaces, ScriptPackSession);
                                //if Roslyn can evaluate the argument, use its value, otherwise assume the string

                                argsToPass.Add(argumentResult.ReturnValue ?? argument);
                            }
                            catch (Exception)
                            {
                                argsToPass.Add(argument);
                            }
                        }
                        var commandResult = command.Execute(this, argsToPass.ToArray());
                        if (commandResult != null)
                        {
                            //if command has a result, print it
                            Console.WriteLine(_serializer.Serialize(commandResult));
                        }

                        Buffer = null;

                        return new ScriptResult
                        {
                            ReturnValue = commandResult
                        };
                    }
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

                var result = ScriptEngine.Execute(Buffer, _scriptArgs, References, DefaultNamespaces, ScriptPackSession);
                if (result == null) return new ScriptResult();

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

                if (result.IsPendingClosingChar)
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
