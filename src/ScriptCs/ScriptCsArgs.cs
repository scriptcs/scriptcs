using System;
using System.Linq;

using PowerArgs;

namespace ScriptCs
{
    [ArgExample("scriptcs server.csx -debug", "Shows how to start the script with debug mode switched on")]
    public class ScriptCsArgs
    {
        [ArgIgnore]
        public bool Repl { get; set; }

        [ArgPosition(0)]
        [ArgDescription("Script file name, must be specified first")]
        public string ScriptName { get; set; }

        [ArgDescription("Displays help")]
        
        [ArgShortcut("?")]
        public bool Help { get; set; }

        [ArgShortcut("debug")]
        [ArgDescription("Flag which switches on debug mode")]
        public bool Debug { get; set; }

        [ArgIgnoreCase]
        [ArgShortcut("log")]
        [DefaultValue(LogLevel.Info)]
        [ArgDescription("Flag which defines the log level used.")]
        public LogLevel LogLevel { get; set; }

        [ArgShortcut("install")]
        [ArgDescription("Installs and restores packages which are specified in packages.config")]
        public string Install { get; set; }

        [ArgShortcut("restore")]
        [ArgDescription("Restores installed packages, making them ready for using by the script")]
        public bool Restore { get; set; }

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

        public static void SplitScriptArgs(ref string[] args, out string[] scriptArgs)
        {
            // Split the arguments list on "--".
            // The arguments before the "--" (or all arguments if there is no "--") are
            // for ScriptCs.exe, and the arguments after that are for the csx script.
            int separatorLocation = Array.IndexOf(args, "--");
            int scriptArgsCount = separatorLocation == -1 ? 0 : args.Length - separatorLocation - 1;
            scriptArgs = new string[scriptArgsCount];
            Array.Copy(args, separatorLocation + 1, scriptArgs, 0, scriptArgsCount);
            if (separatorLocation != -1) args = args.Take(separatorLocation).ToArray();
        }
    }
}