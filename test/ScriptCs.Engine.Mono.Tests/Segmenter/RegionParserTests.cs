using System.Linq;
using ScriptCs.Engine.Mono.Segmenter.Parser;
using Should;
using Xunit;

namespace ScriptCs.Engine.Mono.Tests.Segmenter
{
    public class RegionParserTests
    {
        public class ParseStatements
        {
            [Fact]
            public void ShouldExtractEmptyStatement()
            {
                const string Code = ";";

                var parser = new RegionParser();
                var result = parser.Parse(Code);

                result.Count().ShouldEqual(1);
                var region = result[0];
                Code.Substring(region.Offset, region.Length).ShouldEqual(Code);
            }

            [Fact]
            public void ShouldExtractVariableStatements()
            {
                const string Code = "var x = 10;";

                var parser = new RegionParser();
                var result = parser.Parse(Code);

                result.Count().ShouldEqual(1);
                var region = result[0];
                Code.Substring(region.Offset, region.Length).ShouldEqual(Code);
            }

            // Bug #713
            [Fact]
            public void ShouldExtractMultipleStatements()
            {
                const string Code = "var x = 123;Action a = () => x++;Console.WriteLine(x);";

                var parser = new RegionParser();
                var result = parser.Parse(Code);

                result.Count().ShouldEqual(3);
                var region = result[0];
                Code.Substring(region.Offset, region.Length).ShouldEqual("var x = 123;");

                region = result[1];
                Code.Substring(region.Offset, region.Length).ShouldEqual("Action a = () => x++;");

                region = result[2];
                Code.Substring(region.Offset, region.Length).ShouldEqual("Console.WriteLine(x);");
            }

            [Fact]
            public void ShouldExtractComplexVariableStatement()
            {
                const string Code = "var version = File.ReadAllText(\"src/CommonAssemblyInfo.cs\").Split(new[] { \"AssemblyInformationalVersion(\\\"\" }, 2, StringSplitOptions.None).ElementAt(1).Split(new[] { '\"' }).First();";

                var parser = new RegionParser();
                var result = parser.Parse(Code);

                result.Count().ShouldEqual(1);

                var region = result[0];
                Code.Substring(region.Offset, region.Length).ShouldEqual(Code);
            }

            [Fact]
            public void ShouldExtractComplexMethods()
            {
                const string Code = "var bau = Require<Bau>();\n\nbau\n.Task(\"default\").DependsOn(string.IsNullOrWhiteSpace(ci) ? new[] { \"unit\", \"component\", \"pack\" } : new[] { \"unit\", \"component\", \"accept\", \"pack\" });\n";

                var parser = new RegionParser();
                var result = parser.Parse(Code);

                result.Count().ShouldEqual(2);

                var region = result[0];
                Code.Substring(region.Offset, region.Length).ShouldEqual("var bau = Require<Bau>();");

                region = result[1];
                Code.Substring(region.Offset, region.Length).ShouldEqual("bau\n.Task(\"default\").DependsOn(string.IsNullOrWhiteSpace(ci) ? new[] { \"unit\", \"component\", \"pack\" } : new[] { \"unit\", \"component\", \"accept\", \"pack\" });");
            }
        }

        public class ParseBlocks
        {
            [Fact]
            public void ShouldExtractBlock()
            {
                const string Code = "if (true) { some code; }";

                var parser = new RegionParser();
                var result = parser.Parse(Code);

                result.Count().ShouldEqual(1);
                var region = result[0];
                Code.Substring(region.Offset, region.Length).ShouldEqual(Code);
            }

            [Fact]
            public void ShouldExtractMultipleBlocks()
            {
                const string Code = "using(var s = File.Open(\"test.cs\")){ } public Foo(int x) { return x }";

                var parser = new RegionParser();
                var result = parser.Parse(Code);

                result.Count().ShouldEqual(2);

                var region = result[0];
                Code.Substring(region.Offset, region.Length).ShouldEqual("using(var s = File.Open(\"test.cs\")){ }");

                region = result[1];
                Code.Substring(region.Offset, region.Length).ShouldEqual("public Foo(int x) { return x }");
            }

            [Fact]
            public void ShouldExtractBlockCodeThatBeginsWithLeftCurlyBrackets()
            {
                const string Code = "{ Foo(); ) }";

                var parser = new RegionParser();
                var result = parser.Parse(Code);

                result.Count().ShouldEqual(1);
                var region = result[0];
                Code.Substring(region.Offset, region.Length).ShouldEqual(Code);
            }

            [Fact]
            public void ShouldExtractForLoops() // for loops have ';'
            {
                const string Code = "for(var i = 0; i < 3; i++) { Foo(); }";

                var parser = new RegionParser();
                var result = parser.Parse(Code);

                result.Count().ShouldEqual(1);
                var region = result[0];
                Code.Substring(region.Offset, region.Length).ShouldEqual(Code);
            }

            [Fact]
            public void ShouldExtractDoWhileAsSingleBlock()
            {
                const string Code = "do { }   while (true);";

                var parser = new RegionParser();
                var result = parser.Parse(Code);

                result.Count().ShouldEqual(1);

                var region = result[0];
                Code.Substring(region.Offset, region.Length).ShouldEqual(Code);
            }

            [Fact]
            public void ShouldExtractInvalidDoWhileAsTwoBlocks()
            {
                const string Code = "do { }   if (true);";

                var parser = new RegionParser();
                var result = parser.Parse(Code);

                result.Count().ShouldEqual(2);

                var region = result[0];
                Code.Substring(region.Offset, region.Length).ShouldEqual("do { }");

                region = result[1];
                Code.Substring(region.Offset, region.Length).ShouldEqual("if (true);");
            }

            [Fact]
            public void ShouldExtractIfElseAsSingleBlock()
            {
                const string Code = "if { } else { }";

                var parser = new RegionParser();
                var result = parser.Parse(Code);

                result.Count().ShouldEqual(1);

                var region = result[0];
                Code.Substring(region.Offset, region.Length).ShouldEqual(Code);
            }

            [Fact]
            public void ShouldExtractMultipleIfElseAsSingleBlock()
            {
                const string Code = "if { } else if { } else if { } else { }";

                var parser = new RegionParser();
                var result = parser.Parse(Code);

                result.Count().ShouldEqual(1);

                var region = result[0];
                Code.Substring(region.Offset, region.Length).ShouldEqual(Code);
            }
        }

        public class ParseExpressions
        {
            [Fact]
            public void ShouldExtractExpressionsInParantheses()
            {
                const string Code = "(10 + 5 * ( 4 - 8) )";

                var parser = new RegionParser();
                var result = parser.Parse(Code);

                result.Count().ShouldEqual(1);
                var region = result[0];
                Code.Substring(region.Offset, region.Length).ShouldEqual(Code);
            }
        }
    }
}