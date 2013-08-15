using Moq;
using ScriptCs.Argument;
using ScriptCs.Contracts;
using Xunit;
using Should;

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
                fs.Setup(x => x.FileExists(filePath)).Returns(fileExists);
                fs.Setup(x => x.ReadFile(filePath)).Returns(fileContent);
                var console = new ScriptConsole();

                return new ArgumentHandler(new ArgumentParser(console), new ConfigFileParser(console), fs.Object);
            }

            [Fact]
            public void ShouldHandleConfigFileAndCommandLineArguments()
            {
                const string file = "{\"Install\": \"install test value\" }";
                string[] args = { "server.csx", "-log", "error", "--", "-port", "8080" };

                var argumentHandler = Setup(file);
                var result = argumentHandler.Parse(args);

                result.ShouldNotBeNull();
                result.Arguments.ShouldEqual(args);
                result.CommandArguments.ScriptName.ShouldEqual("server.csx");
                result.CommandArguments.LogLevel.ShouldEqual(LogLevel.Error);
                result.CommandArguments.Install.ShouldEqual("install test value");
                result.ScriptArguments.ShouldEqual(new string[] { "-port", "8080" });
            }

            [Fact]
            public void ShouldHandleCommandLineArgumentsOverConfigFile()
            {
                const string file = "{\"Install\": \"config file arg\", \"debug\": \"true\" }";
                string[] args = { "server.csx", "-Install", "command line arg", "-inMemory", "false", "--", "-port", "8080" };

                var argumentHandler = Setup(file);
                var result = argumentHandler.Parse(args);

                result.ShouldNotBeNull();
                result.Arguments.ShouldEqual(args);
                result.CommandArguments.ScriptName.ShouldEqual("server.csx");
                result.CommandArguments.Install.ShouldEqual("command line arg");
                result.CommandArguments.InMemory.ShouldEqual(false);
                result.ScriptArguments.ShouldEqual(new string[] { "-port", "8080" });
            }

            [Fact]
            public void ShouldHandleCommandLineArgumentsOverConfigFileWithPropertyName()
            {
                const string file = "{\"LogLevel\": \"info\", }";
                string[] args = { "server.csx", "-log", "error", "--", "-port", "8080" };

                var argumentHandler = Setup(file);
                var result = argumentHandler.Parse(args);

                result.ShouldNotBeNull();
                result.Arguments.ShouldEqual(args);
                result.CommandArguments.ScriptName.ShouldEqual("server.csx");
                result.CommandArguments.LogLevel.ShouldEqual(LogLevel.Error);
                result.ScriptArguments.ShouldEqual(new string[] { "-port", "8080" });
            }

            [Fact]
            public void ShouldHandleOnlyCommandLineArguments()
            {
                string[] args = { "server.csx", "--", "-port", "8080" };

                var argumentHandler = Setup(null, "test.txt", false);
                var result = argumentHandler.Parse(args);

                result.ShouldNotBeNull();
                result.Arguments.ShouldEqual(args);
                result.CommandArguments.ScriptName.ShouldEqual("server.csx");
                result.ScriptArguments.ShouldEqual(new string[] { "-port", "8080" });
            }

            [Fact]
            public void ShouldHandleOnlyConfigFile()
            {
                const string file = "{\"log\": \"error\", \"script\": \"server.csx\" }";

                var argumentHandler = Setup(file);
                var result = argumentHandler.Parse(new string[0]);

                result.ShouldNotBeNull();
                result.CommandArguments.ScriptName.ShouldEqual("server.csx");
                result.CommandArguments.LogLevel.ShouldEqual(LogLevel.Error);
                result.ScriptArguments.ShouldEqual(new string[0]);
            }

            [Fact]
            public void ShouldHandleCustomConfigFile()
            {
                const string fileName = "text.txt";
                const string file = "{\"Install\": \"install test value\" }";
                string[] args = { "server.csx", "-log", "error", "-config", fileName, "--", "-port", "8080" };

                var argumentHandler = Setup(file, fileName);
                var result = argumentHandler.Parse(args);

                result.ShouldNotBeNull();
                result.Arguments.ShouldEqual(args);
                result.CommandArguments.ScriptName.ShouldEqual("server.csx");
                result.CommandArguments.LogLevel.ShouldEqual(LogLevel.Error);
                result.ScriptArguments.ShouldEqual(new string[] { "-port", "8080" });
                result.CommandArguments.Install.ShouldEqual("install test value");
            }
        }
    }
}