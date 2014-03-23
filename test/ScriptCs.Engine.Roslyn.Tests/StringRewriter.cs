namespace ScriptCs.Tests
{
    using Roslyn.Compilers;
    using Roslyn.Compilers.CSharp;
    using ScriptCs.Contracts;

    public class StringRewriter : SyntaxRewriter, ICodeRewriter
    {
        public FilePreProcessorResult Rewrite(FilePreProcessorResult preProcessorResult)
        {
            var syntaxTree = SyntaxTree.ParseText(preProcessorResult.Code, options: new ParseOptions(kind: SourceCodeKind.Script));
            preProcessorResult.Code =  Visit(syntaxTree.GetRoot()).ToFullString();

            return preProcessorResult;
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