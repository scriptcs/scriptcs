namespace ScriptCs.Engine.Roslyn
{
	using System;
	using System.Linq;

	using global::Roslyn.Compilers;
	using global::Roslyn.Compilers.CSharp;

	using ScriptCs.Contracts;

	public abstract class RoslynSyntaxRewriter : SyntaxRewriter, ICodeRewriter
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

		public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node)
		{
			var externs = node.Externs.Select(Visit);
			var usings = node.Usings.Select(Visit);
			var attributeLists = node.AttributeLists.Select(Visit);
			var members = node.Members.Select(Visit);
			return Syntax.CompilationUnit(
				Syntax.List(externs),
				Syntax.List(usings),
				Syntax.List(attributeLists),
				Syntax.List(members));
		}

		public override SyntaxNode VisitArgument(ArgumentSyntax node)
		{
			var expression = Visit(node.Expression) ?? node.Expression;
			var nameColon = Visit(node.NameColon) ?? node.NameColon;
			var refOrOut = VisitToken(node.RefOrOutKeyword);

			return Syntax.Argument((NameColonSyntax)nameColon, refOrOut, (ExpressionSyntax)expression);
		}

		public override SyntaxNode VisitParameterList(ParameterListSyntax node)
		{
			var parameters = node.Parameters.Select(Visit).Cast<ParameterSyntax>().ToArray();
			return
				Syntax.ParameterList(
					Syntax.SeparatedList(
						parameters,
						Enumerable.Repeat(Syntax.Token(SyntaxKind.CommaToken), node.Parameters.SeparatorCount)));
		}

		public override SyntaxNode VisitLiteralExpression(LiteralExpressionSyntax node)
		{
			var token = VisitToken(node.Token);
			return Syntax.LiteralExpression(node.Kind, token);
		}
	}

	public class StringRewriter : RoslynSyntaxRewriter
	{
		public override SyntaxToken VisitToken(SyntaxToken token)
		{
			var rewritten = token.Kind == SyntaxKind.StringLiteralToken ? Syntax.Literal("blah") : token;
			return rewritten;
		}
	}
}