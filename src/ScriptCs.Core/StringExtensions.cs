using System;
using System.Collections.Generic;

namespace ScriptCs
{
    public static class StringExtensions
    {
        public static string DefineTrace(this string code)
        {
            return string.Format("#define TRACE{0}{1}", Environment.NewLine, code);
        }

        /// <summary>
        /// Split string on whitespace, but keeps string with quotes together
        /// For example: :cd "\\Foo Bar"
        ///             :cd
        ///             "\\Foo Bar".
        /// </summary>
        /// <param name="argument">String with or without quotes.</param>
        /// <returns>Array of strings.</returns>
        public static string[] SplitQuoted(this string argument)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                return argument.Split(' ');
            }

            // count the number of quotes and throw something is not even
            // the fastest way is to just loop thru the string
            // http://cc.davelozinski.com/c-sharp/fastest-way-to-check-if-a-string-occurs-within-a-string
            Func<string, int> quoteCounterFunc = delegate (string line)
            {
                int count = 0;
                for (int x = 0; x < line.Length; x++)
                {
                    if (line[x] == '"')
                    {
                        count++;
                    }
                }
                return count;
            };
            int quotes = quoteCounterFunc(argument);
            if ((quotes % 2) != 0)
            {
                throw new ArgumentException("String is missing a closing quote");
            }

            List<string> list = new List<string>(argument.Split(' '));

            // quoted string needs to be combine back together
            if (quotes > 0 && list.Count > 0)
            {
                Predicate<string> findQuoteFunc = delegate (string s) { return s.Contains("\""); };
                // create function to find string item with odd number of quotes
                Func<int, int> findOddQuotedItemFunc = delegate (int startingIndex) {
                    if (startingIndex < list.Count)
                    {
                        do
                        {
                            int quickFind = list.FindIndex(startingIndex, findQuoteFunc);
                            int quickCount = quoteCounterFunc(list[quickFind]);
                            if ((quickCount % 2) != 0)
                            {
                                return quickFind;
                            }
                            // we didn't find the quoted line we are looking for
                            startingIndex = quickFind + 1;
                        } while (startingIndex < list.Count);
                    }
                    return -1;
                };

                int index = 0;
                do
                {
                    int start = findOddQuotedItemFunc(index);
                    if (start > 0)
                    {
                        // we have to locate the next string with odd number of quotes
                        int end = findOddQuotedItemFunc(start + 1);

                        string combined = string.Empty;
                        for (int x = start; x <= end; x++)
                        {
                            // because we split on whitespace, we have to put it back when combining
                            combined += list[x] + ' ';
                        }
                        list[start] = combined.TrimEnd(); // remove the extra whitespace that was added

                        // removed the other parts of the combined string from the list
                        do
                        {
                            list.RemoveAt(end--); // from the bottom up
                        } while (start < end);

                        // advance to next item in the adjusted list
                        index = start + 1;
                    }
                    else
                    {
                        break;
                    }

                } while (index < list.Count);
            }

            return list.ToArray();
        }

        public static string UndefineTrace(this string code)
        {
            return string.Format("#undef TRACE{0}{1}", Environment.NewLine, code);
        }
    }
}