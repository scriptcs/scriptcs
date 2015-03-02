using System;
using System.Collections.Generic;
using System.Linq;
using PowerArgs;
using ScriptCs.Contracts;

namespace ScriptCs
{
    // NOTE (Adam): passed across app domains as a property of CrossAppDomainExecuteScriptCommand 
    [Serializable]
    [ArgExample("scriptcs server.csx -logLevel debug", "Shows how to run the script and display detailed log messages. Useful for debugging.")]
    public class ScriptCsArgs
    {
        [ArgDescription("Launch REPL mode when running script. To just launch REPL, simply omit the 'script' argument.")]
        public bool Repl { get; set; }

        [ArgPosition(0)]
        [ArgShortcut("script")]
        [ArgDescription("Script file name, must be specified first")]
        public string ScriptName { get; set; }

        [ArgShortcut("?")]
        [ArgDescription("Displays help")]
        public bool Help { get; set; }

        [ArgDescription("Emits PDB symbols allowing for attaching a Visual Studio debugger")]
        public bool Debug { get; set; }

        [ArgDescription("Flag which determines whether to run in memory or from a .dll")]
        public bool Cache { get; set; }

        [ArgShortcut("log")]
        [ArgDescription("Flag which defines the log level used.")]
        public LogLevel? LogLevel { get; set; }

        [ArgDescription("Installs and restores packages which are specified in packages.config")]
        [ArgShortcut("i")]
        public string Install { get; set; }

        [ArgShortcut("g")]
        [ArgDescription("Installs and restores global packages which are specified in packages.config")]
        public bool Global { get; set; }

        [ArgDescription("Creates a packages.config file based on the packages directory")]
        public bool Save { get; set; }

        [ArgDescription("Cleans installed packages from working directory")]
        public bool Clean { get; set; }

        [ArgShortcut("pre")]
        [ArgDescription("Allows installation of packages' prelease versions")]
        public bool AllowPreRelease { get; set; }

        [ArgDescription("Outputs version information")]
        public bool Version { get; set; }

        [ArgDescription("Watch the script file and reload it when changed")]
        public bool Watch { get; set; }

        [ArgDescription("Specify modules to load")]
        public string Modules { get; set; }

        [DefaultValue(Constants.ConfigFilename)]
        [ArgDescription("Defines config file name")]
        public string Config { get; set; }

        [ArgDescription("Defines the version of the package to install. Used in conjunction with -install")]
        public string PackageVersion { get; set; }

        [ArgDescription("Write all console output to the specified file")]
        public string Output { get; set; }

        public static ScriptCsArgs Parse(string[] args)
        {
            Guard.AgainstNullArgument("args", args);

            var list = args.ToList();
            var curatedList = new List<string>();
            string packageVersion = null;
            for (var index = 0; index < list.Count; ++index)
            {
                if (index < list.Count - 2 &&
                    (string.Equals(list[index], "-install", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(list[index], "-i", StringComparison.OrdinalIgnoreCase)) &&
                    !list[index + 1].StartsWith("-", StringComparison.Ordinal) &&
                    !list[index + 2].StartsWith("-", StringComparison.Ordinal))
                {
                    curatedList.Add(list[index]);
                    curatedList.Add(list[index + 1]);
                    packageVersion = list[index + 2];
                    index += 2;
                }
                else
                {
                    curatedList.Add(list[index]);
                }
            }

            var scriptCsArgs = Args.Parse<ScriptCsArgs>(curatedList.ToArray());
            if (!string.IsNullOrWhiteSpace(packageVersion))
            {
                scriptCsArgs.PackageVersion = packageVersion;
            }

            return scriptCsArgs;
        }

        public static string GetUsage()
        {
            return ArgUsage.GetUsage<ScriptCsArgs>(
                null, new ArgUsageOptions { ShowPosition = false, ShowType = false, });
        }
    }
}
