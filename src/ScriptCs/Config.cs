using System;
using ScriptCs.Contracts;
using System.IO;

namespace ScriptCs
{
    // NOTE (Adam): passed across app domains as a property of CrossAppDomainExecuteScriptCommand
    [Serializable]
    public class Config
    {
        public Config()
        {
            LogLevel = LogLevel.Info;
        }

        // global
        public LogLevel LogLevel { get; set; }

        public string Modules { get; set; }

        public string Output { get; set; }

        // clean command
        public bool Clean { get; set; }

        // install command
        public bool AllowPreRelease { get; set; }

        public bool Global { get; set; }

        public string Install { get; set; }

        public string PackageVersion { get; set; }

        // save command
        public bool Save { get; set; }

        // run command
        public string ScriptName { get; set; }

        public bool Cache { get; set; }

        public bool Debug { get; set; }

        public bool Repl { get; set; }

        public bool Watch { get; set; }

        public Config Apply(ConfigMask mask)
        {
            if (mask == null)
            {
                return this;
            }

            var logLevel = mask.Debug.GetValueOrDefault() && !mask.LogLevel.HasValue && LogLevel != LogLevel.Trace
                ? LogLevel.Debug
                : mask.LogLevel;

            var scriptName = mask.ScriptName != null && !Path.GetFileName(mask.ScriptName).Contains(".")
                ? Path.ChangeExtension(mask.ScriptName, "csx")
                : mask.ScriptName;

            return new Config
            {
                LogLevel = logLevel ?? LogLevel,
                Modules = mask.Modules ?? Modules,
                Output = mask.Output ?? Output,
                Clean = mask.Clean ?? Clean,
                AllowPreRelease = mask.AllowPreRelease ?? AllowPreRelease,
                Global = mask.Global ?? Global,
                Install = mask.Install ?? Install,
                PackageVersion = mask.PackageVersion ?? PackageVersion,
                Save = mask.Save ?? Save,
                Cache = mask.Cache ?? Cache,
                Debug = mask.Debug ?? Debug,
                Repl = mask.Repl ?? Repl,
                ScriptName = scriptName ?? ScriptName,
                Watch = mask.Watch ?? Watch,
            };
        }
    }
}
