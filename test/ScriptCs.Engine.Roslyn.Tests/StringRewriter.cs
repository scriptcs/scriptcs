namespace ScriptCs.Tests
{
    using Roslyn.Compilers;
    using Roslyn.Compilers.CSharp;
    using ScriptCs.Contracts;

    public class StringRewriter : SyntaxRewriter, ICodeRewriter
    {
        public string Rewrite(string code)
        {
            var syntaxTree = SyntaxTree.ParseText(code, options: new ParseOptions(kind: SourceCodeKind.Script));
            return Visit(syntaxTree.GetRoot()).ToFullString();
        }

        public override SyntaxNode DefaultVisit(SyntaxNode node)
        {
            return node;
        }

        public override SyntaxToken VisitToken(SyntaxToken token)
        {
            var rewritten = token.Kind == SyntaxKind.StringLiteralToken ? Syntax.Literal("blah") : token;
            return rewritten;
        }
    }
}