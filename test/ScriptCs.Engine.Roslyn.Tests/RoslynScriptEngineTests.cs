using System.Linq;
using System.Reflection;

using Common.Logging;
using Moq;

using Ploeh.AutoFixture.Xunit;

using Roslyn.Scripting;
using Roslyn.Scripting.CSharp;
using ScriptCs.Contracts;
using ScriptCs.Engine.Roslyn;
using Should;
using Xunit.Extensions;

namespace ScriptCs.Tests
{
    public class RoslynScriptEngineTests
    {
        public class TheExecuteMethod 
        {
            [Theory, ScriptCsAutoData]
            public void ShouldCreateScriptHostWithContexts(
                [Frozen] Mock<IScriptHostFactory> scriptHostFactory,
                [Frozen] Mock<IScriptPack> scriptPack,
                ScriptPackSession scriptPackSession,
                [NoAutoProperties] RoslynScriptEngine engine)
            {
                // Arrange
                const string Code = "var a = 0;";

                var environment = new ScriptEnvironment(Enumerable.Empty<string>(), Enumerable.Empty<string>(), null, new string[0], Code);

                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<ScriptEnvironment>()))
                    .Returns<IScriptPackManager, ScriptEnvironment>((p, q) => new ScriptHost(p, q));

                scriptPack.Setup(p => p.Initialize(It.IsAny<IScriptPackSession>()));
                scriptPack.Setup(p => p.GetContext()).Returns((IScriptPackContext) null);

                // Act
                engine.Execute(environment, scriptPackSession);

                // Assert
                scriptHostFactory.Verify(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<ScriptEnvironment>()));
            }

            [Theory, ScriptCsAutoData]
            public void ShouldReuseExistingSessionIfProvided(
                [Frozen] Mock<IScriptHostFactory> scriptHostFactory,
                [NoAutoProperties] RoslynTestScriptEngine engine,
                ScriptPackSession scriptPackSession)
            {
                // Arrange
                const string Code = "var a = 0;";

                var environment = new ScriptEnvironment(new[] { "System" }, Enumerable.Empty<string>(), null, new string[0], Code);

                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<ScriptEnvironment>()))
                    .Returns<IScriptPackManager, ScriptEnvironment>((p, q) => new ScriptHost(p, q));

                var session = new SessionState<Session> { Session = new ScriptEngine().CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;

                // Act
                engine.Execute(environment, scriptPackSession);

                // Assert
                engine.Session.ShouldEqual(session.Session);
            }

            [Theory, ScriptCsAutoData]
            public void ShouldCreateNewSessionIfNotProvided(
                [Frozen] Mock<IScriptHostFactory> scriptHostFactory,
                [NoAutoProperties] RoslynTestScriptEngine engine,
                ScriptPackSession scriptPackSession)
            {
                // Arrange
                const string Code = "var a = 0;";

                var environment = new ScriptEnvironment(new[] { "System" }, Enumerable.Empty<string>(), null, new string[0], Code);

                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<ScriptEnvironment>()))
                    .Returns<IScriptPackManager, ScriptEnvironment>((p, q) => new ScriptHost(p, q));

                // Act
                engine.Execute(environment, scriptPackSession);

                // Assert
                engine.Session.ShouldNotBeNull();
            }

            [Theory, ScriptCsAutoData]
            public void ShouldAddNewReferencesIfTheyAreProvided(
                [Frozen] Mock<IScriptHostFactory> scriptHostFactory,
                [NoAutoProperties] RoslynTestScriptEngine engine,
                ScriptPackSession scriptPackSession)
            {
                // Arrange
                const string Code = "var a = 0;";

                var environment = new ScriptEnvironment(new[] { "System" }, Enumerable.Empty<string>(), null, new string[0], Code);

                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<ScriptEnvironment>()))
                    .Returns<IScriptPackManager, ScriptEnvironment>((p, q) => new ScriptHost(p, q));

                var session = new SessionState<Session> { Session = new ScriptEngine().CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;

                // Act
                engine.Execute(environment, scriptPackSession);
                
                // Assert
                ((SessionState<Session>)scriptPackSession.State[RoslynScriptEngine.SessionKey]).References.Count().ShouldEqual(1);
            }

            [Theory, ScriptCsAutoData]
            public void ShouldReturnAScriptResult(
                [Frozen] Mock<IScriptHostFactory> scriptHostFactory,
                [NoAutoProperties] RoslynTestScriptEngine engine,
                ScriptPackSession scriptPackSession)
            {
                // Arrange
                var code = string.Empty;

                var environment = new ScriptEnvironment(new[] { "System" }, Enumerable.Empty<string>(), null, new string[0], code);

                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<ScriptEnvironment>()))
                    .Returns<IScriptPackManager, ScriptEnvironment>((p, q) => new ScriptHost(p, q));

                var session = new SessionState<Session> { Session = new ScriptEngine().CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;

                // Act
                var result = engine.Execute(environment, scriptPackSession);

                // Assert
                result.ShouldBeType<ScriptResult>();
            }

            [Theory, ScriptCsAutoData]
            public void ShouldReturnCompileExceptionIfCodeDoesNotCompile(
                [Frozen] Mock<IScriptHostFactory> scriptHostFactory,
                [NoAutoProperties] RoslynScriptEngine engine,
                ScriptPackSession scriptPackSession)
            {
                // Arrange
                const string Code = "this shold not compile";

                var environment = new ScriptEnvironment(new[] { "System" }, Enumerable.Empty<string>(), null, new string[0], Code);

                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<ScriptEnvironment>()))
                    .Returns<IScriptPackManager, ScriptEnvironment>((p, q) => new ScriptHost(p, q));

                var session = new SessionState<Session> { Session = new ScriptEngine().CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;
                
                // Act
                var result = engine.Execute(environment, scriptPackSession);

                // Assert
                result.CompileExceptionInfo.ShouldNotBeNull();
            }

            [Theory, ScriptCsAutoData]
            public void ShouldNotReturnCompileExceptionIfCodeDoesCompile(
                [Frozen] Mock<IScriptHostFactory> scriptHostFactory,
                [NoAutoProperties] RoslynScriptEngine engine,
                ScriptPackSession scriptPackSession)
            {
                // Arrange
                const string Code = "var theNumber = 42; // this should compile";

                var environment = new ScriptEnvironment(new[] { "System" }, Enumerable.Empty<string>(), null, new string[0], Code);

                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<ScriptEnvironment>()))
                    .Returns<IScriptPackManager, ScriptEnvironment>((p, q) => new ScriptHost(p, q));

                var session = new SessionState<Session> { Session = new ScriptEngine().CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;
                
                // Act
                var result = engine.Execute(environment, scriptPackSession);

                // Assert
                result.CompileExceptionInfo.ShouldBeNull();
            }

            [Theory, ScriptCsAutoData]
            public void ShouldReturnExecuteExceptionIfCodeExecutionThrowsException(
                [Frozen] Mock<IScriptHostFactory> scriptHostFactory,
                [NoAutoProperties] RoslynScriptEngine engine,
                ScriptPackSession scriptPackSession)
            {
                // Arrange
                const string Code = "throw new System.Exception(); // this should throw an Exception";

                var environment = new ScriptEnvironment(new[] { "System" }, Enumerable.Empty<string>(), null, new string[0], Code);

                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<ScriptEnvironment>()))
                    .Returns<IScriptPackManager, ScriptEnvironment>((p, q) => new ScriptHost(p, q));

                var session = new SessionState<Session> { Session = new ScriptEngine().CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;
                
                // Act
                var result = engine.Execute(environment, scriptPackSession);

                // Assert
                result.ExecuteExceptionInfo.ShouldNotBeNull();
            }

            [Theory, ScriptCsAutoData]
            public void ShouldNotReturnExecuteExceptionIfCodeExecutionDoesNotThrowAnException(
                [Frozen] Mock<IScriptHostFactory> scriptHostFactory,
                [NoAutoProperties] RoslynScriptEngine engine,
                ScriptPackSession scriptPackSession)
            {
                // Arrange
                const string Code = "var theNumber = 42; // this should not throw an Exception";

                var environment = new ScriptEnvironment(new[] { "System" }, Enumerable.Empty<string>(), null, new string[0], Code);

                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<ScriptEnvironment>()))
                    .Returns<IScriptPackManager, ScriptEnvironment>((p, q) => new ScriptHost(p, q));

                var session = new SessionState<Session> { Session = new ScriptEngine().CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;

                // Act
                var result = engine.Execute(environment, scriptPackSession);

                // Assert
                result.ExecuteExceptionInfo.ShouldBeNull();
            }

            [Theory, ScriptCsAutoData]
            public void ShouldReturnReturnValueIfCodeExecutionReturnsValue(
                [Frozen] Mock<IScriptHostFactory> scriptHostFactory,
                [NoAutoProperties] RoslynScriptEngine engine,
                ScriptPackSession scriptPackSession)
            {
                const string Code = "\"Hello\" //this should return \"Hello\"";

                var environment = new ScriptEnvironment(new[] { "System" }, Enumerable.Empty<string>(), null, new string[0], Code);

                // Arrange
                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<ScriptEnvironment>()))
                    .Returns<IScriptPackManager, ScriptEnvironment>((p, q) => new ScriptHost(p, q));


                var session = new SessionState<Session> { Session = new ScriptEngine().CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;

                // Act
                var result = engine.Execute(environment, scriptPackSession);

                // Assert
                result.ReturnValue.ShouldEqual("Hello");
            }

            [Theory, ScriptCsAutoData]
            public void ShouldNotReturnReturnValueIfCodeExecutionDoesNotReturnValue(
                [Frozen] Mock<IScriptHostFactory> scriptHostFactory,
                [NoAutoProperties] RoslynScriptEngine engine,
                ScriptPackSession scriptPackSession)
            {
                // Arrange
                const string Code = "var theNumber = 42; //this should not return a value";

                var environment = new ScriptEnvironment(new[] { "System" }, Enumerable.Empty<string>(), null, new string[0], Code);

                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<ScriptEnvironment>()))
                    .Returns<IScriptPackManager, ScriptEnvironment>((p, q) => new ScriptHost(p, q));

                var session = new SessionState<Session> { Session = new ScriptEngine().CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;
                
                // Act
                var result = engine.Execute(environment, scriptPackSession);

                // Assert
                result.ReturnValue.ShouldBeNull();
            }

            [Theory, ScriptCsAutoData]
            public void ShouldSetIsPendingClosingCharToTrueIfCodeIsMissingCurlyBracket(
                [Frozen] Mock<IScriptHostFactory> scriptHostFactory,
                [NoAutoProperties] RoslynScriptEngine engine,
                ScriptPackSession scriptPackSession)
            {
                // Arrange
                const string Code = "class test {";

                var environment = new ScriptEnvironment(new[] {"System"}, Enumerable.Empty<string>(), null, new string[0], Code);

                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<ScriptEnvironment>()))
                    .Returns<IScriptPackManager, ScriptEnvironment>((p, q) => new ScriptHost(p, q));

                var session = new SessionState<Session> { Session = new ScriptEngine().CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;

                // Act
                var result = engine.Execute(environment, scriptPackSession);

                // Assert
                result.IsPendingClosingChar.ShouldBeTrue();
                result.ExpectingClosingChar.ShouldEqual('}');
            }

            [Theory, ScriptCsAutoData]
            public void ShouldSetIsPendingClosingCharToTrueIfCodeIsMissingSquareBracket(
                [Frozen] Mock<IScriptHostFactory> scriptHostFactory,
                [NoAutoProperties] RoslynScriptEngine engine,
                ScriptPackSession scriptPackSession)
            {
                // Arrange
                const string Code = "var x = new[1] { 1 }; var y = x[0";

                var environment = new ScriptEnvironment(new[] { "System" }, Enumerable.Empty<string>(), null, new string[0], Code);

                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<ScriptEnvironment>()))
                    .Returns<IScriptPackManager, ScriptEnvironment>((p, q) => new ScriptHost(p, q));

                var session = new SessionState<Session> { Session = new ScriptEngine().CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;

                // Act
                var result = engine.Execute(environment, scriptPackSession);

                // Assert
                result.IsPendingClosingChar.ShouldBeTrue();
                result.ExpectingClosingChar.ShouldEqual(']');
            }

            [Theory, ScriptCsAutoData]
            public void ShouldSetIsPendingClosingCharToTrueIfCodeIsMissingParenthesis(
                [Frozen] Mock<IScriptHostFactory> scriptHostFactory,
                [NoAutoProperties] RoslynScriptEngine engine,
                ScriptPackSession scriptPackSession)
            {
                // Arrange
                const string Code = "System.Diagnostics.Debug.WriteLine(\"a\"";

                var environment = new ScriptEnvironment(new[] { "System" }, Enumerable.Empty<string>(), null, new string[0], Code);

                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<ScriptEnvironment>()))
                    .Returns<IScriptPackManager, ScriptEnvironment>((p, q) => new ScriptHost(p, q));

                var session = new SessionState<Session> { Session = new ScriptEngine().CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;

                // Act
                var result = engine.Execute(environment, scriptPackSession);

                // Assert
                result.IsPendingClosingChar.ShouldBeTrue();
                result.ExpectingClosingChar.ShouldEqual(')');
            }
        }

        public class RoslynTestScriptEngine : RoslynScriptEngine
        {
            public RoslynTestScriptEngine(IScriptHostFactory scriptHostFactory, ILog logger)
                : base(scriptHostFactory, logger) { }

            public Session Session { get; private set; }

            protected override ScriptResult Execute(string code, Session session, out Assembly currentAssebly)
            {
                Session = session;
                currentAssebly = null;
                return new ScriptResult();
            }
        }
    }
}