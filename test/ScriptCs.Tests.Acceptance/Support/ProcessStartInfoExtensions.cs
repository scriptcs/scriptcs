namespace ScriptCs.Tests.Acceptance.Support
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    public static class ProcessStartInfoExtensions
    {
        public static string Run(this ProcessStartInfo info, string logfile)
        {
            var output = new StringBuilder();
            using (var process = new Process())
            {
                process.StartInfo = info;
                process.OutputDataReceived += (sender, e) => output.AppendLine(e.Data);
                process.ErrorDataReceived += (sender, e) => output.AppendLine(e.Data);
                process.Start();
                process.BeginOutputReadLine();
                process.WaitForExit();

                using (var writer = new StreamWriter(logfile, true))
                {
                    writer.WriteLine(output.ToString());
                    writer.Flush();
                }

                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException(output.ToString());
                }
            }

            return output.ToString();
        }
    }
}
