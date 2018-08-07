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

        public string Eval { get; set; }

        public bool? Watch { get; set; }

        public static ConfigMask ReadGlobalOrDefault() => Read(new FileSystem().GlobalOptsFile, true);

        public static ConfigMask ReadLocalOrDefault() => Read(Constants.ConfigFilename, true);

        public static ConfigMask Read(string path) => Read(path, false);

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
