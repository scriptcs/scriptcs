﻿using System.Collections.Generic;
using System.Linq;
using Common.Logging;
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
            var logger = new Mock<ILog>();

            return new RoslynScriptEngine(scriptHostFactory.Object, logger.Object);
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
                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>())).Returns((IScriptPackManager p) => new ScriptHost(p));

                var code = "var a = 0;";

                var engine = CreateScriptEngine(scriptHostFactory: scriptHostFactory);
 
                var scriptPack1 = new Mock<IScriptPack>();
                scriptPack1.Setup(p => p.Initialize(It.IsAny<IScriptPackSession>()));
                scriptPack1.Setup(p => p.GetContext()).Returns((IScriptPackContext)null);

                var scriptPackSession = new ScriptPackSession(new[] { scriptPack1.Object });
    
                engine.Execute(code, Enumerable.Empty<string>(), Enumerable.Empty<string>(), scriptPackSession);

                scriptHostFactory.Verify(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>()));
            }
        }
    }
}