using System;
using Common.Logging;
using Moq;
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
                var assemblyName = new Mock<IAssemblyName>();

                var root = new ScriptServiceRoot(fs.Object, resolver.Object, executor.Object, engine.Object, filePreProcessor.Object, scriptpackResolver.Object, packageInstaller.Object, logger.Object, assemblyName.Object);
                fs.Setup(i => i.DirectoryExists(It.Is<string>(x => x.Contains(Constants.PackagesFolder)))).Returns(true);
                fs.Setup(i => i.GetWorkingDirectory(It.IsAny<string>())).Returns("c:\\");

                var factory = new CommandFactory(root);
                var result = factory.CreateCommand(args, new string[0]);

                result.Execute();

                fs.Verify(i => i.DirectoryExists(It.Is<string>(x => x.Contains(Constants.PackagesFolder))), Times.Once());
                fs.Verify(i => i.DeleteDirectory(It.Is<string>(x => x.Contains(Constants.PackagesFolder))), Times.Once());
            }

            [Fact]
            public void ShouldDeleteBinFolder()
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
                var assemblyName = new Mock<IAssemblyName>();
                var root = new ScriptServiceRoot(fs.Object, resolver.Object, executor.Object, engine.Object, filePreProcessor.Object, scriptpackResolver.Object, packageInstaller.Object, logger.Object, assemblyName.Object);
                fs.Setup(i => i.DirectoryExists(It.Is<string>(x => x.Contains(Constants.BinFolder)))).Returns(true);
                fs.Setup(i => i.GetWorkingDirectory(It.IsAny<string>())).Returns("c:\\");

                var factory = new CommandFactory(root);
                var result = factory.CreateCommand(args, new string[0]);

                result.Execute();

                fs.Verify(i => i.DirectoryExists(It.Is<string>(x => x.Contains(Constants.BinFolder))), Times.Once());
                fs.Verify(i => i.DeleteDirectory(It.Is<string>(x => x.Contains(Constants.BinFolder))), Times.Once());
            }

            [Fact]
            public void ShouldNotDeleteBinFolderIfDllsAreLeft()
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
                var assemblyName = new Mock<IAssemblyName>();
                var root = new ScriptServiceRoot(fs.Object, resolver.Object, executor.Object, engine.Object, filePreProcessor.Object, scriptpackResolver.Object, packageInstaller.Object, logger.Object, assemblyName.Object);

                fs.Setup(i => i.DirectoryExists(It.Is<string>(x => x.Contains(Constants.BinFolder)))).Returns(true);
                fs.Setup(i => i.GetWorkingDirectory(It.IsAny<string>())).Returns("c:/");
                fs.Setup(i => i.EnumerateFiles(It.IsAny<string>(), It.IsAny<string>())).Returns(new[] { "c:/file.dll", "c:/file2.dll" });

                var factory = new CommandFactory(root);
                var result = factory.CreateCommand(args, new string[0]);

                result.Execute();

                fs.Verify(i => i.DeleteDirectory(It.Is<string>(x => x.Contains(Constants.BinFolder))), Times.Never());
            }

            [Fact]
            public void ShouldDeleteAllFilesResolvedFromPackages()
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
                var assemblyName = new Mock<IAssemblyName>();
                var root = new ScriptServiceRoot(fs.Object, resolver.Object, executor.Object, engine.Object, filePreProcessor.Object, scriptpackResolver.Object, packageInstaller.Object, logger.Object, assemblyName.Object);

                fs.Setup(i => i.DirectoryExists(It.Is<string>(x => x.Contains(Constants.BinFolder)))).Returns(true);
                fs.Setup(i => i.GetWorkingDirectory(It.IsAny<string>())).Returns("c:\\");
                fs.Setup(i => i.FileExists(It.IsAny<string>())).Returns(true);
                resolver.Setup(i => i.GetAssemblyNames(It.IsAny<string>(), It.IsAny<Action<string>>())).Returns(new[] { "c:\\file.dll", "c:\\file2.dll" });

                var factory = new CommandFactory(root);
                var result = factory.CreateCommand(args, new string[0]);

                result.Execute();

                fs.Verify(i => i.FileDelete(It.IsAny<string>()), Times.Exactly(2));
            }
        }
    }
}
