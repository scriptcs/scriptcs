using ScriptCs.Engine.Mono.Segmenter.Analyser;
using Should;
using Xunit;

namespace ScriptCs.Engine.Mono.Tests.Segmenter
{
    public class CodeAnalyserTests
    {
        public class AnalyseSegments
        {
            [Fact]
            public void ShouldReturnTrueIfIsClass()
            {
                const string Code = "class A { }";

                var analyser = new CodeAnalyzer();
                analyser.IsClass(Code).ShouldBeTrue();
            }

            [Fact]
            public void ShouldReturnFalseIfIsNotClass()
            {
                const string Code = "void Bar() { }";

                var analyser = new CodeAnalyzer();
                analyser.IsClass(Code).ShouldBeFalse();
            }

            [Fact]
            public void ShouldReturnFalseIfIsNotMethod()
            {
                const string Code = "class A { } ";

                var rewriter = new CodeAnalyzer();
                rewriter.IsMethod(Code).ShouldBeFalse();
            }

            [Fact]
            public void ShouldReturnFalseIfIncompeteMethod()
            {
                const string Code = "void Bar() { ";

                var analyser = new CodeAnalyzer();
                analyser.IsMethod(Code).ShouldBeFalse();
            }

            [Fact]
            public void ShouldReturnFalseIfMissingMethodBody()
            {
                const string Code = "void Bar()";

                var analyser = new CodeAnalyzer();
                analyser.IsMethod(Code).ShouldBeFalse();
            }

            [Fact]
            public void ShouldReturnTrueIfIsMethod()
            {
                const string Code = "void Bar() { }";

                var analyser = new CodeAnalyzer();
                analyser.IsMethod(Code).ShouldBeTrue();
            }
        }

        public class MethodRewrites
        {
            [Fact]
            public void ShouldRewriteToPrototypeAndExpression()
            {
                const string Code = "void Bar() { }";

                var analyser = new CodeAnalyzer();
                var method = analyser.ExtractPrototypeAndMethod(Code);

                method.ProtoType.ShouldEqual("Action Bar;");
                method.MethodExpression.ShouldEqual("Bar = delegate () { };");
            }

            [Fact]
            public void ShouldPreserveMethodBody()
            {
                const string Code = "int Foo(int a) { Foo();\n\nreturn a;\n}";

                var rewriter = new CodeAnalyzer();
                var method = rewriter.ExtractPrototypeAndMethod(Code);

                method.ProtoType.ShouldEqual("Func<int, int> Foo;");
                method.MethodExpression.ShouldEqual("Foo = delegate (int a) { Foo();\n\nreturn a;\n};");
            }

            [Fact]
            public void ShouldPreserveLineCountInMethodSignature()
            {
                const string Code = "int\nFoo\n(\n)\n { return 42; }";

                var rewriter = new CodeAnalyzer();
                var method = rewriter.ExtractPrototypeAndMethod(Code);

                method.ProtoType.ShouldEqual("Func<int> Foo;\n\n\n\n");
                method.MethodExpression.ShouldEqual("\n\n\n\nFoo = delegate () { return 42; };");
            }
        }
    }
}