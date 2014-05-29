namespace ScriptCs.Tests.Acceptance.Support
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;

    public static class ScriptCsExe
    {
        public static string Execute(string[] args, string[] scriptArgs, string logFile, string workingDirectory)
        {
            var commandArgs = new List<string>(args);
            if (scriptArgs.Length > 0)
            {
                commandArgs.Add("--");
                commandArgs.AddRange(scriptArgs);
            }

#if DEBUG
            var config = "Debug";
#else
            var config = "Release";
#endif

#if __MonoCS__
            var exe = Path.Combine("..", "..", "..", "..", "..", "src", "ScriptCs", "bin", config, "scriptcs.exe");
#else
            var exe = Path.Combine("..", "..", "..", "..", "src", "ScriptCs", "bin", config, "scriptcs.exe");
#endif

            var info = new ProcessStartInfo
            {
#if __MonoCS__
                FileName = "mono",
                Arguments = string.Concat(exe, " ", string.Join(" ", commandArgs)),
#else
                FileName = exe,
                Arguments = string.Join(" ", commandArgs),
#endif
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
