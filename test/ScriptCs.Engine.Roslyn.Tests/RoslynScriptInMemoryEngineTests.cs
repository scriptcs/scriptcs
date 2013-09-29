using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Ploeh.AutoFixture.Xunit;
using Roslyn.Compilers;
using ScriptCs.Contracts;
using ScriptCs.Engine.Roslyn;
using ScriptCs.Exceptions;
using Xunit;
using Xunit.Extensions;

namespace ScriptCs.Tests
{
    using Should;

    public class RoslynScriptInMemoryEngineTests
    {
        public class TheExecuteMethod
        {
            [Theory, ScriptCsAutoData]
            public void ShouldExposeExceptionThrownByScriptWhenErrorOccurs(
                [NoAutoProperties] RoslynScriptInMemoryEngine scriptEngine)
            {
                // Arrange
                var lines = new List<string>
                {
                    "using System;",
                    @"throw new InvalidOperationException(""InvalidOperationExceptionMessage."");"
                };

                var code = string.Join(Environment.NewLine, lines);
                var session = new ScriptPackSession(Enumerable.Empty<IScriptPack>(), new string[0]);

                // Act
                var result = scriptEngine.Execute(code, new string[0], Enumerable.Empty<string>(), Enumerable.Empty<string>(),
                        session);

                // Assert
                var exception = Assert.Throws<InvalidOperationException>(() => result.ExecuteExceptionInfo.Throw());
                exception.StackTrace.ShouldContain("at Submission#0");
                exception.Message.ShouldContain("InvalidOperationExceptionMessage");
            }

            [Theory, ScriptCsAutoData]
            public void ShouldExposeExceptionThrownByCompilation(
                [NoAutoProperties] RoslynScriptInMemoryEngine scriptEngine)
            {
                // Arrange
                var lines = new List<string>
                {
                    "using Sysasdasdasdtem;"
                };

                var code = string.Join(Environment.NewLine, lines);
                var session = new ScriptPackSession(Enumerable.Empty<IScriptPack>(), new string[0]);

                // Act
                var result = scriptEngine.Execute(code, new string[0], Enumerable.Empty<string>(), Enumerable.Empty<string>(),
                        session);

                // Assert
                var exception = Assert.Throws<ScriptCompilationException>(() => result.CompileExceptionInfo.Throw());
                exception.InnerException.ShouldBeType<CompilationErrorException>();
                exception.Message.ShouldContain("The type or namespace name 'Sysasdasdasdtem' could not be found");
            }
        }
    }
}
