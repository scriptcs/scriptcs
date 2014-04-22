using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;

namespace ScriptCs.SyntaxTreeParser
{
    public class SyntaxParser
    {
        public ParseResult Parse(string code)
        {
            var result = new ParseResult();

            var parser = new CSharpParser();
            var syntaxTree = parser.Parse(code);
            var codeLines = code.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            var codeLinesDictionary = new Dictionary<int, Tuple<string, bool>>();
            for (int i = 0; i < codeLines.Length; i++)
            {
                codeLinesDictionary.Add(i, new Tuple<string, bool>(codeLines[i], true));
            }

            var typeMembersTree = parser.ParseTypeMembers(code);

            foreach (var typeMember in typeMembersTree.Where(x => x is TypeDeclaration || x is MethodDeclaration))
            {
                var element = typeMember.GetText();
                if (typeMember is TypeDeclaration)
                {
                    result.Declarations += element;
                }

                for (var i = typeMember.StartLocation.Line - 1; i < typeMember.StartLocation.Line; i++)
                {
                    var oldItem = codeLinesDictionary[i];
                    codeLinesDictionary[i] = new Tuple<string, bool>(oldItem.Item1, false);
                }
            }

            var keysToRemove = codeLinesDictionary.Where(x => x.Value.Item2 == false).Select(i => i.Key);
            keysToRemove.ToList().ForEach(x => codeLinesDictionary.Remove(x));

            foreach (var correct in syntaxTree.Members)
            {
                var element = correct.GetText(); ;
                result.Declarations += element;
            }

            if (syntaxTree.Errors.Any())
            {
                var evalLines = codeLines.Skip(syntaxTree.Errors.First().Region.BeginLine - 1).ToList();
                result.Evaluations += string.Join(Environment.NewLine, evalLines);
            }

            var evaluationTree = parser.ParseStatements(result.Evaluations);
            return result;
        }
    }
}
