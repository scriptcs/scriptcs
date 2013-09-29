using System;
namespace ScriptCs.Contracts
{
    public abstract class DirectiveLineProcessor : ILineProcessor
    {
        protected virtual bool ThrowIfAfterCode
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

            if (!isBeforeCode && ThrowIfAfterCode)
            {
                throw new Exception(string.Format("Encountered {0}directive after the start of code. Please move this directive to the beginning of the file.", DirectiveString)); 
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