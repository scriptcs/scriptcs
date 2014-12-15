using Moq;
using ScriptCs.Argument;
using ScriptCs.Contracts;
using ScriptCs.Hosting;
using Should;
using Xunit;

namespace ScriptCs.Tests
{
    public class ArgumentHandlerTests
    {
        public class ParseMethod
        {
            private static IArgumentHandler Setup(string fileContent, string fileName = Constants.ConfigFilename, bool fileExists = true, string globalFileContent = null)
            {
                const string currentDirectory = "C:\\test\\folder";
                const string moduleDirectory = "C:\\test\\moduleFolder";

                string filePath = currentDirectory + '\\' + fileName;
                string globalFilePath = moduleDirectory + "\\" + Constants.ConfigFilename;

                var fs = new Mock<IFileSystem>();
                fs.SetupGet(x => x.CurrentDirectory).Returns(currentDirectory);
                fs.Setup(x => x.FileExists(filePath)).Returns(fileExists);
                fs.Setup(x => x.ReadFile(filePath)).Returns(fileContent);
                fs.SetupGet(x => x.GlobalFolder).Returns(moduleDirectory);
                fs.SetupGet(x => x.GlobalOptsFile).Returns(globalFilePath);
                fs.Setup(x => x.FileExists(globalFilePath)).Returns(globalFileContent != null);
                fs.Setup(x => x.ReadFile(globalFilePath)).Returns(globalFileContent);

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
            public void ShouldHandleGlobalConfigAndCommandLineArguments()
            {
                const string file = "{\"Install\": \"install test value\" }";
                string[] args = { "server.csx", "-log", "error", "--", "-port", "8080" };

                var argumentHandler = Setup(null, globalFileContent: file);
                var result = argumentHandler.Parse(args);

                result.ShouldNotBeNull();
                result.Arguments.ShouldEqual(args);
                result.CommandArguments.ScriptName.ShouldEqual("server.csx");
                result.CommandArguments.LogLevel.ShouldEqual(LogLevel.Error);
                result.CommandArguments.Install.ShouldEqual("install test value");
                result.ScriptArguments.ShouldEqual(new string[] { "-port", "8080" });
            }

            [Fact]
            public void ShouldHandleGlobalConfigAndLocalConfigArguments()
            {
                const string localFile = "{\"Install\": \"install test value\" }";
                const string globalFile = "{\"Modules\": \"modules test value\" }";
                string[] args = { };

                var argumentHandler = Setup(localFile, globalFileContent: globalFile);
                var result = argumentHandler.Parse(args);

                result.ShouldNotBeNull();
                result.Arguments.ShouldEqual(args);
                result.CommandArguments.Install.ShouldEqual("install test value");
                result.CommandArguments.Modules.ShouldEqual("modules test value");
                result.ScriptArguments.ShouldEqual(new string[] { });
            }

            [Fact]
            public void ShouldHandleLocalConfigFileArgumentsOverGlobalConfigFile()
            {
                const string localFile = "{\"Install\": \"local install test value\" }";
                const string globalFile = "{\"Install\": \"global install test value\" }";
                string[] args = { "server.csx", "-cache", "--", "-port", "8080" };

                var argumentHandler = Setup(localFile, globalFileContent: globalFile);
                var result = argumentHandler.Parse(args);

                result.ShouldNotBeNull();
                result.Arguments.ShouldEqual(args);
                result.CommandArguments.ScriptName.ShouldEqual("server.csx");
                result.CommandArguments.Install.ShouldEqual("local install test value");
                result.CommandArguments.Cache.ShouldEqual(true);
                result.ScriptArguments.ShouldEqual(new string[] { "-port", "8080" });
            }

            [Fact]
            public void ShouldHandleCommandLineArgumentsOverConfigFile()
            {
                const string file = "{\"Install\": \"config file arg\", \"debug\": \"true\" }";
                string[] args = { "server.csx", "-Install", "command line arg", "-cache", "--", "-port", "8080" };

                var argumentHandler = Setup(file);
                var result = argumentHandler.Parse(args);

                result.ShouldNotBeNull();
                result.Arguments.ShouldEqual(args);
                result.CommandArguments.ScriptName.ShouldEqual("server.csx");
                result.CommandArguments.Install.ShouldEqual("command line arg");
                result.CommandArguments.Cache.ShouldEqual(true);
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
            public void ShouldHandleOnlyGlobalConfigFile()
            {
                const string file = "{\"log\": \"error\", \"script\": \"server.csx\" }";

                var argumentHandler = Setup(null, globalFileContent: file);
                var result = argumentHandler.Parse(new string[0]);

                result.ShouldNotBeNull();
                result.CommandArguments.ScriptName.ShouldEqual("server.csx");
                result.CommandArguments.LogLevel.ShouldEqual(LogLevel.Error);
                result.ScriptArguments.ShouldEqual(new string[0]);
            }

            [Fact]
            public void ShouldHandleOnlyLocalConfigFile()
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
            public void ShouldHandleCustomLocalConfigFile()
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

            [Fact]
            public void ShouldHandleHelp()
            {
                string[] args = { "-help" };

                var argumentHandler = Setup(null);
                var result = argumentHandler.Parse(args);

                result.ShouldNotBeNull();
                result.Arguments.ShouldEqual(args);
                result.CommandArguments.ScriptName.ShouldBeNull();
                result.CommandArguments.Help.ShouldBeTrue();
            }

            [Fact]
            public void ShouldHandleScriptNameStartingWithRepl()
            {
                const string file = "{\"repl\": true}";
                string[] args = { "replication.csx", "--", "-port", "8080" };

                var argumentHandler = Setup(file);
                var result = argumentHandler.Parse(args);

                result.ShouldNotBeNull();
                result.Arguments.ShouldEqual(args);
                result.CommandArguments.ScriptName.ShouldEqual("replication.csx");
                result.CommandArguments.Repl.ShouldBeTrue();
                result.ScriptArguments.ShouldEqual(new string[] { "-port", "8080" });
            }

            [Fact]
            public void ShouldUseScriptOptsIfParsingFailed()
            {
                var parser = new Mock<IArgumentParser>();
                parser.Setup(x => x.Parse(It.IsAny<string[]>())).Returns(new ScriptCsArgs());
                var system = new Mock<IFileSystem>();
                system.Setup(x => x.CurrentDirectory).Returns(@"C:");

                var configParser = new Mock<IConfigFileParser>();
                var argumentHandler = new ArgumentHandler(parser.Object, configParser.Object, system.Object);

                argumentHandler.Parse(new string[0]);

                system.Verify(x => x.FileExists(@"C:\" + Constants.ConfigFilename), Times.Once());
            }

            [Fact]
            public void ShouldSetLogLevelToDebugWhenDebugIsSetOnCommandLine()
            {
                var argumentHandler = Setup(null);
                string[] args = { "server.csx", "-debug" };

                var result = argumentHandler.Parse(args);

                result.CommandArguments.Debug.ShouldBeTrue();
                result.CommandArguments.LogLevel.ShouldEqual(LogLevel.Debug);
            }

            [Fact]
            public void ShouldSetLogLevelToDebugWhenDebugIsSetInOptsFile()
            {
                var argumentHandler = Setup("{ Debug: \"True\" }");
                string[] args = { "server.csx" };

                var result = argumentHandler.Parse(args);

                result.CommandArguments.Debug.ShouldBeTrue();
                result.CommandArguments.LogLevel.ShouldEqual(LogLevel.Debug);
            }

            [Fact]
            public void ShouldAllowTraceLogLevelWhenDebugIsSet()
            {
                var argumentHandler = Setup(null);
                string[] args = { "server.csx", "-debug", "-log", "trace" };

                var result = argumentHandler.Parse(args);

                result.CommandArguments.Debug.ShouldBeTrue();
                result.CommandArguments.LogLevel.ShouldEqual(LogLevel.Trace);
            }

            [Fact]
            public void ShouldSetLogLevelToDebugWhenDebugIsSetAndLogLevelIsLowerThanDebug()
            {
                var argumentHandler = Setup(null);
                string[] args = { "server.csx", "-debug", "-log", "info" };

                var result = argumentHandler.Parse(args);

                result.CommandArguments.Debug.ShouldBeTrue();
                result.CommandArguments.LogLevel.ShouldEqual(LogLevel.Debug);
            }
        }
    }
}