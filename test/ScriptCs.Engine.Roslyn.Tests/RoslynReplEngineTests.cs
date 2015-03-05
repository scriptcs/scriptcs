using System.Collections.ObjectModel;
using System.Linq;
using Ploeh.AutoFixture.Xunit;
using Roslyn.Scripting;
using Roslyn.Scripting.CSharp;
using ScriptCs.Contracts;
using ScriptCs.Engine.Roslyn;
using Should;
using Xunit.Extensions;

namespace ScriptCs.Tests
{
    public class RoslynReplEngineTests
    {
        public class GetLocalVariablesMethod
        {
            [Theory, ScriptCsAutoData]
            public void ShouldReturnDeclaredVariables([NoAutoProperties]RoslynReplEngine engine, ScriptPackSession scriptPackSession)
            {
                var session = new SessionState<Session> { Session = new ScriptEngine().CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;

                engine.Execute("int x = 1;", new string[0], new AssemblyReferences(), Enumerable.Empty<string>(),
                    scriptPackSession);
                engine.Execute(@"var y = ""www"";", new string[0], new AssemblyReferences(), Enumerable.Empty<string>(),
    scriptPackSession);

                engine.GetLocalVariables(scriptPackSession).ShouldEqual(new Collection<string> { @"System.String y = www", "System.Int32 x = 1"});
            }

            [Theory, ScriptCsAutoData]
            public void ShouldReturnOnlyLastValueOfVariablesDeclaredManyTimes([NoAutoProperties]RoslynReplEngine engine, ScriptPackSession scriptPackSession)
            {
                var session = new SessionState<Session> { Session = new ScriptEngine().CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;

                engine.Execute("int x = 1;", new string[0], new AssemblyReferences(), Enumerable.Empty<string>(), scriptPackSession);
                engine.Execute("int x = 2;", new string[0], new AssemblyReferences(), Enumerable.Empty<string>(), scriptPackSession);

                engine.GetLocalVariables(scriptPackSession).ShouldEqual(new Collection<string> { "System.Int32 x = 2" });
            }

            [Theory, ScriptCsAutoData]
            public void ShouldReturn0VariablesAfterReset([NoAutoProperties]RoslynReplEngine engine, ScriptPackSession scriptPackSession)
            {
                var session = new SessionState<Session> { Session = new ScriptEngine().CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;

                engine.Execute("int x = 1;", new string[0], new AssemblyReferences(), Enumerable.Empty<string>(),
                    scriptPackSession);
                engine.Execute(@"var y = ""www"";", new string[0], new AssemblyReferences(), Enumerable.Empty<string>(),
    scriptPackSession);

                scriptPackSession.State[RoslynScriptEngine.SessionKey] = new SessionState<Session> { Session = new ScriptEngine().CreateSession() };

                engine.GetLocalVariables(scriptPackSession).ShouldBeEmpty();
            }
        }

        public class TheExecuteMethod
        {
            [Theory, ScriptCsAutoData]
            public void ShouldSetIsCompleteSubmissionToFalseIfCodeIsMissingCurlyBracket(
                [NoAutoProperties] RoslynReplEngine engine, ScriptPackSession scriptPackSession)
            {
                // Arrange
                const string Code = "class test {";

                var session = new SessionState<Session> { Session = new ScriptEngine().CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;
                var refs = new AssemblyReferences(new[] { "System" });

                // Act
                var result = engine.Execute(
                    Code, new string[0], refs, Enumerable.Empty<string>(), scriptPackSession);

                // Assert
                result.IsCompleteSubmission.ShouldBeFalse();
            }

            [Theory, ScriptCsAutoData]
            public void ShouldSetIsCompleteSubmissionToFalseIfCodeIsMissingParenthesis(
                [NoAutoProperties] RoslynReplEngine engine,
                ScriptPackSession scriptPackSession)
            {
                // Arrange
                const string Code = "System.Diagnostics.Debug.WriteLine(\"a\"";

                var session = new SessionState<Session> { Session = new ScriptEngine().CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;
                var refs = new AssemblyReferences(new[] { "System" });

                // Act
                var result = engine.Execute(Code, new string[0], refs, Enumerable.Empty<string>(), scriptPackSession);

                // Assert
                result.IsCompleteSubmission.ShouldBeFalse();
            }

            [Theory, ScriptCsAutoData]
            public void ShouldSetIsCompleteSubmissionToFalseIfCodeIsMissingSquareBracket(
                [NoAutoProperties] RoslynReplEngine engine,
                ScriptPackSession scriptPackSession)
            {
                // Arrange
                const string Code = "var x = new[1] { 1 }; var y = x[0";

                var session = new SessionState<Session> { Session = new ScriptEngine().CreateSession() };
                scriptPackSession.State[RoslynScriptEngine.SessionKey] = session;
                var refs = new AssemblyReferences(new[] { "System" });

                // Act
                var result = engine.Execute(Code, new string[0], refs, Enumerable.Empty<string>(), scriptPackSession);

                // Assert
                result.IsCompleteSubmission.ShouldBeFalse();
            }
        }
    }
}