using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Text;
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
        public const string BackspaceChar = " \b";

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

            ReplCommands = new ReplCommands(this);
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

            Console.Write(InputCaret);
            while (Running)
            {
                ExecuteChar();
            }
        }

        private void ExecuteChar()
        {
            var keyInfo = Console.ReadKey();
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
                    Script.Append(c.ToString(CultureInfo.CurrentCulture));
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
            Console.Write(NewLine);

            var script = Script.PendingLine;
            
            var command = ReplCommands.Get(script);
            if (command == CommandInfo.Empty)
            {
                if (script == null)
                {
                    Running = false;
                    return;
                }

                var foregroundColor = Console.ForegroundColor;

                try
                {
                    if (PreProcessorUtil.IsLoadLine(script))
                    {
                        var filepath = PreProcessorUtil.GetPath(PreProcessorUtil.LoadString, script);
                        script = FilePreProcessor.ProcessFile(filepath);
                    }
                    else if (PreProcessorUtil.IsRLine(script))
                    {
                        var assemblyPath = PreProcessorUtil.GetPath(PreProcessorUtil.RString, script);
                        References = References.Union(new[] { assemblyPath });
                        return;
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
                            Console.WriteLine("Try #help'...");
                        }
                    }

                    Console.ForegroundColor = foregroundColor;
                    if (result.ScriptIsMissingClosingChar.HasValue)
                        Console.Write(new string(' ', InputCaret.Length));
                    else
                        Console.Write(InputCaret);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\r\n" + ex + "\r\n");
                }
                Console.ForegroundColor = foregroundColor;
            }
            else
            {
                var output = command.Execute();
                if (output != null)
                    Console.WriteLine(output.ToJsv());
                Console.Write(InputCaret);
            }
        }
    }

    public class ReplCommands
    {
        private readonly Repl _repl;

        private readonly List<CommandInfo> _commands = new List<CommandInfo>(); 

        public ReplCommands(Repl repl)
        {
            _repl = repl;

            _commands.Add(new CommandInfo("exit", Exit));
            _commands.Add(new CommandInfo("help", PrintHelp));
        }

        private object Exit(object parameter)
        {
            _repl.Terminate();
            return null;
        }

        public CommandInfo Get(string line)
        {
            if (line == null)
                return CommandInfo.Empty;

            if (!line.StartsWith("#"))
                return CommandInfo.Empty;

            var name = line.Substring(1).ToLowerInvariant();

            var info = _commands.FirstOrDefault(x => x.Name == name);

            return info ?? CommandInfo.Empty;
        }

        private object PrintHelp(object parameter)
        {
            var help = new StringBuilder();
            help.AppendLine("REPL commands:");
            help.AppendLine("  help    Display all available REPL commands");
            help.Append("  exit    Exit");
            return help.ToString();
        }
    }

    public class CommandInfo
    {
        public string Name { get; set; }
        public Func<object, object> Command;
        public object Parameter;
        public static readonly CommandInfo Empty = new CommandInfo();

        private CommandInfo()
        {
        }

        public CommandInfo(string name, Func<object, object> command)
            : this(command, null)
        {
            Name = name;
        }

        public CommandInfo(Func<object, object> command, object parameter)
        {
            if (command == null)
                throw new ArgumentNullException("command");
            Command = command;
            Parameter = parameter;
        }

        public object Execute()
        {
            return Command(Parameter);
        }
    }
}
