namespace ScriptCs.Engine.Mono.Tests.Parser
{
    using System;
    using System.Linq;

    using ScriptCs.Engine.Mono.Parser;

    using Should;

    using Xunit;

    public class SyntaxParserTests
    {
        public class TheParseMethod
        {
            [Fact]
            public void ShouldNotFailOnNoCode()
            {
                var parser = new SyntaxParser();
                var result = parser.Parse(string.Empty);

                result.TypeDeclarations.Any().ShouldEqual(false);
                result.MethodPrototypes.Any().ShouldEqual(false);
                result.MethodExpressions.Any().ShouldEqual(false);
                result.Evaluations.ShouldEqual(string.Empty);
            }

            [Fact]
            public void ShouldParseSimpleStatement()
            {
                const string Code = "var x = 42;";

                var parser = new SyntaxParser();
                var result = parser.Parse(Code);

                result.TypeDeclarations.Any().ShouldEqual(false);
                result.MethodPrototypes.Any().ShouldEqual(false);
                result.MethodExpressions.Any().ShouldEqual(false);
                result.Evaluations.ShouldEqual(Code);
            }

            [Fact]
            public void ShouldExtractAllClasses()
            {
                const string Code = "class A {} int x; class B {}";

                var parser = new SyntaxParser();
                var result = parser.Parse(Code);

                var classes = result.TypeDeclarations.ToList();

                classes.Count.ShouldEqual(2);

                classes[0].ShouldContain("class A");
                classes[0].ShouldNotContain("int x;");
                classes[1].ShouldContain("class B");
                classes[1].ShouldNotContain("int x;");
            }

            [Fact]
            public void ShouldSupportLooseFunctions()
            {
                const string Code = "public int Foo(int a, int b) { return a + b; }";

                var parser = new SyntaxParser();
                var result = parser.Parse(Code);

                result.MethodPrototypes.Count().ShouldEqual(1);
                result.MethodExpressions.Count().ShouldEqual(1);

                result.MethodPrototypes
                    .FirstOrDefault()
                    .ShouldContain("Func<int, int, int> Foo;");

                result.MethodExpressions
                    .FirstOrDefault()
                    .ShouldContain("Foo = delegate (int a, int b) {" + Environment.NewLine 
                        + "\treturn a + b;" + Environment.NewLine 
                        + "};");
            }

            [Fact]
            public void ShouldSupportMultipleFunctions()
            {
                const string Code = "void Foo() {} int x; void Bar() {}";

                var parser = new SyntaxParser();
                var result = parser.Parse(Code);

                result.MethodPrototypes.Count().ShouldEqual(2);
                result.MethodExpressions.Count().ShouldEqual(2);

                var methods = result.MethodPrototypes.ToList();
                methods[0].ShouldContain("Action Foo;");
                methods[1].ShouldContain("Action Bar;");
            }

            [Fact]
            public void ShouldExtractEvaluation()
            {
                const string Code = "void Foo() {} int x; void Bar() {}";

                var parser = new SyntaxParser();
                var result = parser.Parse(Code);

                result.Evaluations.ShouldContain("int x;");
                result.Evaluations.ShouldNotContain("Foo");
                result.Evaluations.ShouldNotContain("Bar");
            }

            /*
            TODO: This test still fails.
            [Fact]
            public void ShouldParseCodeWithBlockStatements()
            {
                const string Code = "class A {} if(true) { var x = 10; } class B {}";

                var parser = new SyntaxParser();
                var result = parser.Parse(Code);

                var classes = result.TypeDeclarations.ToList();

                classes.Count.ShouldEqual(2);

                classes[0].ShouldContain("class A");
                classes[0].ShouldNotContain("var x = 10;");
                classes[1].ShouldContain("class B"); 
                classes[1].ShouldNotContain("var x = 10;");

                result.Evaluations.ShouldContain("if(true) { var x = 10; }");
            }
            */
        }
    }
}
