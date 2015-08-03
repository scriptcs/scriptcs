using System;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;

namespace ScriptCs
{
    using ScriptCs.Contracts;

    public class ConfigMask
    {
        public bool? AllowPreRelease { get; set; }

        public bool? Cache { get; set; }

        public bool? Clean { get; set; }

        public bool? Debug { get; set; }

        public bool? Global { get; set; }

        public string Install { get; set; }

        public LogLevel? LogLevel { get; set; }

        public string Modules { get; set; }

        public string Output { get; set; }

        public string PackageVersion { get; set; }

        public bool? Repl { get; set; }

        public bool? Save { get; set; }

        public string ScriptName { get; set; }

        public bool? Watch { get; set; }

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

        public static ConfigMask ReadGlobalOrDefault()
        {
            return Read(new FileSystem().GlobalOptsFile, true);
        }

        public static ConfigMask ReadLocalOrDefault()
        {
            return Read(Constants.ConfigFilename, true);
        }

        public static ConfigMask Read(string path)
        {
            return Read(path, false);
        }

        private static ConfigMask Read(string path, bool defaultIfNotExists)
        {
            if (defaultIfNotExists && !File.Exists(path))
            {
                return null;
            }

            var json = File.ReadAllText(path);
            try
            {
                return JsonConvert.DeserializeObject<ConfigMask>(json);
            }
            catch (Exception ex)
            {
                var message = string.Format(
                    CultureInfo.InvariantCulture, "Error reading JSON config from '{0}'.", path);

                throw new InvalidOperationException(message, ex);
            }
        }
    }
}
