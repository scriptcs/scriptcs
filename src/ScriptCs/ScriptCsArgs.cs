using System;
using System.Linq;

using PowerArgs;

using ScriptCs.Contracts;

namespace ScriptCs
{
    [ArgExample("scriptcs server.csx -inMemory", "Shows how to start the script running from memory (not compiling to a .dll)")]
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

<<<<<<< HEAD
        [ArgShortcut("modules")]
        [ArgDescription("Specify modules to load")]
        public string Modules { get; set; }

        public static void SplitScriptArgs(ref string[] args, out string[] scriptArgs)
        {
            Guard.AgainstNullArgument("args", args);

            // Split the arguments list on "--".
            // The arguments before the "--" (or all arguments if there is no "--") are
            // for ScriptCs.exe, and the arguments after that are for the csx script.
            int separatorLocation = Array.IndexOf(args, "--");
            int scriptArgsCount = separatorLocation == -1 ? 0 : args.Length - separatorLocation - 1;
            scriptArgs = new string[scriptArgsCount];
            Array.Copy(args, separatorLocation + 1, scriptArgs, 0, scriptArgsCount);
            if (separatorLocation != -1)
            {
                args = args.Take(separatorLocation).ToArray();
            }
        }
=======
        [ArgShortcut("config")]
        [DefaultValue("scriptcs.opts")]
        [ArgDescription("Defines config file name")]
        public string Config { get; set; }
>>>>>>> little refactoring
    }
}