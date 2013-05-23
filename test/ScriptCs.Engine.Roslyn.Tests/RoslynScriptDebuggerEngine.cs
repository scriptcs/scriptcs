using System;
using System.Linq;
using Common.Logging;
using Moq;
using ScriptCs.Contracts;
using ScriptCs.Engine.Roslyn;
using ScriptCs.Exceptions;
using Should;
using Xunit;

namespace ScriptCs.Tests
{
    public class RoslynScriptDebuggerEngineTests
    {
        private static RoslynScriptDebuggerEngine CreateScriptEngine(
            Mock<IScriptHostFactory> scriptHostFactory = null)
        {
            scriptHostFactory = scriptHostFactory ?? new Mock<IScriptHostFactory>();

            var logger = new Mock<ILog>();

            return new RoslynScriptDebuggerEngine(scriptHostFactory.Object, logger.Object);
        }

        public class TheExecuteMethod
        {
            [Fact]
            public void ShouldThrowExceptionThrownByScriptWhenErrorOccurs()
            {
                var code = string.Format(
                    "{0}{1}{2}", 
                    "using System;",
                    Environment.NewLine,
                    @"throw new InvalidOperationException(""InvalidOperationExceptionMessage."");");

                var scriptEngine = CreateScriptEngine();

                var exception = Assert.Throws<ScriptExecutionException>(
                    () =>
                    scriptEngine.Execute(
                        code, new string[0], Enumerable.Empty<string>(), Enumerable.Empty<string>(), new ScriptPackSession(Enumerable.Empty<IScriptPack>())));

                Console.WriteLine(exception.Message);

                exception.Message.ShouldContain("line 2");
                exception.Message.ShouldContain("Exception Message: InvalidOperationExceptionMessage.");
            }
        }
    }
}
