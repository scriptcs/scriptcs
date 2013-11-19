using System;
using ScriptCs.Contracts.Exceptions;
namespace ScriptCs.Contracts
{
    public abstract class DirectiveLineProcessor : ILineProcessor
    {
        protected virtual BehaviorAfterCode BehaviorAfterCode
        {
            get { return BehaviorAfterCode.Ignore; }
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

            if (!isBeforeCode)
            {
                if (BehaviorAfterCode == Contracts.BehaviorAfterCode.Throw)
                {
                    throw new InvalidDirectiveUseException(string.Format("Encountered {0}directive after the start of code. Please move this directive to the beginning of the file.", DirectiveString));
                }
                else if (BehaviorAfterCode == Contracts.BehaviorAfterCode.Ignore)
                {
                    return true;
                }
             }

            return ProcessLine(parser, context, line);
        }

        protected string GetDirectiveArgument(string line)
        {
            Guard.AgainstNullArgument("line", line);

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