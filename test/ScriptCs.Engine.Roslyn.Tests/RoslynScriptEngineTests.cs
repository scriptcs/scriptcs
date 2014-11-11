using System.Linq;
using System.Reflection;
using Common.Logging;
using Moq;
using Ploeh.AutoFixture.Xunit;
using Roslyn.Compilers;
using Roslyn.Scripting;
using Roslyn.Scripting.CSharp;
using ScriptCs.Contracts;
using ScriptCs.Engine.Roslyn;
using Should;
using Xunit;
using Xunit.Extensions;

namespace ScriptCs.Tests
{
    public class RoslynScriptEngineTests
    {
        public class Constructor
        {
            [Fact]
            public void ShouldAddReferenceToCore()
            {
                var engine = new RoslynTestScriptEngine(new Mock<IScriptHostFactory>().Object, new Mock<ILog>().Object);
                engine.Engine.GetReferences().Where(x => x.Display.EndsWith("ScriptCs.Core.dll")).Count().ShouldEqual(1);
            }
        }

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

                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<string[]>()))
                    .Returns<IScriptPackManager, string[]>((p, q) => new ScriptHost(p, new ScriptEnvironment(q)));

                scriptPack.Setup(p => p.Initialize(It.IsAny<IScriptPackSession>()));
                scriptPack.Setup(p => p.GetContext()).Returns((IScriptPackContext)null);

                // Act
                engine.Execute(Code, new string[0], new AssemblyReferences(), Enumerable.Empty<string>(), scriptPackSession);

                // Assert
                scriptHostFactory.Verify(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<string[]>()));
            }

            [Theory, ScriptCsAutoData]
            public void ShouldReuseExistingSessionIfProvided(
                [Frozen] Mock<IScriptHostFactory> scriptHostFactory,
                [NoAutoProperties] RoslynTestScriptEngine engine,
                ScriptPackSession scriptPackSession)
            {
                // Arrange
                const string Code = "var a = 0;";

                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<string[]>()))
                    .Returns<IScriptPackManager, string[]>((p, q) => new ScriptHost(p, new ScriptEnvironment(q)));

                var session = new SessionState<Session> { Session = new ScriptEngine().CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;

                // Act
                engine.Execute(Code, new string[0], new AssemblyReferences(), Enumerable.Empty<string>(), scriptPackSession);

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

                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<string[]>()))
                    .Returns<IScriptPackManager, string[]>((p, q) => new ScriptHost(p, new ScriptEnvironment(q)));

                // Act
                engine.Execute(Code, new string[0], new AssemblyReferences(), Enumerable.Empty<string>(), scriptPackSession);

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

                var session = new SessionState<Session> { Session = new ScriptEngine().CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;
                var refs = new AssemblyReferences();
                refs.PathReferences.Add("System");

                // Act
                engine.Execute(Code, new string[0], refs, Enumerable.Empty<string>(), scriptPackSession);

                // Assert
                ((SessionState<Session>)scriptPackSession.State[RoslynScriptEngine.SessionKey]).References.PathReferences.Count().ShouldEqual(1);
            }

            [Theory, ScriptCsAutoData]
            public void ShouldReturnAScriptResult(
                [Frozen] Mock<IScriptHostFactory> scriptHostFactory,
                [NoAutoProperties] RoslynTestScriptEngine engine,
                ScriptPackSession scriptPackSession)
            {
                // Arrange
                var code = string.Empty;

                var session = new SessionState<Session> { Session = new ScriptEngine().CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;
                var refs = new AssemblyReferences();
                refs.PathReferences.Add("System");

                // Act
                var result = engine.Execute(code, new string[0], refs, Enumerable.Empty<string>(), scriptPackSession);

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

                var session = new SessionState<Session> { Session = new ScriptEngine().CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;
                var refs = new AssemblyReferences();
                refs.PathReferences.Add("System");

                // Act
                var result = engine.Execute(Code, new string[0], refs, Enumerable.Empty<string>(), scriptPackSession);

                // Assert
                result.CompileExceptionInfo.ShouldNotBeNull();
            }

            [Theory, ScriptCsAutoData]
            public void ShouldReturnInvalidNamespacesIfCS0241Encountered(
                [Frozen] Mock<IScriptHostFactory> scriptHostFactory,
                [NoAutoProperties] RoslynScriptEngine engine,
                ScriptPackSession scriptPackSession)
            {
                var session = new SessionState<Session> { Session = new ScriptEngine().CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;

                // Act
                var result = engine.Execute(string.Empty, new string[0], new AssemblyReferences(), new[] {"foo"}, scriptPackSession);

                // Assert
                result.CompileExceptionInfo.ShouldNotBeNull();
                result.InvalidNamespaces.ShouldContain("foo");
            }

            [Theory, ScriptCsAutoData]
            public void ShouldRemoveInvalidNamespacesFromSessionStateAndEngine(
                [Frozen] Mock<IScriptHostFactory> scriptHostFactory,
                [NoAutoProperties] RoslynScriptEngine engine,
                ScriptPackSession scriptPackSession)
            {
                var session = new SessionState<Session> { Session = new ScriptEngine().CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;

                // Act
                engine.Execute(string.Empty, new string[0], new AssemblyReferences(), new[] { "System", "foo" }, scriptPackSession);

                // Assert
                session.Namespaces.ShouldNotBeEmpty();
                session.Namespaces.ShouldNotContain("foo");
                var pendingNamespacesField = session.Session.GetType().GetField("pendingNamespaces", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                var pendingNamespacesValue = (ReadOnlyArray<string>)pendingNamespacesField.GetValue(session.Session);
                pendingNamespacesValue.AsEnumerable().ShouldNotBeEmpty();
                pendingNamespacesValue.AsEnumerable().ShouldNotContain("foo");
            }

            [Theory, ScriptCsAutoData]
            public void ShouldNotReturnCompileExceptionIfCodeDoesCompile(
                [Frozen] Mock<IScriptHostFactory> scriptHostFactory,
                [NoAutoProperties] RoslynScriptEngine engine,
                ScriptPackSession scriptPackSession)
            {
                // Arrange
                const string Code = "var theNumber = 42; //this should compile";

                var session = new SessionState<Session> { Session = new ScriptEngine().CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;
                var refs = new AssemblyReferences();
                refs.PathReferences.Add("System");

                // Act
                var result = engine.Execute(Code, new string[0], refs, Enumerable.Empty<string>(), scriptPackSession);

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
                const string Code = "throw new System.Exception(); //this should throw an Exception";

                var session = new SessionState<Session> { Session = new ScriptEngine().CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;
                var refs = new AssemblyReferences();
                refs.PathReferences.Add("System");

                // Act
                var result = engine.Execute(Code, new string[0], refs, Enumerable.Empty<string>(), scriptPackSession);

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
                const string Code = "var theNumber = 42; //this should not throw an Exception";

                var session = new SessionState<Session> { Session = new ScriptEngine().CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;
                var refs = new AssemblyReferences();
                refs.PathReferences.Add("System");

                // Act
                var result = engine.Execute(Code, new string[0], refs, Enumerable.Empty<string>(), scriptPackSession);

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

                var session = new SessionState<Session> { Session = new ScriptEngine().CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;
                var refs = new AssemblyReferences();
                refs.PathReferences.Add("System");

                // Act
                var result = engine.Execute(Code, new string[0], refs, Enumerable.Empty<string>(), scriptPackSession);

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

                var session = new SessionState<Session> { Session = new ScriptEngine().CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;
                var refs = new AssemblyReferences();
                refs.PathReferences.Add("System");

                // Act
                var result = engine.Execute(Code, new string[0], refs, Enumerable.Empty<string>(), scriptPackSession);

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

                var session = new SessionState<Session> { Session = new ScriptEngine().CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;
                var refs = new AssemblyReferences();
                refs.PathReferences.Add("System");

                // Act
                var result = engine.Execute(
                    Code, new string[0], refs, Enumerable.Empty<string>(), scriptPackSession);

                // Assert
                result.IsCompleteSubmission.ShouldBeFalse();
            }

            [Theory, ScriptCsAutoData]
            public void ShouldNotMarkSubmissionsAsIncompleteWhenRunningScript(
                [Frozen] Mock<IScriptHostFactory> scriptHostFactory,
                [NoAutoProperties] RoslynScriptEngine engine,
                ScriptPackSession scriptPackSession)
            {
                // Arrange
                const string Code = "class test {";

                var session = new SessionState<Session> { Session = new ScriptEngine().CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;
                var refs = new AssemblyReferences();
                refs.PathReferences.Add("System");
                engine.FileName = "test.csx";

                // Act
                var result = engine.Execute(
                    Code, new string[0], refs, Enumerable.Empty<string>(), scriptPackSession);

                // Assert
                result.IsCompleteSubmission.ShouldBeTrue();
                result.CompileExceptionInfo.ShouldNotBeNull();
            }

            [Theory, ScriptCsAutoData]
            public void ShouldSetIsPendingClosingCharToTrueIfCodeIsMissingSquareBracket(
                [Frozen] Mock<IScriptHostFactory> scriptHostFactory,
                [NoAutoProperties] RoslynScriptEngine engine,
                ScriptPackSession scriptPackSession)
            {
                // Arrange
                const string Code = "var x = new[1] { 1 }; var y = x[0";

                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<string[]>()))
                    .Returns<IScriptPackManager, string[]>((p, q) => new ScriptHost(p, new ScriptEnvironment(q)));

                var session = new SessionState<Session> { Session = new ScriptEngine().CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;
                var refs = new AssemblyReferences();
                refs.PathReferences.Add("System");

                // Act
                var result = engine.Execute(Code, new string[0], refs, Enumerable.Empty<string>(), scriptPackSession);

                // Assert
                result.IsCompleteSubmission.ShouldBeFalse();
            }

            [Theory, ScriptCsAutoData]
            public void ShouldSetIsPendingClosingCharToTrueIfCodeIsMissingParenthesis(
                [Frozen] Mock<IScriptHostFactory> scriptHostFactory,
                [NoAutoProperties] RoslynScriptEngine engine,
                ScriptPackSession scriptPackSession)
            {
                // Arrange
                const string Code = "System.Diagnostics.Debug.WriteLine(\"a\"";

                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<string[]>()))
                    .Returns<IScriptPackManager, string[]>((p, q) => new ScriptHost(p, new ScriptEnvironment(q)));

                var session = new SessionState<Session> { Session = new ScriptEngine().CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;
                var refs = new AssemblyReferences();
                refs.PathReferences.Add("System");

                // Act
                var result = engine.Execute(Code, new string[0], refs, Enumerable.Empty<string>(), scriptPackSession);

                // Assert
                result.IsCompleteSubmission.ShouldBeFalse();
            }
        }

            [Theory, ScriptCsAutoData]
            public void ShouldCompileWhenUsingClassesFromAPassedAssemblyInstance(
                [Frozen] Mock<IScriptHostFactory> scriptHostFactory,
                [NoAutoProperties] RoslynScriptEngine engine,
                ScriptPackSession scriptPackSession)
            {
                // Arrange
                const string Code = "var x = new ScriptCs.Tests.TestMarkerClass();";

                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<string[]>()))
                    .Returns<IScriptPackManager, ScriptEnvironment>((p, q) => new ScriptHost(p, q));

                var session = new SessionState<Session> { Session = new ScriptEngine().CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;
                var refs = new AssemblyReferences();
                refs.PathReferences.Add("System");
                refs.Assemblies.Add(Assembly.GetExecutingAssembly());

                // Act
                var result = engine.Execute(Code, new string[0], refs, Enumerable.Empty<string>(), scriptPackSession);

                // Assert
                result.CompileExceptionInfo.ShouldBeNull();
                result.ExecuteExceptionInfo.ShouldBeNull();
            }

        public class RoslynTestScriptEngine : RoslynScriptEngine
        {
            public RoslynTestScriptEngine(IScriptHostFactory scriptHostFactory, ILog logger)
                : base(scriptHostFactory, logger)
            {
            }

            public Session Session { get; private set; }

            protected override ScriptResult Execute(string code, Session session)
            {
                Session = session;
                return ScriptResult.Empty;
            }

            internal ScriptEngine Engine {
                get { return ScriptEngine; }
            }
        }
    }

    public class TestMarkerClass { }
}