using Moq;
using ScriptCs.Argument;
using ScriptCs.Contracts;
using ScriptCs.Hosting;

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
            public void ShouldPrintHelpMessageForUnsupportedArgs()
            {
                var console = new Mock<IConsole>();
                string[] args = { "-foo" };

                var parser = new ArgumentParser(console.Object);
                var result = parser.Parse(args);

                console.Verify(x => x.WriteLine(It.Is<string>(i => i.StartsWith("Parameter \"foo\" is not supported!"))));
                result.ShouldBeNull();
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

            [Fact]
            public void ShouldSetVersionIfPackageVersionNumberFollowsPackageToInstallName()
            {
                string[] args = { "-install", "glimpse.scriptcs", "1.0.1" };

                var parser = new ArgumentParser(new ScriptConsole());
                var result = parser.Parse(args);

                result.PackageVersion.ShouldEqual("1.0.1");
                result.Install.ShouldEqual("glimpse.scriptcs");
            }

            [Fact]
            public void ShouldSetVersionIfPackageVersionNumberSpecifiedExplicitly()
            {
                string[] args = { "-install", "glimpse.scriptcs", "-packageversion", "1.0.1" };

                var parser = new ArgumentParser(new ScriptConsole());
                var result = parser.Parse(args);

                result.PackageVersion.ShouldEqual("1.0.1");
                result.Install.ShouldEqual("glimpse.scriptcs");
            }

            [Fact]
            public void ShouldAutmoaticallySetLogLevelDebugIfDebugFlagIsPassed()
            {
                string[] args = { "test.csx", "-debug" };

                var parser = new ArgumentParser(new ScriptConsole());
                var result = parser.Parse(args);

                result.Debug.ShouldBeTrue();
                result.LogLevel.ShouldEqual(LogLevel.Debug);
            }
        }
    }
}