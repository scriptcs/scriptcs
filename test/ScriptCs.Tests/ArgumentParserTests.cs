using ScriptCs.Argument;
using Xunit;

namespace ScriptCs.Tests
{
    public class ArgumentParserTests
    {        
        public class ParseMethod
        {
            [Fact]
            public void ShouldHandleCommandLineArguments()
            {
                string[] args = { "server.csx", "-log", "error" };

                var parser = new ArgumentParser(new ScriptConsole());
                var result = parser.Parse(args);

                Assert.NotNull(result);
                Assert.Equal(result.ScriptName, "server.csx");
                Assert.Equal(result.LogLevel, LogLevel.Error);
            }

            [Fact]
            public void ShouldHandleEmptyAttray()
            {
                var parser = new ArgumentParser(new ScriptConsole());
                var result = parser.Parse(new string[0]);

                Assert.NotNull(result);
                Assert.Equal(result.Repl, true);
                Assert.Equal(result.LogLevel, LogLevel.Info);
                Assert.Equal(result.Config, "scriptcs.opts");
            }

            [Fact]
            public void ShouldHandleNull()
            {
                var parser = new ArgumentParser();
                var result = parser.Parse(null);

                Assert.NotNull(result);
                Assert.Equal(result.Repl, true);
                Assert.Equal(result.LogLevel, LogLevel.Info);
                Assert.Equal(result.Config, "scriptcs.opts");
            }

        }         
    }
}