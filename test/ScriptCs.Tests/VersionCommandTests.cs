using Common.Logging;
using Moq;
using ScriptCs.Command;
using ScriptCs.Package;
using System.IO;
using Xunit;

namespace ScriptCs.Tests
{
    public class VersionCommandTests
    {
        public class ExecuteMethod
        {
            private readonly System.Version _currentVersion;

            System.Text.StringBuilder _outputText;
            StringWriter _mockConsole;
            TextWriter _actualConsole;

            public ExecuteMethod()
            {
                _currentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                _outputText = new System.Text.StringBuilder();
                _mockConsole = new StringWriter(_outputText);
                _actualConsole = System.Console.Out;
                System.Console.SetOut(_mockConsole);
            }

            [Fact]
            public void VersionCommandShouldOutputVersion()
            {
                var args = new ScriptCsArgs
                    {
                        Version = true
                    };

                var fs = new Mock<IFileSystem>();
                var resolver = new Mock<IPackageAssemblyResolver>();
                var executor = new Mock<IScriptExecutor>();
                var scriptpackResolver = new Mock<IScriptPackResolver>();
                var packageInstaller = new Mock<IPackageInstaller>();
                var logger = new Mock<ILog>();
                var root = new ScriptServiceRoot(fs.Object, resolver.Object, executor.Object, scriptpackResolver.Object, packageInstaller.Object, logger.Object);

                var factory = new CommandFactory(root);
                var result = factory.CreateCommand(args);

                // clear the fake console output
                _outputText.Clear();

                result.Execute();

                Assert.Contains("ScriptCs version " + _currentVersion.ToString(), _outputText.ToString());
            }
        }
    }
}