namespace ScriptCs.SyntaxTreeParser
{
    using System;
    using System.Linq;
    using System.Text;

    using ICSharpCode.NRefactory.CSharp;
    using ICSharpCode.NRefactory.Editor;
    using ICSharpCode.NRefactory.CSharp.Refactoring;

    using ScriptCs.SyntaxTreeParser.Visitors;

    public class SyntaxParser
    {
        public ParseResult Parse(string code)
        {
            var className = CreateUniqueName();
            code = WrapAsPseudoClass(className, code);
            return new ParseResult
                             {
                                 TypeDeclarations = ExtractClassDeclarations(className, ref code),
                                 MethodDeclarations = ExtractMethodDeclaration(ref code),
                                 Evaluations = UnWrapPseudoClass(code)
                             };
        }
         
        private static string ExtractClassDeclarations(string className, ref string code)
        {
            var visitor = new ClassTypeVisitor();
            var parser = new CSharpParser();
            var syntaxTree = parser.Parse(code);
            syntaxTree.AcceptVisitor(visitor);
            syntaxTree.Freeze();

            var result = string.Empty;
            var document = new StringBuilderDocument(code);
            using (
                var script = new DocumentScript(
                    document,
                    FormattingOptionsFactory.CreateAllman(),
                    new TextEditorOptions()))
            {
                foreach (var klass in visitor.GetClassDeclarations())
                {
                    var src = klass.GetText();
                    if (src.StartsWith(string.Format("class {0}", className)))
                    {
                        continue;
                    }
                    result += src;
                    var offset = script.GetCurrentOffset(klass.GetRegion().Begin);
                    script.Replace(klass, new TypeDeclaration());
                    script.Replace(offset, new TypeDeclaration().GetText().Trim().Length, "");
                }
                code = document.Text;
            }
            return result;
        }

        private static string ExtractMethodDeclaration(ref string code)
        {
            var visitor = new MethodVisitor();
            var parser = new CSharpParser();
            var syntaxTree = parser.Parse(code);
            syntaxTree.AcceptVisitor(visitor);
            syntaxTree.Freeze();

            var result = string.Empty;
            var document = new StringBuilderDocument(code);
            using (var script = new DocumentScript(
                document, 
                FormattingOptionsFactory.CreateAllman(), 
                new TextEditorOptions()))
            {
                foreach (var method in visitor.GetMethodDeclarations())
                {
                    var oldMethod = method.Item1;
                    var newMethod = method.Item2;
                    result += newMethod.GetText();

                    var offset = script.GetCurrentOffset(oldMethod.GetRegion().Begin);
                    script.Replace(oldMethod, new MethodDeclaration());
                    script.Replace(offset, new MethodDeclaration().GetText().Trim().Length, "");
                }
            }
            code = document.Text;
            return result;
        }

        private static string CreateUniqueName()
        {
            return string.Format("_{0}", Guid.NewGuid().ToString().Replace('-', '_'));
        }

        private static string WrapAsPseudoClass(string className, string code)
        {
            var sb = new StringBuilder();
            sb.AppendFormat(@"class {0} ", className);
            sb.Append("{");
            sb.Append(Environment.NewLine);
            sb.Append(code);
            sb.Append(Environment.NewLine);
            sb.Append("}");

            return sb.ToString();
        }

        private static string UnWrapPseudoClass(string code)
        {
            var lines = code.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            return string.Join(Environment.NewLine, lines.Skip(1).Take(lines.Count() - 2));
        }
    }
}
