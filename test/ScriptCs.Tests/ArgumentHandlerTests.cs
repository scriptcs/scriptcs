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

                return new ArgumentHandler(new ConfigFileParser(console), fs.Object);
            }

            [Fact]
            public void ShouldHandleConfigFileAndCommandLineArguments()
            {
                const string file = "{\"Install\": \"install test value\" }";
                string[] args = { "server.csx", "-log", "error" };
                var scriptArgs = ScriptCsArgs.Parse(args);

                var argumentHandler = Setup(file);
                var result = argumentHandler.Parse(scriptArgs, args);

                result.ShouldNotBeNull();
                result.ScriptName.ShouldEqual("server.csx");
                result.LogLevel.ShouldEqual(LogLevel.Error);
                result.Install.ShouldEqual("install test value");
            }

            [Fact]
            public void ShouldHandleGlobalConfigAndCommandLineArguments()
            {
                const string file = "{\"Install\": \"install test value\" }";
                string[] args = { "server.csx", "-log", "error" };
                var scriptArgs = ScriptCsArgs.Parse(args);

                var argumentHandler = Setup(null, globalFileContent: file);
                var result = argumentHandler.Parse(scriptArgs, args);

                result.ShouldNotBeNull();
                result.ScriptName.ShouldEqual("server.csx");
                result.LogLevel.ShouldEqual(LogLevel.Error);
                result.Install.ShouldEqual("install test value");
            }

            [Fact]
            public void ShouldHandleGlobalConfigAndLocalConfigArguments()
            {
                const string localFile = "{\"Install\": \"install test value\" }";
                const string globalFile = "{\"Modules\": \"modules test value\" }";
                string[] args = { };
                var scriptArgs = ScriptCsArgs.Parse(args);

                var argumentHandler = Setup(localFile, globalFileContent: globalFile);
                var result = argumentHandler.Parse(scriptArgs, args);

                result.ShouldNotBeNull();
                result.Install.ShouldEqual("install test value");
                result.Modules.ShouldEqual("modules test value");
            }

            [Fact]
            public void ShouldHandleLocalConfigFileArgumentsOverGlobalConfigFile()
            {
                const string localFile = "{\"Install\": \"local install test value\" }";
                const string globalFile = "{\"Install\": \"global install test value\" }";
                string[] args = { "server.csx", "-cache" };
                var scriptArgs = ScriptCsArgs.Parse(args);

                var argumentHandler = Setup(localFile, globalFileContent: globalFile);
                var result = argumentHandler.Parse(scriptArgs, args);

                result.ShouldNotBeNull();
                result.ScriptName.ShouldEqual("server.csx");
                result.Install.ShouldEqual("local install test value");
                result.Cache.ShouldEqual(true);
            }

            [Fact]
            public void ShouldHandleCommandLineArgumentsOverConfigFile()
            {
                const string file = "{\"Install\": \"config file arg\", \"debug\": \"true\" }";
                string[] args = { "server.csx", "-Install", "command line arg", "-cache" };
                var scriptArgs = ScriptCsArgs.Parse(args);

                var argumentHandler = Setup(file);
                var result = argumentHandler.Parse(scriptArgs, args);

                result.ShouldNotBeNull();
                result.ScriptName.ShouldEqual("server.csx");
                result.Install.ShouldEqual("command line arg");
                result.Cache.ShouldEqual(true);
            }

            [Fact]
            public void ShouldHandleCommandLineArgumentsOverConfigFileWithPropertyName()
            {
                const string file = "{\"LogLevel\": \"info\", }";
                string[] args = { "server.csx", "-log", "error" };
                var scriptArgs = ScriptCsArgs.Parse(args);

                var argumentHandler = Setup(file);
                var result = argumentHandler.Parse(scriptArgs, args);

                result.ShouldNotBeNull();
                result.ScriptName.ShouldEqual("server.csx");
                result.LogLevel.ShouldEqual(LogLevel.Error);
            }

            [Fact]
            public void ShouldHandleOnlyCommandLineArguments()
            {
                string[] args = { "server.csx" };
                var scriptArgs = ScriptCsArgs.Parse(args);

                var argumentHandler = Setup(null, "test.txt", false);
                var result = argumentHandler.Parse(scriptArgs, args);

                result.ShouldNotBeNull();
                result.ScriptName.ShouldEqual("server.csx");
            }

            [Fact]
            public void ShouldHandleOnlyGlobalConfigFile()
            {
                const string file = "{\"log\": \"error\", \"script\": \"server.csx\" }";
                string[] args = { };
                var scriptArgs = ScriptCsArgs.Parse(args);

                var argumentHandler = Setup(null, globalFileContent: file);
                var result = argumentHandler.Parse(scriptArgs, args);

                result.ShouldNotBeNull();
                result.ScriptName.ShouldEqual("server.csx");
                result.LogLevel.ShouldEqual(LogLevel.Error);
            }

            [Fact]
            public void ShouldHandleOnlyLocalConfigFile()
            {
                const string file = "{\"log\": \"error\", \"script\": \"server.csx\" }";
                string[] args = { };
                var scriptArgs = ScriptCsArgs.Parse(args);

                var argumentHandler = Setup(file);
                var result = argumentHandler.Parse(scriptArgs, args);

                result.ShouldNotBeNull();
                result.ScriptName.ShouldEqual("server.csx");
                result.LogLevel.ShouldEqual(LogLevel.Error);
            }

            [Fact]
            public void ShouldHandleCustomLocalConfigFile()
            {
                const string fileName = "text.txt";
                const string file = "{\"Install\": \"install test value\" }";
                string[] args = { "server.csx", "-log", "error", "-config", fileName };
                var scriptArgs = ScriptCsArgs.Parse(args);

                var argumentHandler = Setup(file, fileName);
                var result = argumentHandler.Parse(scriptArgs, args);

                result.ShouldNotBeNull();
                result.ScriptName.ShouldEqual("server.csx");
                result.LogLevel.ShouldEqual(LogLevel.Error);
                result.Install.ShouldEqual("install test value");
            }

            [Fact]
            public void ShouldHandleHelp()
            {
                string[] args = { "-help" };
                var scriptArgs = ScriptCsArgs.Parse(args);

                var argumentHandler = Setup(null);
                var result = argumentHandler.Parse(scriptArgs, args);

                result.ShouldNotBeNull();
                result.ScriptName.ShouldBeNull();
                result.Help.ShouldBeTrue();
            }

            [Fact]
            public void ShouldHandleScriptNameStartingWithRepl()
            {
                const string file = "{\"repl\": true}";
                string[] args = { "replication.csx" };
                var scriptArgs = ScriptCsArgs.Parse(args);

                var argumentHandler = Setup(file);
                var result = argumentHandler.Parse(scriptArgs, args);

                result.ShouldNotBeNull();
                result.ScriptName.ShouldEqual("replication.csx");
                result.Repl.ShouldBeTrue();
            }

            [Fact]
            public void ShouldUseScriptOptsIfParsingFailed()
            {
                string[] args = { };
                var scriptArgs = ScriptCsArgs.Parse(args);
                var system = new Mock<IFileSystem>();
                system.Setup(x => x.CurrentDirectory).Returns(@"C:");

                var configParser = new Mock<IConfigFileParser>();
                var argumentHandler = new ArgumentHandler(configParser.Object, system.Object);

                argumentHandler.Parse(scriptArgs, args);

                system.Verify(x => x.FileExists(@"C:\" + Constants.ConfigFilename), Times.Once());
            }

            [Fact]
            public void ShouldSetLogLevelToDebugWhenDebugIsSetOnCommandLine()
            {
                var argumentHandler = Setup(null);
                string[] args = { "server.csx", "-debug" };
                var scriptArgs = ScriptCsArgs.Parse(args);

                var result = argumentHandler.Parse(scriptArgs, args);

                result.Debug.ShouldBeTrue();
                result.LogLevel.ShouldEqual(LogLevel.Debug);
            }

            [Fact]
            public void ShouldSetLogLevelToDebugWhenDebugIsSetInOptsFile()
            {
                var argumentHandler = Setup("{ Debug: \"True\" }");
                string[] args = { "server.csx" };
                var scriptArgs = ScriptCsArgs.Parse(args);

                var result = argumentHandler.Parse(scriptArgs, args);

                result.Debug.ShouldBeTrue();
                result.LogLevel.ShouldEqual(LogLevel.Debug);
            }

            [Fact]
            public void ShouldOverrideDebugLogLevelIfLogLevelIsSetOnCommandLine()
            {
                var argumentHandler = Setup(null);
                string[] args = { "server.csx", "-debug", "-loglevel", "error" };
                var scriptArgs = ScriptCsArgs.Parse(args);

                var result = argumentHandler.Parse(scriptArgs, args);

                result.Debug.ShouldBeTrue();
                result.LogLevel.ShouldEqual(LogLevel.Error);
            }

            [Fact]
            public void ShouldOverrideDebugLogLevelIfLogLevelIsSetInOptsFile()
            {
                var argumentHandler = Setup("{ LogLevel: \"Error\" }");
                string[] args = { "server.csx", "-debug" };
                var scriptArgs = ScriptCsArgs.Parse(args);

                var result = argumentHandler.Parse(scriptArgs, args);

                result.Debug.ShouldBeTrue();
                result.LogLevel.ShouldEqual(LogLevel.Error);
            }

            [Fact]
            public void ShouldSetLogLevelToInfoIfNotSpecifiedAndNotInDebugMode()
            {
                var argumentHandler = Setup(null);
                string[] args = { "server.csx" };
                var scriptArgs = ScriptCsArgs.Parse(args);

                var result = argumentHandler.Parse(scriptArgs, args);

                result.LogLevel.ShouldEqual(LogLevel.Info);
            }
        }
    }
}