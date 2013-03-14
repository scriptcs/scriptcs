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

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(ScriptName);
        }
    }
}