using ScriptCs.Contracts.Exceptions;

namespace ScriptCs.Contracts
{
    public abstract class DirectiveLineProcessor : IDirectiveLineProcessor
    {
        protected virtual BehaviorAfterCode BehaviorAfterCode
        {
            get { return BehaviorAfterCode.Ignore; }
        }

        protected abstract string DirectiveName { get; }

        private string DirectiveString
        {
            get { return string.Format("#{0}", DirectiveName); }
        }

        public bool ProcessLine(IFileParser parser, FileParserContext context, string line, bool isBeforeCode)
        {
            if (!Matches(line))
            {
                return false;
            }

            if (!isBeforeCode)
            {
                if (BehaviorAfterCode == Contracts.BehaviorAfterCode.Throw)
                {
                    throw new InvalidDirectiveUseException(string.Format("Encountered directive '{0}' after the start of code. Please move this directive to the beginning of the file.", DirectiveString));
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
                .Trim()
                .Replace("\"", string.Empty)
                .Replace(";", string.Empty);
        }

        protected abstract bool ProcessLine(IFileParser parser, FileParserContext context, string line);

        public bool Matches(string line)
        {
            Guard.AgainstNullArgument("line", line);

            var tokens = line.Split();
            return tokens[0] == DirectiveString;
        }
    }
}