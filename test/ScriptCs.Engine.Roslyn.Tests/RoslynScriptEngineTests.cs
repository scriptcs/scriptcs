using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using ScriptCs.Contracts;
using ScriptCs.Engine.Roslyn;
using Xunit;

namespace ScriptCs.Tests
{
    public class RoslynScriptEngineTests
    {
        private static RoslynScriptEngine CreateScriptEngine(
            Mock<IScriptHostFactory> scriptHostFactory = null)
        {
            scriptHostFactory = scriptHostFactory ?? new Mock<IScriptHostFactory>();

            return new RoslynScriptEngine {
                ScriptHostFactory = scriptHostFactory.Object
            };
        }

        public class TheExecuteMethod 
        {
            [Fact]
            public void ShouldInitializeScriptPacks()
            {
                var fileSystem = new Mock<IFileSystem>();
                fileSystem.Setup(f => f.CurrentDirectory).Returns(@"c:\my_script");
                fileSystem.Setup(f => f.GetWorkingDirectory(It.IsAny<string>())).Returns(@"c:\my_script");

                var code = "var a = 0;";
                var engine = CreateScriptEngine();

                var scriptPack1 = new Mock<IScriptPack>();
                scriptPack1.Setup(p => p.Initialize(It.IsAny<IScriptPackSession>()));
                scriptPack1.Setup(p => p.GetContext()).Returns(Mock.Of<IScriptPackContext>());

                engine.Execute(code, Enumerable.Empty<string>(), new List<IScriptPack> { scriptPack1.Object });
                scriptPack1.Verify(p => p.Initialize(It.IsAny<IScriptPackSession>()));
            }

            [Fact]
            public void ShouldCreateScriptHostWithContexts()
            {
                var fileSystem = new Mock<IFileSystem>();
                fileSystem.Setup(f => f.CurrentDirectory).Returns(@"c:\my_script");
                fileSystem.Setup(f => f.GetWorkingDirectory(It.IsAny<string>())).Returns(@"c:\my_script");

                var scriptHostFactory = new Mock<IScriptHostFactory>();
                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IEnumerable<IScriptPackContext>>())).Returns((IEnumerable<IScriptPackContext> c) => new ScriptHost(c));

                var code = "var a = 0;";
                var engine = CreateScriptEngine(scriptHostFactory: scriptHostFactory);

                var scriptPack = new Mock<IScriptPack>();
                var context = new Mock<IScriptPackContext>().Object;

                scriptPack.Setup(p => p.GetContext()).Returns(context);

                engine.Execute(code, Enumerable.Empty<string>(), new List<IScriptPack> { scriptPack.Object });
                scriptHostFactory.Verify(f => f.CreateScriptHost(It.IsAny<IEnumerable<IScriptPackContext>>()));
            }
 
            [Fact]
            public void ShouldTerminateScriptPacksWhenScriptFinishes()
            {
                var fileSystem = new Mock<IFileSystem>();
                fileSystem.Setup(f => f.CurrentDirectory).Returns(@"c:\my_script");
                fileSystem.Setup(f => f.GetWorkingDirectory(It.IsAny<string>())).Returns(@"c:\my_script");

                var code = "var a = 0;";
                var engine = CreateScriptEngine();

                var scriptPack1 = new Mock<IScriptPack>();
                scriptPack1.Setup(p => p.GetContext()).Returns(Mock.Of<IScriptPackContext>());

                engine.Execute(code, Enumerable.Empty<string>(), new List<IScriptPack> { scriptPack1.Object });
                scriptPack1.Verify(p => p.Terminate());
            }
        }
    }
}