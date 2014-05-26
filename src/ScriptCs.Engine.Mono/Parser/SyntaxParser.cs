﻿namespace ScriptCs.Engine.Mono.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using ICSharpCode.NRefactory.CSharp;
    using ICSharpCode.NRefactory.Editor;
    using ICSharpCode.NRefactory.CSharp.Refactoring;

    using ScriptCs.Engine.Mono.Parser.Visitors;

    public class SyntaxParser
    {
        public ParseResult Parse(string code)
        {
            var className = CreateUniqueName();
            code = WrapAsPseudoClass(className, code);

            var classes = ExtractClassDeclarations(className, code);
            code = RemoveClasses(code, classes);

            var methods = ExtractMethodDeclaration(code);
            code = RemoveMethods(code, methods);

            return new ParseResult 
            {
                TypeDeclarations = classes.Select(x => x.GetText()),
                MethodPrototypes = methods.Select(x => x.MethodPrototype.GetText()),
                MethodExpressions = methods.Select(x => x.MethodExpression.GetText()),
                Evaluations = UnWrapPseudoClass(code)
            };
        }
         
        private static IList<TypeDeclaration> ExtractClassDeclarations(string className, string code)
        {
            var visitor = new ClassTypeVisitor();
            var parser = new CSharpParser();
            var syntaxTree = parser.Parse(code);
            syntaxTree.AcceptVisitor(visitor);
            syntaxTree.Freeze();

            return visitor.GetClassDeclarations()
                    .Where(x => !x.GetText().StartsWith(string.Format("class {0}", className)))
                    .ToList();
        }

        private static string RemoveClasses(string code, IEnumerable<TypeDeclaration> classes)
        {
            var document = new StringBuilderDocument(code);
            using (
                var script = new DocumentScript(
                    document,
                    FormattingOptionsFactory.CreateAllman(),
                    new TextEditorOptions()))
            {
                foreach (var @class in classes)
                {
                    var offset = script.GetCurrentOffset(@class.GetRegion().Begin);
                    script.Replace(@class, new TypeDeclaration());
                    script.Replace(offset, new TypeDeclaration().GetText().Trim().Length, "");
                }
            }
            return document.Text;
        }

        private static IList<MethodVisitorResult> ExtractMethodDeclaration(string code)
        {
            var visitor = new MethodVisitor();
            var parser = new CSharpParser();
            var syntaxTree = parser.Parse(code);
            syntaxTree.AcceptVisitor(visitor);
            syntaxTree.Freeze();

            return visitor.GetMethodDeclarations();
        }

        private static string RemoveMethods(string code, IEnumerable<MethodVisitorResult> methods)
        {
            var document = new StringBuilderDocument(code);
            using (var script = new DocumentScript(
                document, 
                FormattingOptionsFactory.CreateAllman(), 
                new TextEditorOptions()))
            {
                foreach (var method in methods)
                {
                    var offset = script.GetCurrentOffset(method.MethodDefinition.GetRegion().Begin);
                    script.Replace(method.MethodDefinition, new MethodDeclaration());
                    script.Replace(offset, new MethodDeclaration().GetText().Trim().Length, "");
                }
            }
            return document.Text;
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