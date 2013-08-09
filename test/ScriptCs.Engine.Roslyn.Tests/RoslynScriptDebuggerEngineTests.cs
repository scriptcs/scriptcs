using System;
using System.Collections.Generic;
using System.Linq;

using Ploeh.AutoFixture.Xunit;

using ScriptCs.Contracts;
using ScriptCs.Engine.Roslyn;
using ScriptCs.Exceptions;
using Should;
using Xunit;
using Xunit.Extensions;

namespace ScriptCs.Tests
{
    public class RoslynScriptDebuggerEngineTests
    {
        public class TheExecuteMethod
        {
            [Theory, ScriptCsAutoData]
            public void ShouldThrowExceptionThrownByScriptWhenErrorOccurs(
                [NoAutoProperties] RoslynScriptDebuggerEngine scriptEngine)
            {
                // Arrange
                var lines = new List<string>
                {
                    "using System;",
                    @"throw new InvalidOperationException(""InvalidOperationExceptionMessage."");"
                };

                var code = string.Join(Environment.NewLine, lines);
                var environment = new ScriptEnvironment(Enumerable.Empty<string>(), Enumerable.Empty<string>(), null, new string[0], code);
                var session = new ScriptPackSession(Enumerable.Empty<IScriptPack>());

                // Act
                var exception = Assert.Throws<ScriptExecutionException>(() => scriptEngine.Execute(environment, session));

                // Assert
                exception.Message.ShouldContain("at Submission#0");
                exception.Message.ShouldContain("Exception Message: InvalidOperationExceptionMessage.");
            }
        }
    }
}
