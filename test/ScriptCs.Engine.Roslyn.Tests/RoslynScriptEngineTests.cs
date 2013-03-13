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

        }
    }
}