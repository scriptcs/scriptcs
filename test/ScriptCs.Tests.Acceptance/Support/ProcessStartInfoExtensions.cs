namespace ScriptCs.Tests.Acceptance.Support
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
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
                process.BeginErrorReadLine();
                process.WaitForExit();

                using (var writer = new StreamWriter(logfile, true))
                {
                    writer.WriteLine(output.ToString());
                    writer.Flush();
                }

                if (process.ExitCode != 0)
                {
                    var message = string.Format(
                        CultureInfo.InvariantCulture,
                        "The process exited with code {0}. The output was: {1}",
                        process.ExitCode.ToString(CultureInfo.InvariantCulture),
                        output.ToString());

                    throw new InvalidOperationException(message);
                }
            }

            return output.ToString();
        }
    }
}
