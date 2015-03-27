using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace ScriptCs
{
    public static class VersionWriter
    {
        private static readonly string version =
            FileVersionInfo.GetVersionInfo(typeof(VersionWriter).Assembly.Location).ProductVersion;

        private static readonly Regex colorRegex = new Regex(
            @"\+(?<color>\w*)(?<ascii>(.*(?=\+))|.*)", RegexOptions.Compiled | RegexOptions.Singleline);

        public static void Write()
        {
            var lines = new[]
            {
                @"+cyan               _       _",
                @"+cyan ___  ___ _ __(_)_ __ | |__+darkMagenta ___ ___",
                @"+cyan/ __|/ __| '__| | '_ \| __/+darkMagenta/ __/ __|",
                @"+cyan\__ \ (__| |  | | |_) | |_+darkMagenta| (__\__ \",
                @"+cyan|___/\___|_|  |_| .__/ \__\+darkMagenta\___|___/",
                string.Format(@"+cyan                |_|+white Version: {0}", version)
            };

            foreach (var lineMatches in lines.Select(line => colorRegex.Matches(line)))
            {
                foreach (Match match in lineMatches)
                {
                    ConsoleColor color;
                    if (Enum.TryParse(match.Groups["color"].Value, true, out color))
                    {
                        Console.ForegroundColor = color;
                    }

                    try
                    {
                        Console.Write(match.Groups["ascii"].Value);
                    }
                    finally
                    {
                        Console.ResetColor();
                    }
                }

                Console.WriteLine();
            }

            Console.WriteLine();
        }
    }
}
