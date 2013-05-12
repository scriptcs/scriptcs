using System.Collections.Generic;
using System.Linq;
using Common.Logging;
using Moq;
using Roslyn.Scripting;
using Roslyn.Scripting.CSharp;
using ScriptCs.Contracts;
using ScriptCs.Engine.Roslyn;
using Should;
using Xunit;

namespace ScriptCs.Tests
{
    public class RoslynScriptEngineTests
    {
        public class TestRoslynScriptEngine : RoslynScriptEngine
        {
            public TestRoslynScriptEngine(IScriptHostFactory scriptHostFactory, ILog logger)
                : base(scriptHostFactory, logger)
            {

            }

            public Session Session { get; set; }

            protected override object Execute(string code, Session session)
            {
                Session = session;
                return null;
            }
        }

        private static RoslynScriptEngine CreateScriptEngine(
            Mock<IScriptHostFactory> scriptHostFactory = null)
        {
            scriptHostFactory = scriptHostFactory ?? new Mock<IScriptHostFactory>();
            var logger = new Mock<ILog>();

            return new RoslynScriptEngine(scriptHostFactory.Object, logger.Object);
        }

        private static TestRoslynScriptEngine CreateTestScriptEngine(
            Mock<IScriptHostFactory> scriptHostFactory = null)
        {
            scriptHostFactory = scriptHostFactory ?? new Mock<IScriptHostFactory>();
            var logger = new Mock<ILog>();

            return new TestRoslynScriptEngine(scriptHostFactory.Object, logger.Object);
        }

        public class TheExecuteMethod 
        {
            [Fact]
            public void ShouldCreateScriptHostWithContexts()
            {
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

            [Fact]
            public void ShouldReuseExistingSessionIfProvided()
            {
                var scriptHostFactory = new Mock<IScriptHostFactory>();
                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>())).Returns((IScriptPackManager p) => new ScriptHost(p));

                var code = "var a = 0;";

                var engine = CreateTestScriptEngine(scriptHostFactory: scriptHostFactory);
                var scriptPackSession = new ScriptPackSession(new List<IScriptPack>());
                var roslynEngine = new ScriptEngine();
                var session = roslynEngine.CreateSession();
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;
                engine.Execute(code, Enumerable.Empty<string>(), Enumerable.Empty<string>(), scriptPackSession);
                engine.Session.ShouldEqual(session);
            }

            [Fact]
            public void ShouldCreateNewSessionIfNotProvided()
            {
                var scriptHostFactory = new Mock<IScriptHostFactory>();
                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>())).Returns((IScriptPackManager p) => new ScriptHost(p));

                var code = "var a = 0;";

                var engine = CreateTestScriptEngine(scriptHostFactory: scriptHostFactory);
                var scriptPackSession = new ScriptPackSession(new List<IScriptPack>());
                engine.Execute(code, Enumerable.Empty<string>(), Enumerable.Empty<string>(), scriptPackSession);
                engine.Session.ShouldNotBeNull();
            }
        }
    }
}