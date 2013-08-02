using System;
using System.Text.RegularExpressions;

namespace ScriptCs
{
    public static class PreProcessorUtil
    {
        public const string LoadString = "#load ";
        public const string UsingString = "using ";
        public const string RString = "#r ";
        private static Regex EnvironmentRegex = new Regex("%.+?%");

        public static bool IsNonDirectiveLine(string line)
        {
            return !IsRLine(line) && !IsLoadLine(line) && line.Trim() != string.Empty;
        }

        public static bool IsUsingLine(string line)
        {
            Guard.AgainstNullArgument("line", line);

            return line.TrimStart(' ').StartsWith(UsingString) && !line.Contains("{") && line.Contains(";");
        }

        public static bool IsRLine(string line)
        {
            Guard.AgainstNullArgument("line", line);

            return line.TrimStart(' ').StartsWith(RString);
        }

        public static bool IsLoadLine(string line)
        {
            Guard.AgainstNullArgument("line", line);

            return line.TrimStart(' ').StartsWith(LoadString);
        }

        public static string GetPath(string replaceString, string line)
        {
            Guard.AgainstNullArgument("line", line);

            var trimedFilePath = line.Replace(replaceString, string.Empty).Trim(' ').Replace("\"", string.Empty).Replace(";", string.Empty);
            var filepath = Environment.ExpandEnvironmentVariables(trimedFilePath);
            return filepath;
        }
    }
}