using System;
using ScriptCs.Contracts;
using System.IO;

namespace ScriptCs
{
    // NOTE (Adam): passed across app domains as a property of CrossAppDomainExecuteScriptCommand
    [Serializable]
    public class Config
    {
        private string[] _modules;

        public Config()
        {
            LogLevel = LogLevel.Info;
        }

        // global
        public LogLevel LogLevel { get; set; }

        public string[] Modules
        {   
            get { return _modules ?? new string[0]; }
            set { _modules = value; }
        }

        public string OutputFile { get; set; }

        // clean command
        public bool Clean { get; set; }

        // install command
        public bool AllowPreRelease { get; set; }

        public bool Global { get; set; }

        public string PackageName { get; set; }

        public string PackageVersion { get; set; }

        // save command
        public bool Save { get; set; }

        // run command
        public string ScriptName { get; set; }

        public bool Cache { get; set; }

        public bool Debug { get; set; }

        public bool Repl { get; set; }

        public bool Watch { get; set; }

        public static Config Create(ScriptCsArgs commandArgs)
        {
            Guard.AgainstNullArgument("commandArgs", commandArgs);

            return new Config()
                .Apply(ConfigMask.ReadGlobalOrDefault())
                .Apply(commandArgs.Config == null ? ConfigMask.ReadLocalOrDefault() : ConfigMask.Read(commandArgs.Config))
                .Apply(ConfigMask.Create(commandArgs));
        }

        public Config Apply(ConfigMask mask)
        {
            if (mask == null)
            {
                return this;
            }

            var logLevel = mask.Debug.GetValueOrDefault() && !mask.LogLevel.HasValue && LogLevel != LogLevel.Trace
                ? LogLevel.Debug
                : mask.LogLevel;

            var modules = mask.Modules == null
                ? null
                : mask.Modules.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);

            var scriptName = mask.ScriptName != null && !Path.GetFileName(mask.ScriptName).Contains(".")
                ? Path.ChangeExtension(mask.ScriptName, "csx")
                : mask.ScriptName;

            return new Config
            {
                LogLevel = logLevel ?? LogLevel,
                _modules = modules ?? _modules,
                OutputFile = mask.Output ?? OutputFile,
                Clean = mask.Clean ?? Clean,
                AllowPreRelease = mask.AllowPreRelease ?? AllowPreRelease,
                Global = mask.Global ?? Global,
                PackageName = mask.Install ?? PackageName,
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
