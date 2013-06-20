using Moq;
using Xunit;

namespace ScriptCs.Tests
{
    public class ArgumentParserTests
    {
        public class ExecuteMethod
        {
            private IFileSystem Setup(string fileContent, bool fileExists = true)
            {
                const string CurrentDirectory = "C:\\test\\folder";
                
                string filePath = CurrentDirectory + '\\' + ArgumentParser.CofigurationFileName;

                var fs = new Mock<IFileSystem>();
                fs.SetupGet(x => x.CurrentDirectory).Returns(CurrentDirectory);
                fs.Setup(x => x.PathCombine(CurrentDirectory, ArgumentParser.CofigurationFileName)).Returns(filePath);
                fs.Setup(x => x.FileExists(filePath)).Returns(fileExists);
                fs.Setup(x => x.ReadFile(filePath)).Returns(fileContent);

                return fs.Object;
            }

            [Fact]
            public void ShouldHandleConfigFile()
            {
                const string file = "{\"Install\": \"install test value\" }";
                var fileSystem = Setup(file);

                string[] args = {};

                var parser = new ArgumentParser(args, fileSystem);

                Assert.NotNull(parser.CommandArguments);
                Assert.Equal(parser.CommandArguments.Install, "install test value");
            }

            [Fact]
            public void ShouldHandleConfigFileAndCommandLineArguments()
            {
                const string file = "{\"Install\": \"install test value\" }";
                var fileSystem = Setup(file);

                string[] args = { "server.csx", "-log", "error", "--", "-port", "8080" };

                var parser = new ArgumentParser(args, fileSystem);

                Assert.NotNull(parser.CommandArguments);
                Assert.Equal(parser.CommandArguments.ScriptName, "server.csx");
                Assert.Equal(parser.CommandArguments.LogLevel, LogLevel.Error);
                Assert.Equal(parser.CommandArguments.Install, "install test value");

                Assert.Equal(new string[] { "-port", "8080" }, parser.ScriptArguments);
            }

            [Fact]
            public void ShouldHandleConfigFileOverCommandLineArguments()
            {
                const string file = "{\"Install\": \"config file arg\", }";
                var fileSystem = Setup(file);

                string[] args = { "server.csx", "-Install", "command line arg", "--", "-port", "8080" };

                var parser = new ArgumentParser(args, fileSystem);

                Assert.NotNull(parser.CommandArguments);
                Assert.Equal(parser.CommandArguments.ScriptName, "server.csx");
                Assert.Equal(parser.CommandArguments.Install, "command line arg");

                Assert.Equal(new string[] { "-port", "8080" }, parser.ScriptArguments);
            }

            [Fact]
            public void ShouldHandleConfigFileOverCommandLineArgumentsWithPropertyName()
            {
                const string file = "{\"LogLevel\": \"info\", }";
                var fileSystem = Setup(file);

                string[] args = { "server.csx", "-log", "error", "--", "-port", "8080" };

                var parser = new ArgumentParser(args, fileSystem);

                Assert.NotNull(parser.CommandArguments);
                Assert.Equal(parser.CommandArguments.ScriptName, "server.csx");
                Assert.Equal(parser.CommandArguments.LogLevel, LogLevel.Error);

                Assert.Equal(new string[] { "-port", "8080" }, parser.ScriptArguments);
            }

            [Fact]
            public void ShouldHanldeArgumentTypeConversionBool()
            {
                const string file = "{\"debug\": \"true\", }";
                var fileSystem = Setup(file);

                string[] args = { "server.csx", "--", "-port", "8080" };

                var parser = new ArgumentParser(args, fileSystem);

                Assert.NotNull(parser.CommandArguments);
                Assert.Equal(parser.CommandArguments.ScriptName, "server.csx");
                Assert.Equal(parser.CommandArguments.Debug, true);

                Assert.Equal(new string[] { "-port", "8080" }, parser.ScriptArguments);
            }

            [Fact]
            public void ShouldHanldeArgumentTypeConversionEnum()
            {
                const string file = "{\"log\": \"error\", }";
                var fileSystem = Setup(file);

                string[] args = { "server.csx", "--", "-port", "8080" };

                var parser = new ArgumentParser(args, fileSystem);

                Assert.NotNull(parser.CommandArguments);
                Assert.Equal(parser.CommandArguments.ScriptName, "server.csx");
                Assert.Equal(parser.CommandArguments.LogLevel, LogLevel.Error);

                Assert.Equal(new string[] { "-port", "8080" }, parser.ScriptArguments);
            }

            [Fact]
            public void ShouldHandleOnlyCommandLineArguments()
            {
                var fileSystem = Setup(null, false);

                string[] args = { "server.csx", "--", "-port", "8080" };

                var parser = new ArgumentParser(args, fileSystem);

                Assert.NotNull(parser.CommandArguments);
                Assert.Equal(parser.CommandArguments.ScriptName, "server.csx");

                Assert.Equal(new string[] { "-port", "8080" }, parser.ScriptArguments);
            }

            [Fact]
            public void ShouldHandleOnlyConfigFile()
            {
                const string file = "{\"log\": \"error\", \"script\": \"server.csx\" }";
                var fileSystem = Setup(file);

                var parser = new ArgumentParser(new string[] {}, fileSystem);

                Assert.NotNull(parser.CommandArguments);
                Assert.Equal(parser.CommandArguments.ScriptName, "server.csx");
                Assert.Equal(parser.CommandArguments.LogLevel, LogLevel.Error);
            }

            [Fact]
            public void ShouldHandleConfigArgumentsCaseInsensitive()
            {
                const string file = "{\"logLEVEL\": \"TRaCE\", }";
                var fileSystem = Setup(file);

                string[] args = { "server.csx", "-dEBUg", "FalsE", "--", "-port", "8080" };

                var parser = new ArgumentParser(args, fileSystem);

                Assert.NotNull(parser.CommandArguments);
                Assert.Equal(parser.CommandArguments.ScriptName, "server.csx");
                Assert.Equal(parser.CommandArguments.LogLevel, LogLevel.Trace);
                Assert.Equal(parser.CommandArguments.Debug, false);

                Assert.Equal(new string[] { "-port", "8080" }, parser.ScriptArguments);
            }

        }
    }
}