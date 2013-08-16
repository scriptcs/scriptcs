using System;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public abstract class DirectiveLineProcessor : ILineProcessor
    {
        private readonly IFileSystem _fileSystem;

        public DirectiveLineProcessor(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public string GetArgumentFullPath(string line)
        {
            var argument = GetDirectiveArgument(line);
            var assemblyPath = Environment.ExpandEnvironmentVariables(argument);

            var referencePath = _fileSystem.GetFullPath(assemblyPath);
            return referencePath;
        }

        protected virtual bool IgnoreAfterCode
        {
            get { return false; }
        }

        protected abstract string DirectiveName { get; }

        private string DirectiveString
        {
            get { return string.Format("#{0} ", DirectiveName); }
        }
        
        public bool ProcessLine(IFileParser parser, FileParserContext context, string line, bool isBeforeCode)
        {
            if (!IsDirective(line))
            {
                return false;
            }

            if (!isBeforeCode && IgnoreAfterCode)
            {
                return true;
            }

            return ProcessLine(parser, context, line);
        }

        protected string GetDirectiveArgument(string line)
        {
            return line.Replace(DirectiveString, string.Empty)
                .Trim(' ')
                .Replace("\"", string.Empty)
                .Replace(";", string.Empty);
        }

        protected abstract bool ProcessLine(IFileParser parser, FileParserContext context, string line);

        private bool IsDirective(string line)
        {
            return line.Trim(' ').StartsWith(DirectiveString);
        }
    }
}