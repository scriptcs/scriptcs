namespace ScriptCs.Tests.Acceptance.Support
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    public static class ScriptCsExe
    {
        private static readonly bool isMono = Type.GetType("Mono.Runtime") != null;

        public static string BinFolder
        {
            get { return "scriptcs_bin"; }
        }

        public static string DllCacheFolder
        {
            get { return ".scriptcs_cache"; }
        }

        public static string PackagesFile
        {
            get { return "scriptcs_packages.config"; }
        }

        public static string PackagesFolder
        {
            get { return "scriptcs_packages"; }
        }

        public static string NugetFile
        {
            get { return "scriptcs_nuget.config"; }
        }

        public static string Run(IEnumerable<string> args, ScenarioDirectory directory)
        {
            return Run(null, true, args, Enumerable.Empty<string>(), directory);
        }

        public static string Run(string scriptName, ScenarioDirectory directory)
        {
            return Run(scriptName, true, Enumerable.Empty<string>(), Enumerable.Empty<string>(), directory);
        }

        public static string Run(string scriptName, bool debug, ScenarioDirectory directory)
        {
            return Run(scriptName, debug, Enumerable.Empty<string>(), Enumerable.Empty<string>(), directory);
        }

        public static string Run(string scriptName, bool debug, IEnumerable<string> args, ScenarioDirectory directory)
        {
            return Run(scriptName, debug, args, Enumerable.Empty<string>(), directory);
        }

        public static string Run(
            string scriptName,
            bool debug,
            IEnumerable<string> args,
            IEnumerable<string> scriptArgs,
            ScenarioDirectory directory)
        {
            var debugArgs =
                    debug &&
                    !args.Select(arg => arg.Trim().ToUpperInvariant()).Contains("-DEBUG") &&
                    !args.Select(arg => arg.Trim().ToUpperInvariant()).Contains("-D")
                ? new[] { "-debug" }
                : new string[0];

            return Execute(
                (scriptName == null ? Enumerable.Empty<string>() : new[] { scriptName }).Concat(debugArgs).Concat(args),
                scriptArgs,
                directory);
        }

        public static string Install(string package, ScenarioDirectory directory)
        {
            using (var writer = new StreamWriter(directory.Map(NugetFile), false))
            {
                writer.Write(
@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <clear />
    <add key=""Local"" value=""" + Path.GetFullPath(Path.Combine("Support", "Gallery")) + @""" />
  </packageSources>
  <activePackageSource>
    <add key=""All"" value=""(Aggregate source)"" />
  </activePackageSource>
</configuration>"
                    );

                writer.Flush();
            }

            return Execute(new[] { "-install", package }, Enumerable.Empty<string>(), directory);
        }

        public static string Save(ScenarioDirectory directory)
        {
            return Execute(new[] { "-save" }, Enumerable.Empty<string>(), directory);
        }

        public static string Clean(ScenarioDirectory directory)
        {
            return Execute(new[] { "-clean" }, Enumerable.Empty<string>(), directory);
        }

        private static string Execute(
            IEnumerable<string> args, IEnumerable<string> scriptArgs, ScenarioDirectory directory)
        {
            var commandArgs = new List<string>(args);
            var scriptArgsArray = scriptArgs.ToArray();
            if (scriptArgsArray.Any())
            {
                commandArgs.Add("--");
                commandArgs.AddRange(scriptArgsArray);
            }

#if DEBUG
            var config = "Debug";
#else
            var config = "Release";
#endif

            var exe = Path.GetFullPath(
                Path.Combine("..", "..", "..", "..", "src", "ScriptCs", "bin", config, "scriptcs.exe"));

            var info = new ProcessStartInfo
            {
                FileName = isMono
                    ? "mono"
                    : exe,
                Arguments = isMono
                    ? string.Concat(exe, " ", string.Join(" ", commandArgs))
                    : string.Join(" ", commandArgs),
                WorkingDirectory = directory.Name,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            var result = info.Run(Path.GetFileName(directory.Name) + ".log");
            if (result.Item1 != 0)
            {
                var message = string.Format(
                    CultureInfo.InvariantCulture,
                    "scriptcs.exe exited with code {0}. The output was: {1}",
                    result.Item1.ToString(CultureInfo.InvariantCulture),
                    result.Item2);

                throw new ScriptCsException(message);
            }

            return result.Item2;
        }
    }
}
