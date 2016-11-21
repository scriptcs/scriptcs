using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.CSharp;
using Ploeh.AutoFixture.Xunit;
using ScriptCs.Contracts;
using ScriptCs.Engine.Roslyn;
using Should;
using Xunit.Extensions;

namespace ScriptCs.Tests
{
    public class CSharpReplEngineTests
    {
        public class GetLocalVariablesMethod
        {
            [Theory, ScriptCsAutoData]
            public void ShouldReturnDeclaredVariables([NoAutoProperties]CSharpReplEngine engine, ScriptPackSession scriptPackSession)
            {
                var session = new SessionState<ScriptState> { Session = CSharpScript.Run("") };
                scriptPackSession.State[CommonScriptEngine.SessionKey] = session;

                engine.Execute("int x = 1;", new string[0], new AssemblyReferences(), Enumerable.Empty<string>(),
                    scriptPackSession);
                engine.Execute(@"var y = ""www"";", new string[0], new AssemblyReferences(), Enumerable.Empty<string>(),
    scriptPackSession);

                engine.GetLocalVariables(scriptPackSession).ShouldEqual(new Collection<string> { "System.Int32 x", "System.String y" });
            }

            [Theory, ScriptCsAutoData]
            public void ShouldReturnOnlyLastValueOfVariablesDeclaredManyTimes([NoAutoProperties]CSharpReplEngine engine, ScriptPackSession scriptPackSession)
            {
                var session = new SessionState<ScriptState> { Session = CSharpScript.Run("") };
                scriptPackSession.State[CommonScriptEngine.SessionKey] = session;

                engine.Execute("int x = 1;", new string[0], new AssemblyReferences(), Enumerable.Empty<string>(), scriptPackSession);
                engine.Execute("int x = 2;", new string[0], new AssemblyReferences(), Enumerable.Empty<string>(), scriptPackSession);

                engine.GetLocalVariables(scriptPackSession).ShouldEqual(new Collection<string> { "System.Int32 x" });
            }

            [Theory, ScriptCsAutoData]
            public void ShouldReturn0VariablesAfterReset([NoAutoProperties]CSharpReplEngine engine, ScriptPackSession scriptPackSession)
            {
                var session = new SessionState<ScriptState> { Session = CSharpScript.Run("") };
                scriptPackSession.State[CommonScriptEngine.SessionKey] = session;

                engine.Execute("int x = 1;", new string[0], new AssemblyReferences(), Enumerable.Empty<string>(),
                    scriptPackSession);
                engine.Execute(@"var y = ""www"";", new string[0], new AssemblyReferences(), Enumerable.Empty<string>(),
    scriptPackSession);

                scriptPackSession.State[CommonScriptEngine.SessionKey] = new SessionState<ScriptState> { Session = CSharpScript.Run("") };

                engine.GetLocalVariables(scriptPackSession).ShouldBeEmpty();
            }
        }

        public class TheExecuteMethod
        {
            [Theory, ScriptCsAutoData]
            public void ShouldSetIsCompleteSubmissionToFalseIfCodeIsMissingCurlyBracket(
                [NoAutoProperties] CSharpReplEngine engine, ScriptPackSession scriptPackSession)
            {
                // Arrange
                const string Code = "class test {";

                var session = new SessionState<ScriptState> { Session = CSharpScript.Run("") };
                scriptPackSession.State[CommonScriptEngine.SessionKey] = session;
                var refs = new AssemblyReferences(new[] { "System" });

                // Act
                var result = engine.Execute(
                    Code, new string[0], refs, Enumerable.Empty<string>(), scriptPackSession);

                // Assert
                result.IsCompleteSubmission.ShouldBeFalse();
            }

            [Theory, ScriptCsAutoData]
            public void ShouldSetIsCompleteSubmissionToFalseIfCodeIsMissingParenthesis(
                [NoAutoProperties] CSharpReplEngine engine,
                ScriptPackSession scriptPackSession)
            {
                // Arrange
                const string Code = "System.Diagnostics.Debug.WriteLine(\"a\"";

                var session = new SessionState<ScriptState> { Session = CSharpScript.Run("") };
                scriptPackSession.State[CommonScriptEngine.SessionKey] = session;
                var refs = new AssemblyReferences(new[] { "System" });

                // Act
                var result = engine.Execute(Code, new string[0], refs, Enumerable.Empty<string>(), scriptPackSession);

                // Assert
                result.IsCompleteSubmission.ShouldBeFalse();
            }

            [Theory, ScriptCsAutoData]
            public void ShouldSetIsCompleteSubmissionToFalseIfCodeIsMissingSquareBracket(
                [NoAutoProperties] CSharpReplEngine engine,
                ScriptPackSession scriptPackSession)
            {
                // Arrange
                const string Code = "var x = new[1] { 1 }; var y = x[0";

                var session = new SessionState<ScriptState> { Session  = CSharpScript.Run("") };
                scriptPackSession.State[CommonScriptEngine.SessionKey] = session;
                var refs = new AssemblyReferences(new[] { "System" });

                // Act
                var result = engine.Execute(Code, new string[0], refs, Enumerable.Empty<string>(), scriptPackSession);

                // Assert
                result.IsCompleteSubmission.ShouldBeFalse();
            }
        }
    }
}
