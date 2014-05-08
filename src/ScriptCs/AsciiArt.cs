using System;
using System.Linq;
using System.Text.RegularExpressions;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public static class AsciiArt
    {
        public static void WriteAsciiArt(this IConsole console, string version)
        {
            Guard.AgainstNullArgument("console", console);

            var lines = new[]
            {
                @"+cyan               _       _",
                @"+cyan ___  ___ _ __(_)_ __ | |__+darkMagenta ___ ___",
                @"+cyan/ __|/ __| '__| | '_ \| __/+darkMagenta/ __/ __|",
                @"+cyan\__ \ (__| |  | | |_) | |_+darkMagenta| (__\__ \",
                @"+cyan|___/\___|_|  |_| .__/ \__\+darkMagenta\___|___/",
                string.Format(@"+cyan                |_|+white Version: {0}", version)
            };

            var colorRegex = new Regex(@"\+(?<color>\w*)(?<ascii>(.*(?=\+))|.*)",
                RegexOptions.Compiled | RegexOptions.Singleline);

            foreach (var matches in lines.Select(line => colorRegex.Matches(line))) 
            {
                foreach (Match match in matches)
                {
                    ConsoleColor color;
                    if (Enum.TryParse(match.Groups["color"].Value, true, out color))
                    {
                        console.ForegroundColor = color;
                    }

                    console.Write(match.Groups["ascii"].Value);

                    console.ResetColor();
                }

                console.WriteLine();
            }

            console.WriteLine();
        }
    }
}