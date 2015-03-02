using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ScriptCs.Contracts;

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

        public bool Repl { get; set; }

        public string ScriptName { get; set; }

        public bool Debug { get; set; }

        public bool Cache { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public LogLevel LogLevel { get; set; }

        public string Install { get; set; }

        public bool Global { get; set; }

        public bool Save { get; set; }

        public bool Clean { get; set; }

        public bool AllowPreRelease { get; set; }

        public bool Watch { get; set; }

        public string Modules { get; set; }

        public string PackageVersion { get; set; }

        public string Output { get; set; }

        public Config Apply(ConfigMask mask)
        {
            if (mask == null)
            {
                return this;
            }

            var scriptName = mask.ScriptName ?? ScriptName;
            if (scriptName != null && !Path.HasExtension(scriptName))
            {
                scriptName += ".csx";
            }

            return new Config
            {
                AllowPreRelease = mask.AllowPreRelease ?? AllowPreRelease,
                Cache = mask.Cache ?? Cache,
                Clean = mask.Clean ?? Clean,
                Debug = mask.Debug ?? Debug,
                Global = mask.Global ?? Global,
                Install = mask.Install ?? Install,
                LogLevel = mask.Debug.HasValue && mask.Debug.Value ? LogLevel.Debug : mask.LogLevel ?? LogLevel,
                Modules = mask.Modules ?? Modules,
                Output = mask.Output ?? Output,
                PackageVersion = mask.PackageVersion ?? PackageVersion,
                Repl = mask.Repl ?? Repl,
                Save = mask.Save ?? Save,
                ScriptName = scriptName,
                Watch = mask.Watch ?? Watch,
            };
        }
    }
}
