using System.IO;
using Newtonsoft.Json;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ConfigMask
    {
        private ConfigMask()
        {
        }

        public bool? Repl { get; set; }

        public string ScriptName { get; set; }

        public bool? Debug { get; set; }

        public bool? Cache { get; set; }

        public LogLevel? LogLevel { get; set; }

        public string Install { get; set; }

        public bool? Global { get; set; }

        public bool? Save { get; set; }

        public bool? Clean { get; set; }

        public bool? AllowPreRelease { get; set; }

        public bool? Watch { get; set; }

        public string Modules { get; set; }

        public string PackageVersion { get; set; }

        public string Output { get; set; }

        public static ConfigMask Create(ScriptCsArgs args)
        {
            Guard.AgainstNullArgument("args", args);

            return new ConfigMask
            {
                AllowPreRelease = args.AllowPreRelease ? (bool?)true : null,
                Cache = args.Cache ? (bool?)true : null,
                Clean = args.Clean ? (bool?)true : null,
                Debug = args.Debug ? (bool?)true : null,
                Global = args.Global ? (bool?)true : null,
                Install = args.Install,
                LogLevel = args.LogLevel,
                Modules = args.Modules,
                Output = args.Output,
                PackageVersion = args.PackageVersion,
                Repl = args.Repl ? (bool?)true : null,
                Save = args.Save ? (bool?)true : null,
                ScriptName = args.ScriptName,
                Watch = args.Watch ? (bool?)true : null,
            };
        }

        public static ConfigMask ReadOrDefault(string path)
        {
            return File.Exists(path) ? JsonConvert.DeserializeObject<ConfigMask>(File.ReadAllText(path)) : null;
        }
    }
}
