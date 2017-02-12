extern alias MonoCSharp;

using System.Collections.ObjectModel;
using System.Linq;
using MonoCSharp::Mono.CSharp;
using Moq;
using Ploeh.AutoFixture.Xunit;
using ScriptCs.Contracts;
using ScriptCs.Tests;
using Should;
using Xunit.Extensions;

namespace ScriptCs.Engine.Mono.Tests
{
    public class MonoScriptEngineTests
    {
        public class GetLocalVariablesMethod
        {
            [Theory, ScriptCsAutoData]
            public void ShouldReturnDeclaredVariables([NoAutoProperties]MonoScriptEngine engine, ScriptPackSession scriptPackSession)
            {
                var session = new SessionState<Evaluator> { Session = new Evaluator(new CompilerContext(new CompilerSettings(), new ConsoleReportPrinter())) };
                scriptPackSession.State[MonoScriptEngine.SessionKey] = session;

                engine.Execute("int x = 1;", new string[0], new AssemblyReferences(), Enumerable.Empty<string>(),
                    scriptPackSession);
                engine.Execute(@"var y = ""www"";", new string[0], new AssemblyReferences(), Enumerable.Empty<string>(),
    scriptPackSession);

                engine.GetLocalVariables(scriptPackSession).ShouldEqual(new Collection<string> { "int x = 1", @"string y = ""www""" });
            }

            [Theory, ScriptCsAutoData]
            public void ShouldReturnOnlyLastValueOfVariablesDeclaredManyTimes([NoAutoProperties]MonoScriptEngine engine, ScriptPackSession scriptPackSession)
            {
                var session = new SessionState<Evaluator> { Session = new Evaluator(new CompilerContext(new CompilerSettings(), new ConsoleReportPrinter())) };
                scriptPackSession.State[MonoScriptEngine.SessionKey] = session;

                engine.Execute("int x = 1;", new string[0], new AssemblyReferences(), Enumerable.Empty<string>(), scriptPackSession);
                engine.Execute("int x = 2;", new string[0], new AssemblyReferences(), Enumerable.Empty<string>(), scriptPackSession);

                engine.GetLocalVariables(scriptPackSession).ShouldEqual(new Collection<string> { "int x = 2" });
            }

            [Theory, ScriptCsAutoData]
            public void ShouldReturn0VariablesAfterReset([NoAutoProperties]MonoScriptEngine engine, ScriptPackSession scriptPackSession)
            {
                var session = new SessionState<Evaluator> { Session = new Evaluator(new CompilerContext(new CompilerSettings(), new ConsoleReportPrinter())) };
                scriptPackSession.State[MonoScriptEngine.SessionKey] = session;

                engine.Execute("int x = 1;", new string[0], new AssemblyReferences(), Enumerable.Empty<string>(),
                    scriptPackSession);
                engine.Execute(@"var y = ""www"";", new string[0], new AssemblyReferences(), Enumerable.Empty<string>(),
    scriptPackSession);

                scriptPackSession.State[MonoScriptEngine.SessionKey] = new SessionState<Evaluator> { Session = new Evaluator(new CompilerContext(new CompilerSettings(), new ConsoleReportPrinter())) };

                engine.GetLocalVariables(scriptPackSession).ShouldBeEmpty();
            }
        }

        public class TheExecuteMethod
        {
            private IConsole _console = new Mock<IConsole>().Object;
            private IObjectSerializer _serializer = new Mock<IObjectSerializer>().Object;
            private Printers _printers;
            public TheExecuteMethod()
            {
                _printers =  new Printers(_serializer);
            }

            [Theory, ScriptCsAutoData]
            public void ShouldCreateScriptHostWithContexts(
                [Frozen] Mock<IScriptHostFactory> scriptHostFactory,
                [Frozen] Mock<IScriptPack> scriptPack,
                ScriptPackSession scriptPackSession,
                [NoAutoProperties] MonoScriptEngine engine)
            {
                // Arrange
                const string Code = "var a = 0;";

                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<string[]>()))
                    .Returns<IScriptPackManager, string[]>((p, q) => new ScriptHost(p, new ScriptEnvironment(q, _console, _printers)));

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
                [NoAutoProperties] MonoTestScriptEngine engine,
                ScriptPackSession scriptPackSession)
            {
                // Arrange
                const string Code = "var a = 0;";

                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<string[]>()))
                    .Returns<IScriptPackManager, string[]>((p, q) => new ScriptHost(p, new ScriptEnvironment(q, _console, _printers)));

                var session = new SessionState<Evaluator> { Session = new Evaluator(new CompilerContext(new CompilerSettings(), new ConsoleReportPrinter())) };
                scriptPackSession.State[MonoScriptEngine.SessionKey] = session;

                // Act
                engine.Execute(Code, new string[0], new AssemblyReferences(), Enumerable.Empty<string>(), scriptPackSession);

                // Assert
                engine.Session.ShouldEqual(session.Session);
            }

            [Theory, ScriptCsAutoData]
            public void ShouldCreateNewSessionIfNotProvided(
                [Frozen] Mock<IScriptHostFactory> scriptHostFactory,
                [NoAutoProperties] MonoTestScriptEngine engine,
                ScriptPackSession scriptPackSession)
            {
                // Arrange
                const string Code = "var a = 0;";

                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<string[]>()))
                    .Returns<IScriptPackManager, string[]>((p, q) => new ScriptHost(p, new ScriptEnvironment(q, _console, _printers)));

                // Act
                engine.Execute(Code, new string[0], new AssemblyReferences(), Enumerable.Empty<string>(), scriptPackSession);

                // Assert
                engine.Session.ShouldNotBeNull();
            }

            [Theory, ScriptCsAutoData]
            public void ShouldAddNewReferencesIfTheyAreProvided(
                [Frozen] Mock<IScriptHostFactory> scriptHostFactory,
                [NoAutoProperties] MonoTestScriptEngine engine,
                ScriptPackSession scriptPackSession)
            {
                // Arrange
                const string Code = "var a = 0;";

                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<string[]>()))
                    .Returns<IScriptPackManager, string[]>((p, q) => new ScriptHost(p, new ScriptEnvironment(q, _console, _printers)));

                var session = new SessionState<Evaluator> { Session = new Evaluator(new CompilerContext(new CompilerSettings(), new ConsoleReportPrinter())) };
                scriptPackSession.State[MonoScriptEngine.SessionKey] = session;
                var refs = new AssemblyReferences(new[] { "System" });

                // Act
                engine.Execute(Code, new string[0], refs, Enumerable.Empty<string>(), scriptPackSession);

                // Assert
                ((SessionState<Evaluator>)scriptPackSession.State[MonoScriptEngine.SessionKey]).References.Paths.Count().ShouldEqual(1);
            }

            [Theory, ScriptCsAutoData]
            public void ShouldReturnAScriptResult(
                [Frozen] Mock<IScriptHostFactory> scriptHostFactory,
                [NoAutoProperties] MonoScriptEngine engine,
                ScriptPackSession scriptPackSession)
            {
                // Arrange
                var code = string.Empty;

                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<string[]>()))
                    .Returns<IScriptPackManager, string[]>((p, q) => new ScriptHost(p, new ScriptEnvironment(q, _console, _printers)));

                var session = new SessionState<Evaluator> { Session = new Evaluator(new CompilerContext(new CompilerSettings(), new ConsoleReportPrinter())) };
                scriptPackSession.State[MonoScriptEngine.SessionKey] = session;
                var refs = new AssemblyReferences(new[] { "System" });

                // Act
                var result = engine.Execute(code, new string[0], refs, Enumerable.Empty<string>(), scriptPackSession);

                // Assert
                result.ShouldBeType<ScriptResult>();
            }

            // Need to get the compiler ex from mono.
            // Currently the ConsoleReportWriter is swallowing the ex
            /*
            [Theory, ScriptCsAutoData]
            public void ShouldReturnCompileExceptionIfCodeDoesNotCompile(
                [Frozen] Mock<IScriptHostFactory> scriptHostFactory,
                [NoAutoProperties] MonoScriptEngine engine,
                ScriptPackSession scriptPackSession)
            {
                // Arrange
                const string Code = "this shold not compile";

                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<string[]>()))
                    .Returns<IScriptPackManager, string[]>((p, q) => new ScriptHost(p, new ScriptEnvironment(q)));

                var session = new SessionState<Evaluator> { Session = new Evaluator(new CompilerContext( new CompilerSettings(), new ConsoleReportPrinter())) };
                scriptPackSession.State[MonoScriptEngine.SessionKey] = session;
                var refs = new AssemblyReferences();
                refs.PathReferences.Add("System");

                // Act
                var result = engine.Execute(Code, new string[0], refs, Enumerable.Empty<string>(), scriptPackSession);

                System.Threading.Thread.Sleep(1000);

                // Assert
                result.CompileExceptionInfo.ShouldNotBeNull();
            }
            */

            [Theory, ScriptCsAutoData]
            public void ShouldNotReturnCompileExceptionIfCodeDoesCompile(
                [Frozen] Mock<IScriptHostFactory> scriptHostFactory,
                [NoAutoProperties] MonoScriptEngine engine,
                ScriptPackSession scriptPackSession)
            {
                // Arrange
                const string Code = "var theNumber = 42; //this should compile";

                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<string[]>()))
                    .Returns<IScriptPackManager, string[]>((p, q) => new ScriptHost(p, new ScriptEnvironment(q, _console, _printers)));

                var session = new SessionState<Evaluator> { Session = new Evaluator(new CompilerContext(new CompilerSettings(), new ConsoleReportPrinter())) };
                scriptPackSession.State[MonoScriptEngine.SessionKey] = session;
                var refs = new AssemblyReferences(new[] { "System" });

                // Act
                var result = engine.Execute(Code, new string[0], refs, Enumerable.Empty<string>(), scriptPackSession);

                // Assert
                result.ShouldBeType<ScriptResult>();
                result.CompileExceptionInfo.ShouldBeNull();
                result.ExecuteExceptionInfo.ShouldBeNull();
            }

            [Theory, ScriptCsAutoData]
            public void ShouldReturnExecuteExceptionIfCodeExecutionThrowsException(
                [Frozen] Mock<IScriptHostFactory> scriptHostFactory,
                [NoAutoProperties] MonoScriptEngine engine,
                ScriptPackSession scriptPackSession)
            {
                // Arrange
                const string Code = "throw new System.Exception(); //this should throw an Exception";

                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<string[]>()))
                    .Returns<IScriptPackManager, string[]>((p, q) => new ScriptHost(p, new ScriptEnvironment(q, _console, _printers)));

                var session = new SessionState<Evaluator> { Session = new Evaluator(new CompilerContext(new CompilerSettings(), new ConsoleReportPrinter())) };
                scriptPackSession.State[MonoScriptEngine.SessionKey] = session;
                var refs = new AssemblyReferences(new[] { "System" });

                // Act
                var result = engine.Execute(Code, new string[0], refs, Enumerable.Empty<string>(), scriptPackSession);

                // Assert
                result.ExecuteExceptionInfo.ShouldNotBeNull();
            }

            [Theory, ScriptCsAutoData]
            public void ShouldNotReturnExecuteExceptionIfCodeExecutionDoesNotThrowAnException(
                [Frozen] Mock<IScriptHostFactory> scriptHostFactory,
                [NoAutoProperties] MonoScriptEngine engine,
                ScriptPackSession scriptPackSession)
            {
                // Arrange
                const string Code = "var theNumber = 42; //this should not throw an Exception";

                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<string[]>()))
                    .Returns<IScriptPackManager, string[]>((p, q) => new ScriptHost(p, new ScriptEnvironment(q, _console, _printers)));

                var session = new SessionState<Evaluator> { Session = new Evaluator(new CompilerContext(new CompilerSettings(), new ConsoleReportPrinter())) };
                scriptPackSession.State[MonoScriptEngine.SessionKey] = session;
                var refs = new AssemblyReferences(new[] { "System" });

                // Act
                var result = engine.Execute(Code, new string[0], refs, Enumerable.Empty<string>(), scriptPackSession);

                // Assert
                result.ExecuteExceptionInfo.ShouldBeNull();
            }

            [Theory, ScriptCsAutoData]
            public void ShouldReturnReturnValueIfCodeExecutionReturnsValue(
                [Frozen] Mock<IScriptHostFactory> scriptHostFactory,
                [NoAutoProperties] MonoScriptEngine engine,
                ScriptPackSession scriptPackSession)
            {
                // Mono doesn't support comments after evals as well as Roslyn
                const string Code = "\"Hello\"";  //this should return \"Hello\"";

                // Arrange
                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<string[]>()))
                    .Returns<IScriptPackManager, string[]>((p, q) => new ScriptHost(p, new ScriptEnvironment(q, _console, _printers)));

                var session = new SessionState<Evaluator> { Session = new Evaluator(new CompilerContext(new CompilerSettings(), new ConsoleReportPrinter())) };
                scriptPackSession.State[MonoScriptEngine.SessionKey] = session;
                var refs = new AssemblyReferences(new[] { "System" });

                // Act
                var result = engine.Execute(Code, new string[0], refs, Enumerable.Empty<string>(), scriptPackSession);

                // Assert
                result.ReturnValue.ShouldEqual("Hello");
            }

            [Theory, ScriptCsAutoData]
            public void ShouldNotReturnReturnValueIfCodeExecutionDoesNotReturnValue(
                [Frozen] Mock<IScriptHostFactory> scriptHostFactory,
                [NoAutoProperties] MonoScriptEngine engine,
                ScriptPackSession scriptPackSession)
            {
                // Arrange
                const string Code = "var theNumber = 42; //this should not return a value";

                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<string[]>()))
                    .Returns<IScriptPackManager, string[]>((p, q) => new ScriptHost(p, new ScriptEnvironment(q, _console, _printers)));

                var session = new SessionState<Evaluator> { Session = new Evaluator(new CompilerContext(new CompilerSettings(), new ConsoleReportPrinter())) };
                scriptPackSession.State[MonoScriptEngine.SessionKey] = session;
                var refs = new AssemblyReferences(new[] { "System" });

                // Act
                var result = engine.Execute(Code, new string[0], refs, Enumerable.Empty<string>(), scriptPackSession);

                // Assert
                result.ReturnValue.ShouldBeNull();
            }

            [Theory, ScriptCsAutoData]
            public void ShouldInitializeScriptLibraryWrapperHost(
                [Frozen] Mock<IScriptHostFactory> scriptHostFactory,
                Mock<IScriptPackManager> manager,
                [NoAutoProperties] MonoScriptEngine engine,
                ScriptPackSession scriptPackSession
                )
            {
                // Arrange
                const string Code = "var theNumber = 42; //this should compile";

                var refs = new AssemblyReferences(new[] { "System" });

                scriptHostFactory.Setup(s => s.CreateScriptHost(It.IsAny<IScriptPackManager>(), It.IsAny<string[]>()))
                    .Returns(new ScriptHost(manager.Object, null));

                // Act
                engine.Execute(Code, new string[0], refs, Enumerable.Empty<string>(), scriptPackSession);

                // Assert
                ScriptLibraryWrapper.ScriptHost.ShouldNotEqual(null);
            }
        }

        public class MonoTestScriptEngine : MonoScriptEngine
        {
            public MonoTestScriptEngine(IScriptHostFactory scriptHostFactory, TestLogProvider logProvider)
                : base(scriptHostFactory, logProvider)
            {
            }

            public Evaluator Session { get; private set; }

            protected override ScriptResult Execute(string code, Evaluator session)
            {
                Session = session;
                return ScriptResult.Empty;
            }
        }
    }
}
