using System;
using System.IO;
using Common.Logging;
using Moq;
using ScriptCs.Command;
using ScriptCs.Package;
using Xunit;

namespace ScriptCs.Tests
{
    public class RestoreCommandTests
    {
        public class TheExecuteMethod
        {
            [Fact]
            public void ShouldNotCopyFilesInPathIfLastWriteTimeEqualsLastWriteTimeOfFileInBin()
            {
                var args = new ScriptCsArgs { Restore = true, ScriptName = "" };

                var fs = new Mock<IFileSystem>();
                var resolver = new Mock<IPackageAssemblyResolver>();
                var executor = new Mock<IScriptExecutor>();
                var scriptpackResolver = new Mock<IScriptPackResolver>();
                var packageInstaller = new Mock<IPackageInstaller>();
                var logger = new Mock<ILog>();
                var root = new ScriptServiceRoot(fs.Object, resolver.Object, executor.Object, scriptpackResolver.Object, packageInstaller.Object, logger.Object);

                const string CurrentDirectory = @"C:\";

                var sourceFilePath = Path.Combine(CurrentDirectory, "fileName.cs");
                var sourceWriteTime = new DateTime(2013, 3, 7);

                var destFilePath = Path.Combine(CurrentDirectory, "bin", "fileName.cs");
                var destWriteTime = sourceWriteTime;

                fs.Setup(x => x.GetWorkingDirectory(It.IsAny<string>())).Returns(CurrentDirectory);

                fs.Setup(x => x.GetLastWriteTime(sourceFilePath)).Returns(sourceWriteTime).Verifiable();
                fs.Setup(x => x.GetLastWriteTime(destFilePath)).Returns(destWriteTime).Verifiable();

                resolver.Setup(i => i.GetAssemblyNames(CurrentDirectory, It.IsAny<Action<string>>())).Returns(new[] { sourceFilePath });

                var factory = new CommandFactory(root);
                var result = factory.CreateCommand(args);

                result.Execute();

                fs.Verify(x => x.Copy(sourceFilePath, destFilePath, true), Times.Never());
                fs.Verify(x => x.GetLastWriteTime(sourceFilePath), Times.Once());
                fs.Verify(x => x.GetLastWriteTime(destFilePath), Times.Once());
            }

            [Fact]
            public void ShouldCopyFilesInPathIfLastWriteTimeDiffersFromLastWriteTimeOfFileInBin()
            {
                var args = new ScriptCsArgs { Restore = true, ScriptName = "" };

                var fs = new Mock<IFileSystem>();
                var resolver = new Mock<IPackageAssemblyResolver>();
                var executor = new Mock<IScriptExecutor>();
                var scriptpackResolver = new Mock<IScriptPackResolver>();
                var packageInstaller = new Mock<IPackageInstaller>();
                var logger = new Mock<ILog>();
                var root = new ScriptServiceRoot(fs.Object, resolver.Object, executor.Object, scriptpackResolver.Object, packageInstaller.Object, logger.Object);

                const string CurrentDirectory = @"C:\";

                var sourceFilePath = Path.Combine(CurrentDirectory, "fileName.cs");
                var sourceWriteTime = new DateTime(2013, 3, 7);

                var destFilePath = Path.Combine(CurrentDirectory, "bin", "fileName.cs");
                var destWriteTime = new DateTime(2013, 2, 7);

                fs.Setup(x => x.GetWorkingDirectory(It.IsAny<string>())).Returns(CurrentDirectory);

                fs.Setup(x => x.GetLastWriteTime(sourceFilePath)).Returns(sourceWriteTime).Verifiable();
                fs.Setup(x => x.GetLastWriteTime(destFilePath)).Returns(destWriteTime).Verifiable();

                resolver.Setup(i => i.GetAssemblyNames(CurrentDirectory, It.IsAny<Action<string>>())).Returns(new[] { sourceFilePath });

                var factory = new CommandFactory(root);
                var result = factory.CreateCommand(args);

                result.Execute();

                fs.Verify(x => x.Copy(sourceFilePath, destFilePath, true), Times.Once());
                fs.Verify(x => x.GetLastWriteTime(sourceFilePath), Times.Once());
                fs.Verify(x => x.GetLastWriteTime(destFilePath), Times.Once());
            }

            [Fact]
            public void ShouldCreateBinFolderIfItDoesNotExist()
            {
                var args = new ScriptCsArgs { Restore = true, ScriptName = "" };

                var fs = new Mock<IFileSystem>();
                var resolver = new Mock<IPackageAssemblyResolver>();
                var executor = new Mock<IScriptExecutor>();
                var scriptpackResolver = new Mock<IScriptPackResolver>();
                var packageInstaller = new Mock<IPackageInstaller>();
                var logger = new Mock<ILog>();
                var root = new ScriptServiceRoot(fs.Object, resolver.Object, executor.Object, scriptpackResolver.Object, packageInstaller.Object, logger.Object);

                const string CurrentDirectory = @"C:\";
                const string BinFolder = @"C:\bin";

                fs.Setup(x => x.GetWorkingDirectory(It.IsAny<string>())).Returns(CurrentDirectory);

                var binFolderCreated = false;

                fs.Setup(x => x.DirectoryExists(BinFolder)).Returns(() => binFolderCreated).Verifiable();
                fs.Setup(x => x.CreateDirectory(BinFolder)).Callback(() => binFolderCreated = true).Verifiable();

                var factory = new CommandFactory(root);
                var result = factory.CreateCommand(args);

                result.Execute();

                fs.Verify(x => x.DirectoryExists(BinFolder), Times.AtLeastOnce());
                fs.Verify(x => x.CreateDirectory(BinFolder), Times.Once());
            }
        }
    }
}