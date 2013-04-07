using PowerArgs;

namespace ScriptCs
{
    [ArgExample("scriptcs server.csx -debug", "Shows how to start the script with debug mode switched on")]
    public class ScriptCsArgs
    {
        [ArgDescription("Script file name, must be specified first")]
        [ArgPosition(0)]
        public string ScriptName { get; set; }

        [ArgDescription("Flag which switches on debug mode")]
        [ArgShortcut("debug")]
        public bool DebugFlag { get; set; }

        [ArgDescription("Installs and restores packages which are specified in packages.config")]
        [ArgShortcut("install")]
        public string Install { get; set; }

        [ArgDescription("Restores installed packages, making them ready for using by the script")]
        [ArgShortcut("restore")]
        public bool Restore { get; set; }

        [ArgDescription("Creates a packages.config file based on the packages directory")]
        [ArgShortcut("save")]
        public bool Save { get; set; }

        [ArgDescription("Cleans installed packages from working directory")]
        [ArgShortcut("clean")]
        public bool Clean { get; set; }

        [ArgDescription("Allows installation of packages' prelease versions")]
        [ArgShortcut("pre")]
        public bool AllowPreReleaseFlag { get; set; }

        [ArgDescription("Outputs version information")]
        public bool Version { get; set; }
    }
}