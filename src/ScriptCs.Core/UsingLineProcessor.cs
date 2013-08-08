namespace ScriptCs
{
    public class UsingLineProcessor : ILineProcessor
    {
        private const string UsingString = "using ";

        public bool ProcessLine(IFileParser parser, FileParserContext context, string line, bool isBeforeCode)
        {
            if (!IsUsingLine(line)) return false;

            var @namespace = GetNamespace(line);
            if (!context.Namespaces.Contains(@namespace))
            {
                context.Namespaces.Add(@namespace);
            }

            return true;
        }

        private static bool IsUsingLine(string line)
        {
            return line.Trim(' ').StartsWith(UsingString) && !line.Contains("{") && line.Contains(";");
        }

        private static string GetNamespace(string line)
        {
            return line.Trim(' ')
                .Replace(UsingString, string.Empty)
                .Replace("\"", string.Empty)
                .Replace(";", string.Empty);
        }
    }
}