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
        private const string ValidLogLevels =
            "off, debug, emergency, fatal, alert, critical, severe, error, warn, notice, info, debug, fine, trace, verbose";

        private string _logLevel;

        [ArgDescription("Script file name")]
        [ArgPosition(0)]
        public string ScriptName { get; set; }

        [ArgDescription("Flag which switches on debug mode")]
        [ArgShortcut("debug")]
        public bool DebugFlag { get; set; }

        [ArgDescription("Flag which defines the log level used. Possible values:" + ValidLogLevels)]
        [ArgShortcut("log")]
        [DefaultValue("off")]
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

        [ArgShortcut("pre")]
        public bool AllowPreReleaseFlag { get; set; }

        public bool IsValid()
        {
            return (!string.IsNullOrWhiteSpace(ScriptName) || Install != null) && this.IsLogLevelValid();
        }

        private bool IsLogLevelValid()
        {
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