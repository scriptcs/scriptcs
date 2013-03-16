using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using ScriptCs.Contracts;
using ScriptCs.Engine.Roslyn;
using Should;
using Xunit;

namespace ScriptCs.Tests
{
    using Roslyn.Scripting.CSharp;
    using Xunit.Extensions;

    public class RoslynScriptEngineTests
    {
        private static RoslynScriptEngine CreateScriptEngine(
            Mock<IScriptHostFactory> scriptHostFactory = null)
        {
            scriptHostFactory = scriptHostFactory ?? new Mock<IScriptHostFactory>();

            return new RoslynScriptEngine(scriptHostFactory.Object);
        }

        public class TheExecuteMethod 
        {
            [Fact]
            public void ShouldCreateScriptHostWithContexts()
            {
                var fileSystem = new Mock<IFileSystem>();
                fileSystem.Setup(f => f.GetWorkingDirectory(It.IsAny<string>())).Returns(@"c:\my_script");
                fileSystem.Setup(f => f.CurrentDirectory).Returns(@"c:\my_script");

                var scriptHostFactory = new Mock<IScriptHostFactory>();
                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IEnumerable<IScriptPackContext>>())).Returns((IEnumerable<IScriptPackContext> c) => new ScriptHost(c));

                var code = "var a = 0;";

                var engine = CreateScriptEngine(scriptHostFactory: scriptHostFactory);

                var scriptPack1 = new Mock<IScriptPack>();
                scriptPack1.Setup(p => p.Initialize(It.IsAny<IScriptPackSession>()));
                scriptPack1.Setup(p => p.GetContext()).Returns(Mock.Of<IScriptPackContext>());

                var scriptPackSession = new ScriptPackSession(new[] { scriptPack1.Object });

                engine.Execute(code, Enumerable.Empty<string>(), scriptPackSession);

                scriptHostFactory.Verify(f => f.CreateScriptHost(It.IsAny<IEnumerable<IScriptPackContext>>()));
            }

            [Theory]
            [InlineData("1 + 1", 2)]
            [InlineData("int a; a = 1;", 1)]
            [InlineData("int a = 1;", null)]
            [InlineData("var b = \"test\"; b", "test")]
            [InlineData("System.Console.WriteLine()", null)]
            public void ShouldReturnValueOfLastExpressionOrNullForVoid(string code, object expectedResult)
            {
                var engine = CreateScriptEngine();
                var result = engine.Execute(code, Enumerable.Empty<string>(), new ScriptPackSession(Enumerable.Empty<IScriptPack>()));
                result.ShouldEqual(expectedResult, "Script: " + code);
            }
        }
    }
}