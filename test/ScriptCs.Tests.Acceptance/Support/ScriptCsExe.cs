namespace ScriptCs.Tests.Acceptance.Support
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    public static class ScriptCsExe
    {
        private static readonly bool isMono = Type.GetType("Mono.Runtime") != null;

        public static string Execute(string arg, string logFile, string workingDirectory)
        {
            return Execute(new[] { arg }, Enumerable.Empty<string>(), logFile, workingDirectory);
        }

        public static string Execute(IEnumerable<string> args, string logFile, string workingDirectory)
        {
            return Execute(args, Enumerable.Empty<string>(), logFile, workingDirectory);
        }

        public static string Execute(
            IEnumerable<string> args, IEnumerable<string> scriptArgs, string logFile, string workingDirectory)
        {
            var commandArgs = new List<string>(args);
            if (scriptArgs.Count() > 0)
            {
                commandArgs.Add("--");
                commandArgs.AddRange(scriptArgs);
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
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            return info.Run(logFile);
        }
    }
}
