using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Roslyn.Compilers;
using ScriptCs.Contracts;
using ScriptCs.Engine.Roslyn;
using ScriptCs.Exceptions;
using ScriptCs.Contracts.Logging;
using Xunit;

namespace ScriptCs.Tests
{
    using Should;

    public class RoslynScriptInMemoryEngineTests
    {
        public class TheExecuteMethod
        {
            [Fact]
            public void ShouldExposeExceptionThrownByScriptWhenErrorOccurs()
            {
                var scriptEngine = new RoslynScriptInMemoryEngine(new ScriptHostFactory(), new TestLogProvider());
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
                var scriptEngine = new RoslynScriptInMemoryEngine(new ScriptHostFactory(), new TestLogProvider());

                // Arrange
                var lines = new List<string>
                {
                    "using Sysasdasdasdtem;"
                };

                var code = string.Join(Environment.NewLine, lines);
                var session = new ScriptPackSession(Enumerable.Empty<IScriptPack>(), new string[0]);

                // Act
                var result = scriptEngine.Execute(code, new string[0], new AssemblyReferences(), Enumerable.Empty<string>(),
                        session);

                // Assert
                var exception = Assert.Throws<ScriptCompilationException>(() => result.CompileExceptionInfo.Throw());
                exception.InnerException.ShouldBeType<CompilationErrorException>();
                exception.Message.ShouldContain("The type or namespace name 'Sysasdasdasdtem' could not be found");
            }
        }
    }
}
