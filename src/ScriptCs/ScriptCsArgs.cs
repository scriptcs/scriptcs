using PowerArgs;

namespace ScriptCs
{
    public class ScriptCsArgs
    {
        [ArgDescription("Script file name")]
        [ArgPosition(0)]
        public string ScriptName { get; set; }

        [ArgDescription("Flag which switches on debug mode")]
        [ArgShortcut("debug")]
        public bool DebugFlag { get; set; }

        [ArgShortcut("install")]
        public string Install { get; set; }

        [ArgShortcut("pre")]
        public bool AllowPreReleaseFlag { get; set; }

        [ArgDescription("Outputs version information")]
        public bool Version { get; set; }


        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(ScriptName) || Install != null;
        }
    }
}