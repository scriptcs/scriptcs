using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using Common.Logging;
using ScriptCs.Contracts;
using ServiceStack.Text;

namespace ScriptCs
{
    public class Repl
    {
        public static readonly string[] DefaultReferences = new[] { "System", "System.Core", "System.Data", "System.Data.DataSetExtensions", "System.Xml", "System.Xml.Linq" };
        public static readonly string[] DefaultNamespaces = new[] { "System", "System.Collections.Generic", "System.Linq", "System.Text", "System.Threading.Tasks" };

        public static readonly string NewLine = Environment.NewLine;

        public const string InputCaret = "> ";
        public const string BackspaceChar = "\b \b";

        public IFileSystem FileSystem { get; private set; }
        public IScriptEngine ScriptEngine { get; private set; }
        public IFilePreProcessor FilePreProcessor { get; private set; }
        public ILog Logger { get; private set; }
        public IConsole Console { get; private set; }
        public ScriptPackSession ScriptPackSession { get; private set; }
        public IEnumerable<string> References { get; private set; }

        private readonly ReplCommands ReplCommands;

        public Repl(IFileSystem fileSystem, IScriptEngine scriptEngine, ILog logger, IConsole console, IFilePreProcessor filePreProcessor)
        {
            FileSystem = fileSystem;
            ScriptEngine = scriptEngine;
            FilePreProcessor = filePreProcessor;
            Logger = logger;
            Console = console;

            ReplCommands = new ReplCommands(this, NewLine);
        }

        public void Initialize(IEnumerable<string> paths, IEnumerable<IScriptPack> scriptPacks)
        {
            References = DefaultReferences.Union(paths);
            var bin = Path.Combine(FileSystem.CurrentDirectory, "bin");

            ScriptEngine.BaseDirectory = bin;

            Logger.Debug("Initializing script packs");
            var scriptPackSession = new ScriptPackSession(scriptPacks);

            scriptPackSession.InitializePacks();
            ScriptPackSession = scriptPackSession;

            Script = new ReplScript(NewLine, References, DefaultNamespaces, ScriptEngine, ScriptPackSession);
        }

        public bool Running { get; private set; }

        public ReplScript Script { get; private set; }

        public void Run()
        {
            Running = true;

            while (Running)
            {
                if (Script.PendingLine == null && !Script.MissingClosingChar.HasValue)
                    Console.Write(InputCaret);
                ExecuteChar();
            }
        }

        private void ExecuteChar()
        {
            var keyInfo = Console.ReadKey(true);
            var c = keyInfo.KeyChar;
            var key = keyInfo.Key;

            switch (key)
            {
                case ConsoleKey.Backspace:
                    ProcessBackspace();
                    break;
                case ConsoleKey.Enter:
                    Execute();
                    break;
                default:
                    var s = new string(c, 1);
                    Script.Append(s);
                    Console.Write(s);
                    break;
            }
        }

        private void ProcessBackspace()
        {
            if (Script.RemoveInput(1))
                Console.Write(BackspaceChar);
        }

        public void Terminate()
        {
            Logger.Debug("Terminating packs");
            ScriptPackSession.TerminatePacks();
            Running = false;
        }

        public void Execute()
        {
            var script = Script.PendingLine;

            if (script == null)
            {
                Running = false;
                return;
            }

            Console.Write(NewLine);

            var command = ReplCommands.Get(script);
            if (command == null)
            {
                var foregroundColor = Console.ForegroundColor;

                try
                {
                    if (PreProcessorUtil.IsLoadLine(script))
                    {
                        var filepath = PreProcessorUtil.GetPath(PreProcessorUtil.LoadString, script);
                        if (FileSystem.FileExists(filepath))
                        {
                            var processorResult = FilePreProcessor.ProcessFile(filepath);
                            script = processorResult.Code;
                        }
                        else
                        {
                            throw new FileNotFoundException(string.Format("Could not find script '{0}'", filepath), filepath);
                        }
                    }
                    else if (PreProcessorUtil.IsRLine(script))
                    {
                        var assemblyPath = PreProcessorUtil.GetPath(PreProcessorUtil.RString, script);
                        if (FileSystem.FileExists(assemblyPath))
                        {
                            References = References.Union(new[] {assemblyPath});
                            return;
                        }
                        else
                        {
                            throw new FileNotFoundException(string.Format("Could not find assembly '{0}'", assemblyPath), assemblyPath);
                        }
                    }

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    //var result = ScriptEngine.Execute(script, References, DefaultNamespaces, ScriptPackSession);
                    var result = Script.Execute();

                    if (result.Result != null)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(result.Result.ToJsv());
                    }
                    else if (result.RuntimeException != null)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(NewLine + result.RuntimeException + NewLine);
                    }
                    else if (result.CompilationException != null)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(NewLine + result.CompilationException + NewLine);
                        if (result.CompilationException.Message.Contains("CS1024: Preprocessor directive expected"))
                        {
                            Console.WriteLine("Try #help...");
                        }
                    }

                    Console.ForegroundColor = foregroundColor;
                    if (result.ScriptIsMissingClosingChar.HasValue)
                    {
                        Script.MissingClosingChar = result.ScriptIsMissingClosingChar;
                        Console.Write(new string(' ', InputCaret.Length));
                    }
                    else
                    {
                        Script.MissingClosingChar = null;
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(NewLine + ex + NewLine);
                }
            }
            else
            {
                object output = null;
                try
                {
                    output = command.Execute();
                }
                catch (Exception ex)
                {
                    output = ex;
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                Script.EmptyLine();
                Script.Execute();
                if (output != null)
                    Console.WriteLine(output.ToJsv());
            }

            Console.ResetColor();
        }
    }
}
