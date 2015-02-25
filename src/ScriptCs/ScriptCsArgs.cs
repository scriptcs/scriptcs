using System;
using PowerArgs;

using ScriptCs.Contracts;

namespace ScriptCs
{
    [Serializable]
    [ArgExample("scriptcs server.csx -logLevel debug", "Shows how to run the script and display detailed log messages. Useful for debugging.")]
    public class ScriptCsArgs
    {
        public ScriptCsArgs()
        {
            Config = Constants.ConfigFilename;
        }

        [ArgDescription("Launch REPL mode when running script. To just launch REPL, simply use 'scriptcs' without any args.")]
        public bool Repl { get; set; }

        [ArgPosition(0)]
        [ArgShortcut("script")]
        [ArgDescription("Script file name, must be specified first")]
        public string ScriptName { get; set; }

        [ArgShortcut("?")]
        [ArgDescription("Displays help")]
        public bool Help { get; set; }

        [DefaultValue(false)]
        [ArgDescription("Emits PDB symbols allowing for attaching a Visual Studio debugger")]
        public bool Debug { get; set; }

        [DefaultValue(false)]
        [ArgDescription("Flag which determines whether to run in memory or from a .dll")]
        public bool Cache { get; set; }

        [ArgIgnoreCase]
        [ArgShortcut("log")]
        [ArgDescription("Flag which defines the log level used.")]
        public LogLevel? LogLevel { get; set; }

        [ArgDescription("Installs and restores packages which are specified in packages.config")]
        public string Install { get; set; }

        [ArgShortcut("g")]
        [ArgDescription("Installs and restores global packages which are specified in packages.config")]
        public bool Global { get; set; }

        [ArgDescription("Creates a packages.config file based on the packages directory")]
        public bool Save { get; set; }

        [ArgDescription("Cleans installed packages from working directory")]
        public bool Clean { get; set; }

        [ArgShortcut("pre")]
        [ArgDescription("Allows installation of packages' prelease versions")]
        public bool AllowPreRelease { get; set; }

        [ArgShortcut("rsl")]
        [ArgDescription("Refresh the cache of script libraries")]
        public bool RefreshScriptLibraries { get; set; }

        [ArgDescription("Outputs version information")]
        public bool Version { get; set; }

        [ArgDescription("Watch the script file and reload it when changed")]
        public bool Watch { get; set; }

        [ArgDescription("Specify modules to load")]
        public string Modules { get; set; }

        [DefaultValue(Constants.ConfigFilename)]
        [ArgDescription("Defines config file name")]
        public string Config { get; set; }

        [ArgDescription("Defines the version of the package to install. Used in conjunction with -install")]
        public string PackageVersion { get; set; }

        [ArgDescription("Write all console output to the specified file")]
        public string Output { get; set; }
    }
}