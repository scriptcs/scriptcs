using System.Linq;
using System.Text;
using ScriptCs.Contracts;

namespace ScriptCs.ReplCommands
{
    public class HelpCommand : IReplCommand
    {
        private readonly IConsole _console;

        public HelpCommand(IConsole console)
        {
            Guard.AgainstNullArgument("console", console);

            _console = console;
        }

        public string Description
        {
            get { return "Shows this help."; }
        }

        public string CommandName
        {
            get { return "help"; }
        }

        public object Execute(IRepl repl, object[] args)
        {
            Guard.AgainstNullArgument("repl", repl);

            _console.WriteLine("The following commands are available in the REPL:");
            foreach (var command in repl.Commands.OrderBy(x => x.Key))
            {
                string key = string.Format(" :{0,-15} - ", command.Key);
                string desc = WrapTextToColumn(command.Value.Description, _console.Width - key.Length - 1, indentWidth: key.Length);

                _console.WriteLine(string.Format("{0}{1,10}", key, desc));
            }

            return null;
        }

        /// <summary>
        /// Word wrap text to specified column width.
        /// </summary>
        /// <param name="text">Unformatted text.</param>
        /// <param name="columnWidth">Size of the column width.</param>
        /// <param name="indentWidth">Indentation width when the text is wrap. The first line is not indented.</param>
        /// <param name="initialWidth">First line indent width.</param>
        /// <returns>Formatted text.</returns>
        /// <remarks>In the future, I believe this method will be moved into some sort of formatting helper class.</remarks>
        private string WrapTextToColumn(string text, int columnWidth, int indentWidth = 0, int initialWidth = 0)
        {
            // check the initial width
            if ((initialWidth < 0) || (initialWidth > (indentWidth + columnWidth)))
            {
                throw new System.ArgumentOutOfRangeException("initialWidth");
            }
            
            // TODO: Add additional parameter error checking
                        
            StringBuilder paragraph = new StringBuilder(text.Trim());

            // add the initial space to text
            paragraph.Insert(0, " ", initialWidth);

            if (paragraph.Length > (columnWidth))
            {
                int pos = columnWidth;
                int backSearchLimit = initialWidth;
                do
                {
                    // find a whitespace we can wrap the description line
                    int savedPos = pos;
                    while (!char.IsWhiteSpace(paragraph[pos]))
                    {
                        pos--;

                        // guard against not finding a natural whitespace
                        // don't go below the spaces we create (indent)
                        if (pos < backSearchLimit)
                        {
                            pos = savedPos;
                            break;
                        }
                    }

                    if (char.IsWhiteSpace(paragraph[pos]))
                    {
                        paragraph.Remove(pos, 1);    // remove the whitespace we found
                    }
                    // inject a newline   
                    paragraph.Insert(pos, System.Environment.NewLine);
                    pos += System.Environment.NewLine.Length;
                    paragraph.Insert(pos, " ", indentWidth);
                    pos += indentWidth;

                    // prevent searching for whitespace to go below the spaces we put in
                    backSearchLimit = pos; 

                    pos += columnWidth;
                } while (pos < paragraph.Length);

            }

            return paragraph.ToString();
        }
    }
}