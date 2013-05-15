using System;
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

            private readonly ScriptPackSession _scriptPackSession = new ScriptPackSession(Enumerable.Empty<IScriptPack>());

            public Session Session { get; set; }

            public ScriptExecutionResult Execute(string code)
            {
                return Execute(code, Enumerable.Empty<string>(), Enumerable.Empty<string>(), _scriptPackSession);
            }

            protected override ScriptExecutionResult Execute(string code, Session session)
            {
                Session = session;
                return base.Execute(code, session);
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
                var session = new SessionState<Session> {Session = roslynEngine.CreateSession()};
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;
                engine.Execute(code, Enumerable.Empty<string>(), Enumerable.Empty<string>(), scriptPackSession);
                engine.Session.ShouldEqual(session.Session);
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

            [Fact]
            public void ShouldAddNewReferencesIfTheyAreProvided()
            {
                var scriptHostFactory = new Mock<IScriptHostFactory>();
                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>())).Returns((IScriptPackManager p) => new ScriptHost(p));

                var code = "var a = 0;";

                var engine = CreateTestScriptEngine(scriptHostFactory: scriptHostFactory);
                var scriptPackSession = new ScriptPackSession(new List<IScriptPack>());
                var roslynEngine = new ScriptEngine();
                var session = new SessionState<Session> { Session = roslynEngine.CreateSession()};
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;
                engine.Execute(code, new[] {"System"}, Enumerable.Empty<string>(), scriptPackSession);
                
                ((SessionState<Session>)scriptPackSession.State[RoslynScriptEngine.SessionKey]).References.Count().ShouldEqual(1);
            }

            [Fact]
            public void ShouldReturnAResult()
            {
                var engine = CreateTestScriptEngine();

                var code = "var a = 0;";

                var result = engine.Execute(code);
                result.ShouldNotBeNull();
            }

            [Fact]
            public void ShouldReturnACompilationExceptionIfTheCodeDoesNotCompile()
            {
                var engine = CreateTestScriptEngine();

                var code = "this should not compile";

                var result = engine.Execute(code);
                result.CompilationException.ShouldNotBeNull();
            }

            [Fact]
            public void ShouldNotReturnACompilationExceptionIfTheCodeDoesCompile()
            {
                var engine = CreateTestScriptEngine();

                var code = "var theNumber = 42; //this should compile";

                var result = engine.Execute(code);
                result.CompilationException.ShouldBeNull();
            }

            [Fact]
            public void ShouldReturnARuntimeExceptionIfTheCodeThrowsARuntimeException()
            {
                var engine = CreateTestScriptEngine();

                var code = "string foo = null; var i = foo.Length; //this should throw a NullReferenceException";

                var result = engine.Execute(code);
                result.RuntimeException.ShouldBeType(typeof(NullReferenceException));
            }

            [Fact]
            public void ShouldReturnAResultIfTheCodeReturnsAResult()
            {
                var engine = CreateTestScriptEngine();

                var code = "public int WhatIsTheNumber(){ return 42;} WhatIsTheNumber(); //this should return 42";

                var result = engine.Execute(code);
                result.Result.ShouldEqual(42);
            }

            [Fact]
            public void ShouldIndicateAnUnclosedState()
            {
                var engine = CreateTestScriptEngine();

                var code = "public class Foo { //this should be expecting a closing curly brace";

                var result = engine.Execute(code);
                result.ScriptIsMissingClosingChar.ShouldEqual('}');
            }
        }
    }
}
