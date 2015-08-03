using System;
using System.Linq;

using ScriptCs.Engine.Mono.Segmenter.Analyser.Visitors;

using ICSharpCode.NRefactory.CSharp;

namespace ScriptCs.Engine.Mono.Segmenter.Analyser
{
    public class CodeAnalyzer
    {
        public bool IsClass(string code)
        {
            var visitor = new ClassTypeVisitor();
            var parser = new CSharpParser();
            var syntaxTree = parser.Parse(code);
            syntaxTree.AcceptVisitor(visitor);
            syntaxTree.Freeze();

            return visitor.GetClassDeclarations().Any();
        }

        public bool IsMethod(string code)
        {
            Guard.AgainstNullArgument("code", code);

            var @class = "class A { " + code + " } ";
            var visitor = new MethodVisitor();
            var parser = new CSharpParser();
            var syntaxTree = parser.Parse(@class);
            syntaxTree.AcceptVisitor(visitor);
            syntaxTree.Freeze();

            return visitor.GetMethodDeclarations().Any() && code.TrimEnd().EndsWith("}");
        }

        public MethodResult ExtractPrototypeAndMethod(string code)
        {
            Guard.AgainstNullArgument("code", code);

            var @class = "class A { " + code + " } ";
            var visitor = new MethodVisitor();
            var parser = new CSharpParser();
            var syntaxTree = parser.Parse(@class);
            syntaxTree.AcceptVisitor(visitor);
            syntaxTree.Freeze();

            var result = visitor.GetMethodDeclarations().FirstOrDefault();

            // find newlines in method signature to maintain linenumbers
            var newLines = code.Substring(0, code.IndexOf("{", StringComparison.Ordinal) - 1)
                .Where(x => x.Equals('\n'))
                .Aggregate(string.Empty, (a, c) => a + c);

            // use code methodblock to maintain linenumbers
            var codeBlock = code.Substring(code.IndexOf("{", StringComparison.Ordinal), code.LastIndexOf("}", StringComparison.Ordinal) - code.IndexOf("{", StringComparison.Ordinal) + 1);
            var method = result.MethodExpression.ToString();
            var blockStart = method.IndexOf("{", StringComparison.Ordinal);
            var blockEnd = method.LastIndexOf("}", StringComparison.Ordinal);
            method = method.Remove(blockStart, blockEnd - blockStart + 1);
            method = method.Insert(blockStart, codeBlock);

            return new MethodResult
            {
                ProtoType = result.MethodPrototype.ToString().Trim() + newLines,
                MethodExpression = newLines + method.Trim()
            };
        }
    }
}   