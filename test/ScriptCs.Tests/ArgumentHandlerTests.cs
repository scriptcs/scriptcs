using Moq;
using ScriptCs.Argument;
using Xunit;

namespace ScriptCs.Tests
{
    public class ArgumentHandlerTests
    {
        public class ParseMethod
        {
            private static IArgumentHandler Setup(string fileContent, string fileName = "scriptcs.opts", bool fileExists = true)
            {
                const string currentDirectory = "C:\\test\\folder";
                
                string filePath = currentDirectory + '\\' + fileName;

                var fs = new Mock<IFileSystem>();
                fs.SetupGet(x => x.CurrentDirectory).Returns(currentDirectory);
                fs.Setup(x => x.PathCombine(currentDirectory, fileName)).Returns(filePath);
                fs.Setup(x => x.FileExists(filePath)).Returns(fileExists);
                fs.Setup(x => x.ReadFile(filePath)).Returns(fileContent);

                return new ArgumentHandler(new ArgumentParser(), new ConfigFileParser(), fs.Object);
            }

            [Fact]
            public void ShouldHandleConfigFileAndCommandLineArguments()
            {
                const string file = "{\"Install\": \"install test value\" }";
                string[] args = { "server.csx", "-log", "error", "--", "-port", "8080" };

                var argumentHandler = Setup(file);
                var result = argumentHandler.Parse(args);

                Assert.NotNull(result);
                Assert.Equal(result.Arguments, args);
                Assert.Equal(result.CommandArguments.ScriptName, "server.csx");
                Assert.Equal(result.CommandArguments.LogLevel, LogLevel.Error);
                Assert.Equal(result.CommandArguments.Install, "install test value");

                Assert.Equal(new string[] { "-port", "8080" }, result.ScriptArguments);
            }

            [Fact]
            public void ShouldHandleCommandLineArgumentsOverConfigFile()
            {
                const string file = "{\"Install\": \"config file arg\", \"debug\": \"true\" }";
                string[] args = { "server.csx", "-Install", "command line arg", "-debug", "false", "--", "-port", "8080" };

                var argumentHandler = Setup(file);
                var result = argumentHandler.Parse(args);

                Assert.NotNull(result.CommandArguments);
                Assert.Equal(result.Arguments, args);
                Assert.Equal(result.CommandArguments.ScriptName, "server.csx");
                Assert.Equal(result.CommandArguments.Install, "command line arg");
                Assert.Equal(result.CommandArguments.Debug, false);

                Assert.Equal(new string[] { "-port", "8080" }, result.ScriptArguments);
            }

            [Fact]
            public void ShouldHandleCommandLineArgumentsOverConfigFileWithPropertyName()
            {
                const string file = "{\"LogLevel\": \"info\", }";
                string[] args = { "server.csx", "-log", "error", "--", "-port", "8080" };

                var argumentHandler = Setup(file);
                var result = argumentHandler.Parse(args);

                Assert.NotNull(result);
                Assert.Equal(result.Arguments, args);
                Assert.Equal(result.CommandArguments.ScriptName, "server.csx");
                Assert.Equal(result.CommandArguments.LogLevel, LogLevel.Error);

                Assert.Equal(new string[] { "-port", "8080" }, result.ScriptArguments);
            }

            [Fact]
            public void ShouldHandleOnlyCommandLineArguments()
            {
                string[] args = { "server.csx", "--", "-port", "8080" };

                var argumentHandler = Setup(null, "test.txt", false);
                var result = argumentHandler.Parse(args);

                Assert.NotNull(result);
                Assert.Equal(result.Arguments, args);
                Assert.NotNull(result.CommandArguments);
                Assert.Equal(result.CommandArguments.ScriptName, "server.csx");

                Assert.Equal(new string[] { "-port", "8080" }, result.ScriptArguments);
            }

            [Fact]
            public void ShouldHandleOnlyConfigFile()
            {
                const string file = "{\"log\": \"error\", \"script\": \"server.csx\" }";

                var argumentHandler = Setup(file);
                var result = argumentHandler.Parse(new string[0]);

                Assert.NotNull(result);
                Assert.Equal(result.CommandArguments.ScriptName, "server.csx");
                Assert.Equal(result.CommandArguments.LogLevel, LogLevel.Error);
                Assert.Equal(result.ScriptArguments, new string[0]);
            }

            [Fact]
            public void ShouldHandleCustomConfigFile()
            {
                const string fileName = "text.txt";
                const string file = "{\"Install\": \"install test value\" }";
                string[] args = { "server.csx", "-log", "error", "-config", fileName, "--", "-port", "8080" };

                var argumentHandler = Setup(file, fileName);
                var result = argumentHandler.Parse(args);

                Assert.NotNull(result);
                Assert.Equal(result.Arguments, args);
                Assert.Equal(result.CommandArguments.ScriptName, "server.csx");
                Assert.Equal(result.CommandArguments.LogLevel, LogLevel.Error);
                Assert.Equal(result.CommandArguments.Install, "install test value");

                Assert.Equal(new string[] { "-port", "8080" }, result.ScriptArguments);                
            }
        }
    }
}