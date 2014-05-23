namespace ScriptCs.Tests.Acceptance.Support
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;

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

            var info = new ProcessStartInfo
            {
#if DEBUG
                FileName = @"..\..\..\..\src\ScriptCs\bin\Debug\scriptcs.exe",
#else
                FileName = @"..\..\..\..\src\ScriptCs\bin\Release\scriptcs.exe",
#endif
                Arguments = string.Join(" ", commandArgs),
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
