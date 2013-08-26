using System;
using System.Collections.Generic;
using System.Linq;

using Ploeh.AutoFixture.Xunit;

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
            public void ShouldThrowExceptionThrownByScriptWhenErrorOccurs(
                [NoAutoProperties] RoslynScriptInMemoryEngine scriptEngine)
            {
                // Arrange
                var lines = new List<string>
                {
                    "using System;",
                    @"throw new InvalidOperationException(""InvalidOperationExceptionMessage."");"
                };

                var code = string.Join(Environment.NewLine, lines);
                var session = new ScriptPackSession(Enumerable.Empty<IScriptPack>());

                // Act
                var exception = Assert.Throws<ScriptExecutionException>(() =>
                    scriptEngine.Execute(
                        code,
                        new string[0],
                        Enumerable.Empty<string>(),
                        Enumerable.Empty<string>(),
                        session));

                // Assert
                exception.Message.ShouldContain("at Submission#0");
                exception.Message.ShouldContain("Exception Message: InvalidOperationExceptionMessage.");
            }
        }
    }
}