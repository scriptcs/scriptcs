using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.Scripting;
using Moq;
using ScriptCs.Contracts;
using ScriptCs.Engine.Roslyn;
using ScriptCs.Exceptions;
using Should;
using Xunit;

namespace ScriptCs.Tests
{
    public class CSharpScriptInMemoryEngineTests
    {
        public class TheExecuteMethod
        {
            private IConsole _console = new Mock<IConsole>().Object;
            private IObjectSerializer _serializer = new Mock<IObjectSerializer>().Object;
            private Printers _printers;

            public TheExecuteMethod()
            {
                _printers =  new Printers(_serializer);
            }

            [Fact]
            public void ShouldExposeExceptionThrownByScriptWhenErrorOccurs()
            {
                var scriptEngine = new CSharpScriptInMemoryEngine(new ScriptHostFactory(_console, _printers), new TestLogProvider());
                // Arrange
                var lines = new List<string>
                {
                    "using System;",
                    @"throw new InvalidOperationException(""InvalidOperationExceptionMessage."");"
                };

                var code = string.Join(Environment.NewLine, lines);
                var session = new ScriptPackSession(Enumerable.Empty<IScriptPack>(), new string[0]);

                // Act
                var result = scriptEngine.Execute(code, new string[0], new AssemblyReferences(), Enumerable.Empty<string>(),
                        session);

                // Assert
                var exception = Assert.Throws<InvalidOperationException>(() => result.ExecuteExceptionInfo.Throw());
                exception.StackTrace.ShouldContain("Submission#0");
                exception.Message.ShouldContain("InvalidOperationExceptionMessage");
            }

            [Fact]
            public void ShouldExposeExceptionThrownByCompilation()
            {
                var scriptEngine = new CSharpScriptInMemoryEngine(new ScriptHostFactory(_console, _printers), new TestLogProvider());

                // Arrange
                var lines = new List<string>
                {
                    "Sysasdasdasdtem;"
                };

                var code = string.Join(Environment.NewLine, lines);
                var session = new ScriptPackSession(Enumerable.Empty<IScriptPack>(), new string[0]);

                // Act
                var result = scriptEngine.Execute(code, new string[0], new AssemblyReferences(), Enumerable.Empty<string>(),
                        session);

                // Assert
                var exception = Assert.Throws<ScriptCompilationException>(() => result.CompileExceptionInfo.Throw());
                exception.Message.ShouldContain("error CS0103: The name 'Sysasdasdasdtem' does not exist in the current context");
            }
        }
    }
}
