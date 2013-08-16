using System;
using System.IO;
using PowerArgs;
using ScriptCs.Contracts;

namespace ScriptCs
{
    [ArgExample("scriptcs server.csx -inMemory", "Shows how to start the script running from memory (not compiling to a .dll)")]
    [Serializable]
    public class ScriptCsArgs
    {
        public ScriptCsArgs()
        {
            LogLevel = LogLevel.Info;
            Config = "scriptcs.opts";
        }

        [ArgIgnore]
        public bool Repl { get; set; }

        [ArgPosition(0)]
        [ArgShortcut("script")]
        [ArgDescription("Script file name, must be specified first")]
        public string ScriptName { get; set; }

        [ArgShortcut("?")]
        [ArgDescription("Displays help")]
        public bool Help { get; set; }

        [ArgShortcut("inMemory")]
        [ArgDescription("Flag which determines whether to run in memory or from a .dll")]
        public bool InMemory { get; set; }

        [ArgIgnoreCase]
        [ArgShortcut("log")]
        [DefaultValue(LogLevel.Info)]
        [ArgDescription("Flag which defines the log level used.")]
        public LogLevel LogLevel { get; set; }

        [ArgShortcut("install")]
        [ArgDescription("Installs and restores packages which are specified in packages.config")]
        public string Install { get; set; }

        [ArgShortcut("g")]
        [ArgDescription("Installs and restores global packages which are specified in packages.config")]
        public bool Global { get; set; }


        [ArgShortcut("save")]
        [ArgDescription("Creates a packages.config file based on the packages directory")]
        public bool Save { get; set; }

        [ArgShortcut("clean")]
        [ArgDescription("Cleans installed packages from working directory")]
        public bool Clean { get; set; }

        [ArgShortcut("pre")]
        [ArgDescription("Allows installation of packages' prelease versions")]
        public bool AllowPreRelease { get; set; }

        [ArgShortcut("version")]
        [ArgDescription("Outputs version information")]
        public bool Version { get; set; }

        [ArgShortcut("isolated")]
        [ArgDescription("Runs the script in an isolated AppDomain")]
        public bool Isolated { get; set; }

        [ArgShortcut("modules")]
        [ArgDescription("Specify modules to load")]
        public string Modules { get; set; }

        [ArgShortcut("config")]
        [DefaultValue("scriptcs.opts")]
        [ArgDescription("Defines config file name")]
        public string Config { get; set; }

        public ScriptServices CreateServices(ScriptConsole console = null)
        {
            if(console == null)
            {
                console = new ScriptConsole();
            }
            var configurator = new LoggerConfigurator(LogLevel);
            configurator.Configure(console);
            var logger = configurator.GetLogger();

            var scriptServicesBuilder = new ScriptServicesBuilder(console, logger)
                .InMemory(InMemory)
                .LogLevel(LogLevel)
                .ScriptName(ScriptName)
                .Repl(Repl);

            var modules = GetModuleList(Modules);
            var extension = Path.GetExtension(ScriptName);
            if(extension != null)
            {
                extension = extension.Substring(1);
            }
            scriptServicesBuilder.LoadModules(extension, modules);
            return scriptServicesBuilder.Build();
        }

        private static string[] GetModuleList(string modulesArg)
        {
            var modules = new string[0];
            if(modulesArg != null)
            {
                modules = modulesArg.Split(',');
            }
            return modules;
        }
    }
}
