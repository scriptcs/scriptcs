using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Common.Logging;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class Repl 
    {
        public static readonly string[] DefaultReferences = new[] { "System", "System.Core", "System.Data", "System.Data.DataSetExtensions", "System.Xml", "System.Xml.Linq" };
        public static readonly string[] DefaultNamespaces = new[] { "System", "System.Collections.Generic", "System.Linq", "System.Text", "System.Threading.Tasks" };

        public IFileSystem FileSystem { get; private set; }
        public IScriptEngine ScriptEngine { get; private set; }
        public ILog Logger { get; private set; }
        public IConsole Console { get; private set; } 
        public ScriptPackSession ScriptPackSession { get; private set; }
        public IEnumerable<string> References { get; private set; } 

        public Repl(IFileSystem fileSystem, IScriptEngine scriptEngine, ILog logger, IConsole console)
        {
            FileSystem = fileSystem;
            ScriptEngine = scriptEngine;
            Logger = logger;
            Console = console;
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

        }

        public void Terminate()
        {
            Logger.Debug("Terminating packs");
            ScriptPackSession.TerminatePacks();
        }

        public void Execute(string script)
        {
            var foregroundColor = Console.ForegroundColor;

            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                ScriptEngine.Execute(script, References, DefaultNamespaces, ScriptPackSession);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\r\n" + ex + "\r\n");
            }
            Console.ForegroundColor = foregroundColor;
        }
    }
}
