using Common.Logging;
using Moq;
using ScriptCs.Argument;
using ScriptCs.Command;
using ScriptCs.Package;
using Xunit;

namespace ScriptCs.Tests
{
    public class CleanCommandTests
    {
        public class ExecuteMethod
        {
            [Fact]
            public void ShouldDeletePackagesFolder()
            {
                var args = new ScriptCsArgs { Clean = true };

                var fs = new Mock<IFileSystem>();
                var resolver = new Mock<IPackageAssemblyResolver>();
                var executor = new Mock<IScriptExecutor>();
                var engine = new Mock<IScriptEngine>();
                var scriptpackResolver = new Mock<IScriptPackResolver>();
                var packageInstaller = new Mock<IPackageInstaller>();
                var logger = new Mock<ILog>();
                var filePreProcessor = new Mock<IFilePreProcessor>();
                var assemblyName = new Mock<IAssemblyResolver>();
                var argumentHandler = new Mock<IArgumentHandler>();

                var argsParseResult = new ArgumentParseResult(new string[0], args, new string[0]);
                argumentHandler.Setup(i => i.GetParsedArguments()).Returns(argsParseResult);

                var root = new ScriptServiceRoot(fs.Object, resolver.Object, executor.Object, engine.Object, filePreProcessor.Object, scriptpackResolver.Object, packageInstaller.Object, logger.Object, assemblyName.Object, argumentHandler.Object);
                fs.Setup(i => i.DirectoryExists(It.Is<string>(x => x.Contains(Constants.PackagesFolder)))).Returns(true);
                fs.Setup(i => i.GetWorkingDirectory(It.IsAny<string>())).Returns("c:\\");

                var factory = new CommandFactory(root);
                var result = factory.CreateCommand();

                result.Execute();

                fs.Verify(i => i.DirectoryExists(It.Is<string>(x => x.Contains(Constants.PackagesFolder))), Times.Once());
                fs.Verify(i => i.DeleteDirectory(It.Is<string>(x => x.Contains(Constants.PackagesFolder))), Times.Once());
            }
        }
    }
}
