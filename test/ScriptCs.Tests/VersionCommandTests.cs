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
                var engine = new Mock<IScriptEngine>();
                var scriptpackResolver = new Mock<IScriptPackResolver>();
                var packageInstaller = new Mock<IPackageInstaller>();
                var logger = new Mock<ILog>();
                var filePreProcessor = new Mock<IFilePreProcessor>();
                var assemblyName = new Mock<IAssemblyName>();
                var root = new ScriptServiceRoot(fs.Object, resolver.Object, executor.Object, engine.Object, filePreProcessor.Object, scriptpackResolver.Object, packageInstaller.Object, logger.Object, assemblyName.Object);

                var factory = new CommandFactory(root);
                var result = factory.CreateCommand(args, new string[0]);

                // clear the fake console output
                _outputText.Clear();

                result.Execute();

                Assert.Contains("scriptcs version " + _currentVersion.ToString(), _outputText.ToString());
            }
        }
    }
}
