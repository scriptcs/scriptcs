using ScriptCs.Argument;
using ScriptCs.Contracts;
using Should;
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

                result.ShouldNotBeNull();
                result.ScriptName.ShouldEqual("server.csx");
                result.LogLevel.ShouldEqual(LogLevel.Error);
            }

            [Fact]
            public void ShouldHandleEmptyAttray()
            {
                var parser = new ArgumentParser(new ScriptConsole());
                var result = parser.Parse(new string[0]);

                result.ShouldNotBeNull();
                result.Repl.ShouldBeTrue();
                result.LogLevel.ShouldEqual(LogLevel.Info);
                result.Config.ShouldEqual("scriptcs.opts");
            }

            [Fact]
            public void ShouldHandleNull()
            {
                var parser = new ArgumentParser(new ScriptConsole());
                var result = parser.Parse(null);

                result.ShouldNotBeNull();
                result.Repl.ShouldBeTrue();
                result.LogLevel.ShouldEqual(LogLevel.Info);
                result.Config.ShouldEqual("scriptcs.opts");
            }

            [Fact]
            public void ShouldSupportHelp() 
            {
                string[] args = { "-help" };

                var parser = new ArgumentParser(new ScriptConsole());
                var result = parser.Parse(args);

                result.ShouldNotBeNull();
                result.ScriptName.ShouldBeNull();
                result.Help.ShouldBeTrue();
                result.LogLevel.ShouldEqual(LogLevel.Info);
            }

            [Fact]
            public void ShouldGoIntoReplIfOnlyLogLevelIsSet()
            {
                string[] args = { "-loglevel", "debug" };

                var parser = new ArgumentParser(new ScriptConsole());
                var result = parser.Parse(args);

                result.Repl.ShouldBeTrue();
                result.LogLevel.ShouldEqual(LogLevel.Debug);
            }

            [Fact]
            public void ShouldGoIntoReplIfOnlyLogIsSet()
            {
                string[] args = { "-log", "debug" };

                var parser = new ArgumentParser(new ScriptConsole());
                var result = parser.Parse(args);

                result.Repl.ShouldBeTrue();
                result.LogLevel.ShouldEqual(LogLevel.Debug);
            }
        }
    }
}