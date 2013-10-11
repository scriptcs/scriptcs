using PowerArgs;

using ScriptCs.Contracts;

namespace ScriptCs
{
    [ArgExample("scriptcs server.csx -logLevel debug", "Shows how to run the script and display detailed log messages. Useful for debugging.")]
    public class ScriptCsArgs
    {
        public ScriptCsArgs()
        {
            LogLevel = LogLevel.Info;
            Config = "scriptcs.opts";
        }

        [ArgShortcut("repl")]
        [ArgDescription("Launch REPL mode when running script. To just launch REPL, simply use 'scriptcs' without any args.")]
        public bool Repl { get; set; }

        [ArgPosition(0)]
        [ArgShortcut("script")]
        [ArgDescription("Script file name, must be specified first")]
        public string ScriptName { get; set; }

        [ArgShortcut("?")]
        [ArgDescription("Displays help")]
        public bool Help { get; set; }

        [ArgShortcut("inMemory")]
        [DefaultValue(true)]
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

        [ArgShortcut("modules")]
        [ArgDescription("Specify modules to load")]
        public string Modules { get; set; }

        [ArgShortcut("config")]
        [DefaultValue("scriptcs.opts")]
        [ArgDescription("Defines config file name")]
        public string Config { get; set; }

    }
}