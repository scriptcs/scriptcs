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

            protected override ScriptResult Execute(string code, Session session)
            {
                Session = session;
                return new ScriptResult();
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
                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<string[]>())).Returns((IScriptPackManager p, string[] q) => new ScriptHost(p, q));

                var code = "var a = 0;";

                var engine = CreateScriptEngine(scriptHostFactory: scriptHostFactory);
 
                var scriptPack1 = new Mock<IScriptPack>();
                scriptPack1.Setup(p => p.Initialize(It.IsAny<IScriptPackSession>()));
                scriptPack1.Setup(p => p.GetContext()).Returns((IScriptPackContext)null);

                var scriptPackSession = new ScriptPackSession(new[] { scriptPack1.Object });
    
                engine.Execute(code, new string[0], Enumerable.Empty<string>(), Enumerable.Empty<string>(), scriptPackSession);

                scriptHostFactory.Verify(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<string[]>()));
            }

            [Fact]
            public void ShouldReuseExistingSessionIfProvided()
            {
                var scriptHostFactory = new Mock<IScriptHostFactory>();
                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<string[]>())).Returns((IScriptPackManager p, string[] q) => new ScriptHost(p, q));

                var code = "var a = 0;";

                var engine = CreateTestScriptEngine(scriptHostFactory: scriptHostFactory);
                var scriptPackSession = new ScriptPackSession(new List<IScriptPack>());
                var roslynEngine = new ScriptEngine();
                var session = new SessionState<Session> {Session = roslynEngine.CreateSession()};
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;
                engine.Execute(code, new string[0], Enumerable.Empty<string>(), Enumerable.Empty<string>(), scriptPackSession);
                engine.Session.ShouldEqual(session.Session);
            }

            [Fact]
            public void ShouldCreateNewSessionIfNotProvided()
            {
                var scriptHostFactory = new Mock<IScriptHostFactory>();
                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<string[]>())).Returns((IScriptPackManager p, string[] q) => new ScriptHost(p, q));

                var code = "var a = 0;";

                var engine = CreateTestScriptEngine(scriptHostFactory: scriptHostFactory);
                var scriptPackSession = new ScriptPackSession(new List<IScriptPack>());
                engine.Execute(code, new string[0], Enumerable.Empty<string>(), Enumerable.Empty<string>(), scriptPackSession);
                engine.Session.ShouldNotBeNull();
            }

            [Fact]
            public void ShouldAddNewReferencesIfTheyAreProvided()
            {
                var scriptHostFactory = new Mock<IScriptHostFactory>();
                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<string[]>())).Returns((IScriptPackManager p, string[] q) => new ScriptHost(p, q));

                var code = "var a = 0;";

                var engine = CreateTestScriptEngine(scriptHostFactory: scriptHostFactory);
                var scriptPackSession = new ScriptPackSession(new List<IScriptPack>());
                var roslynEngine = new ScriptEngine();
                var session = new SessionState<Session> { Session = roslynEngine.CreateSession()};
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;
                engine.Execute(code, new string[0], new[] {"System"}, Enumerable.Empty<string>(), scriptPackSession);
                
                ((SessionState<Session>)scriptPackSession.State[RoslynScriptEngine.SessionKey]).References.Count().ShouldEqual(1);
            }

            [Fact]
            public void ShouldReturnAScriptResult()
            {
                var scriptHostFactory = new Mock<IScriptHostFactory>();
                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<string[]>())).Returns((IScriptPackManager p, string[] q) => new ScriptHost(p, q));

                var code = "";

                var engine = CreateTestScriptEngine(scriptHostFactory: scriptHostFactory);
                var scriptPackSession = new ScriptPackSession(new List<IScriptPack>());
                var roslynEngine = new ScriptEngine();
                var session = new SessionState<Session> { Session = roslynEngine.CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;
                var result = engine.Execute(code, new string[0], new[] { "System" }, Enumerable.Empty<string>(), scriptPackSession);

                Assert.IsType<ScriptResult>(result);
            }

            [Fact]
            public void ShouldReturnCompileExceptionIfCodeDoesNotCompile()
            {
                var scriptHostFactory = new Mock<IScriptHostFactory>();
                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<string[]>())).Returns((IScriptPackManager p, string[] q) => new ScriptHost(p, q));

                var code = "this shold not compile";

                var engine = CreateScriptEngine(scriptHostFactory: scriptHostFactory);
                var scriptPackSession = new ScriptPackSession(new List<IScriptPack>());
                var roslynEngine = new ScriptEngine();
                var session = new SessionState<Session> { Session = roslynEngine.CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;
                var result = engine.Execute(code, new string[0], new[] { "System" }, Enumerable.Empty<string>(), scriptPackSession);

                result.CompileException.ShouldNotBeNull();
            }

            [Fact]
            public void ShouldNotReturnCompileExceptionIfCodeDoesCompile()
            {
                var scriptHostFactory = new Mock<IScriptHostFactory>();
                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<string[]>())).Returns((IScriptPackManager p, string[] q) => new ScriptHost(p, q));

                var code = "var theNumber = 42; //this should compile";

                var engine = CreateScriptEngine(scriptHostFactory: scriptHostFactory);
                var scriptPackSession = new ScriptPackSession(new List<IScriptPack>());
                var roslynEngine = new ScriptEngine();
                var session = new SessionState<Session> { Session = roslynEngine.CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;
                var result = engine.Execute(code, new string[0], new[] { "System" }, Enumerable.Empty<string>(), scriptPackSession);

                result.CompileException.ShouldBeNull();
            }

            [Fact]
            public void ShouldReturnExecuteExceptionIfCodeExecutionThrowsException()
            {
                var scriptHostFactory = new Mock<IScriptHostFactory>();
                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<string[]>())).Returns((IScriptPackManager p, string[] q) => new ScriptHost(p, q));

                var code = "throw new System.Exception(); //this should throw an Exception";

                var engine = CreateScriptEngine(scriptHostFactory: scriptHostFactory);
                var scriptPackSession = new ScriptPackSession(new List<IScriptPack>());
                var roslynEngine = new ScriptEngine();
                var session = new SessionState<Session> { Session = roslynEngine.CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;
                var result = engine.Execute(code, new string[0], new[] { "System" }, Enumerable.Empty<string>(), scriptPackSession);

                result.ExecuteException.ShouldNotBeNull();
            }

            [Fact]
            public void ShouldNotReturnExecuteExceptionIfCodeExecutionDoesNotThrowAnException()
            {
                var scriptHostFactory = new Mock<IScriptHostFactory>();
                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<string[]>())).Returns((IScriptPackManager p, string[] q) => new ScriptHost(p, q));

                var code = "var theNumber = 42; //this should not throw an Exception";

                var engine = CreateScriptEngine(scriptHostFactory: scriptHostFactory);
                var scriptPackSession = new ScriptPackSession(new List<IScriptPack>());
                var roslynEngine = new ScriptEngine();
                var session = new SessionState<Session> { Session = roslynEngine.CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;
                var result = engine.Execute(code, new string[0], new[] { "System" }, Enumerable.Empty<string>(), scriptPackSession);

                result.ExecuteException.ShouldBeNull();
            }

            [Fact]
            public void ShouldReturnReturnValueIfCodeExecutionReturnsValue()
            {
                var scriptHostFactory = new Mock<IScriptHostFactory>();
                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<string[]>())).Returns((IScriptPackManager p, string[] q) => new ScriptHost(p, q));

                var code = "\"Hello\" //this should return \"Hello\"";

                var engine = CreateScriptEngine(scriptHostFactory: scriptHostFactory);
                var scriptPackSession = new ScriptPackSession(new List<IScriptPack>());
                var roslynEngine = new ScriptEngine();
                var session = new SessionState<Session> { Session = roslynEngine.CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;
                var result = engine.Execute(code, new string[0], new[] { "System" }, Enumerable.Empty<string>(), scriptPackSession);

                result.ReturnValue.ShouldEqual("Hello");
            }

            [Fact]
            public void ShouldNotReturnReturnValueIfCodeExecutionDoesNotReturnValue()
            {
                var scriptHostFactory = new Mock<IScriptHostFactory>();
                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<string[]>())).Returns((IScriptPackManager p, string[] q) => new ScriptHost(p, q));

                var code = "var theNumber = 42; //this should not return a value";

                var engine = CreateScriptEngine(scriptHostFactory: scriptHostFactory);
                var scriptPackSession = new ScriptPackSession(new List<IScriptPack>());
                var roslynEngine = new ScriptEngine();
                var session = new SessionState<Session> { Session = roslynEngine.CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;
                var result = engine.Execute(code, new string[0], new[] { "System" }, Enumerable.Empty<string>(), scriptPackSession);

                result.ReturnValue.ShouldBeNull();
            }
        }
    }
}