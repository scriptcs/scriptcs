namespace ScriptCs.Engine.Mono.Tests.Parser
{
    using System;
    using System.Linq;

    using Common.Logging;
    using Moq;
    using ScriptCs.Engine.Mono.Parser;
    using Should;
    using Xunit;

    public class SyntaxParserTests
    {
        public class TheParseMethod
        {
            private readonly Mock<ILog> _logger;

            public TheParseMethod()
            {
                _logger = new Mock<ILog>();
            }

            [Fact]
            public void ShouldNotFailOnNoCode()
            {
                var parser = new SyntaxParser(_logger.Object);
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

                var parser = new SyntaxParser(_logger.Object);
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

                var parser = new SyntaxParser(_logger.Object);
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

                var parser = new SyntaxParser(_logger.Object);
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

                var parser = new SyntaxParser(_logger.Object);
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

                var parser = new SyntaxParser(_logger.Object);
                var result = parser.Parse(Code);

                result.Evaluations.ShouldContain("int x;");
                result.Evaluations.ShouldNotContain("Foo");
                result.Evaluations.ShouldNotContain("Bar");
            }

            /// <summary>
            /// This test check that the evaluations of a script file that is sent to the 
            /// mono engine will be compiled as a pseudo class that inherits MonoHost 
            /// (so it has access to Require<>) and captures the evaluation inside a 
            /// static Run method. Then a method is generated to execute the evaluation.
            /// 
            /// Example:
            ///   source: #line 1 /tmp/ \n int x;
            /// 
            /// Will be:
            ///   TypeDeclaration: class _GUID : MonoHost { public static void Run() { int x; }
            ///   Evaluation: _GUID.Run()
            /// </summary>
            [Fact]
            public void ShouldCompileFileSourcesAsPseudoStaticMethodsAndHaveEvalExec()
            {
                const string Code = "#line 1 /tmp/ \n var x = 123;Action a = () => x++;";

                var parser = new SyntaxParser(_logger.Object);
                var result = parser.Parse(Code);

                result.TypeDeclarations.Count().ShouldEqual(1);

                result.TypeDeclarations.FirstOrDefault().ShouldStartWith("class _");
                result.TypeDeclarations.FirstOrDefault()
                    .ShouldContain(": ScriptCs.Engine.Mono.MonoHost" + Environment.NewLine 
                        + "{" + Environment.NewLine 
                        + "\tpublic static void Run ()" + Environment.NewLine 
                        + "\t{" + Environment.NewLine 
                        + "\t\tvar x = 123;" + Environment.NewLine 
                        + "\t\tAction a = () => x++;" + Environment.NewLine 
                        + "\t}" + Environment.NewLine 
                        + "}");

                result.Evaluations.ShouldStartWith("_");
                result.Evaluations.ShouldContain(".Run()");
                result.Evaluations.ShouldNotContain("int x;");
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
