using System;

namespace ScriptCs
{
    public static class StringExtensions
    {
        public static string DefineTrace(this string code)
        {
            return string.Format("#define TRACE{0}{1}", Environment.NewLine, code);
        }

        public static string UndefineTrace(this string code)
        {
            return string.Format("#undef TRACE{0}{1}", Environment.NewLine, code);
        }
    }
}