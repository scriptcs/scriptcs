using System;
using System.Linq;
using log4net;
using PowerArgs;

namespace ScriptCs
{
    using System.Globalization;

    using log4net.Core;

    public class ScriptCsArgs
    {
        private const string ValidLogLevels = "error, info, debug, trace";

        private string _logLevel;

        [ArgDescription("Script file name")]
        [ArgPosition(0)]
        public string ScriptName { get; set; }

        [ArgDescription("Flag which switches on debug mode")]
        [ArgShortcut("debug")]
        public bool DebugFlag { get; set; }

        [ArgDescription("Flag which defines the log level used. Possible values:" + ValidLogLevels)]
        [ArgShortcut("log")]
        [DefaultValue("info")]
        public string LogLevel 
        {
            get
            {
                return _logLevel;
            }
            set
            {
                _logLevel = value.ToUpper(CultureInfo.CurrentUICulture);
            }
        }

        [ArgShortcut("install")]
        public string Install { get; set; }

        [ArgShortcut("restore")]
        public bool Restore { get; set; }

        [ArgShortcut("clean")]
        public bool Clean { get; set; }

        [ArgShortcut("pre")]
        public bool AllowPreReleaseFlag { get; set; }

        [ArgDescription("Outputs version information")]
        public bool Version { get; set; }


        public bool IsValid()
        {
            return (!string.IsNullOrWhiteSpace(ScriptName) || Install != null) && this.IsLogLevelValid();
        }

        private bool IsLogLevelValid()
        {
            if (!ValidLogLevels.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Contains(LogLevel))
            {
                return false;
            }

            var repository = LogManager.GetRepository();
            var levelMap = repository.LevelMap;
            return levelMap
                .AllLevels
                .Cast<Level>()
                .Any(level =>
                    level.Name.Equals(LogLevel, StringComparison.CurrentCulture));
        }
    }
}