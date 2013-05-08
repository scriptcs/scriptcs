using System.Collections.Generic;
using System.IO;
using Common.Logging;
using Moq;
using ScriptCs.Command;
using ScriptCs.Contracts;
using ScriptCs.Package;
using Xunit;

namespace ScriptCs.Tests
{
    public class ExecuteScriptCommandTests
    {
        public class ExecuteMethod
        {
            [Fact]
            public void ScriptExecCommandShouldInvokeWithScriptPassedFromArgs()
            {
                var args = new ScriptCsArgs
                    {
                        AllowPreReleaseFlag = false,
                        Install = "",
                        ScriptName = "test.csx"
                    };

                var fs = new Mock<IFileSystem>();
                var resolver = new Mock<IPackageAssemblyResolver>();
                var executor = new Mock<IScriptExecutor>();
                var engine = new Mock<IScriptEngine>();
                var scriptpackResolver = new Mock<IScriptPackResolver>();
                var packageInstaller = new Mock<IPackageInstaller>();
                var logger = new Mock<ILog>();
                var filePreProcessor = new Mock<IFilePreProcessor>();
                var root = new ScriptServiceRoot(fs.Object, resolver.Object, executor.Object, engine.Object, filePreProcessor.Object, scriptpackResolver.Object, packageInstaller.Object, logger.Object);

                var factory = new CommandFactory(root);
                var result = factory.CreateCommand(args);

                result.Execute();

                executor.Verify(i => i.Execute(It.Is<string>(x => x == "test.csx"), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<IScriptPack>>()), Times.Once());
            }

            [Fact]
            public void ShouldCreateMissingBinFolder()
            {
                const string WorkingDirectory = @"C:\";

                var binFolder = Path.Combine(WorkingDirectory, "bin");

                var args = new ScriptCsArgs { ScriptName = "test.csx" };

                var fs = new Mock<IFileSystem>();
                fs.Setup(x => x.GetWorkingDirectory(It.IsAny<string>())).Returns(WorkingDirectory);
                fs.Setup(x => x.DirectoryExists(binFolder)).Returns(false);

                var resolver = new Mock<IPackageAssemblyResolver>();
                var executor = new Mock<IScriptExecutor>();
                var engine = new Mock<IScriptEngine>();
                var scriptpackResolver = new Mock<IScriptPackResolver>();
                var packageInstaller = new Mock<IPackageInstaller>();
                var logger = new Mock<ILog>();
                var filePreProcessor = new Mock<IFilePreProcessor>();
                var root = new ScriptServiceRoot(fs.Object, resolver.Object, executor.Object, engine.Object, filePreProcessor.Object, scriptpackResolver.Object, packageInstaller.Object, logger.Object);

                var factory = new CommandFactory(root);
                var result = factory.CreateCommand(args);

                result.Execute();

                fs.Verify(x => x.CreateDirectory(binFolder), Times.Once());
            }
        }
    }
}