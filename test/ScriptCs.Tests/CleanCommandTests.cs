using System;
using Moq;

using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;

using ScriptCs.Command;
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

                var fixture = new Fixture().Customize(new AutoMoqCustomization());

                var fs = new Mock<IFileSystem>();
                fs.Setup(i => i.DirectoryExists(It.Is<string>(x => x.Contains(Constants.PackagesFolder)))).Returns(true);
                fs.Setup(i => i.GetWorkingDirectory(It.IsAny<string>())).Returns("c:\\");

                fixture.Register(() => fs.Object);

                var root = fixture.Create<ScriptServiceRoot>();

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

                var fixture = new Fixture().Customize(new AutoMoqCustomization());

                var fs = new Mock<IFileSystem>();
                fs.Setup(i => i.DirectoryExists(It.Is<string>(x => x.Contains(Constants.BinFolder)))).Returns(true);
                fs.Setup(i => i.GetWorkingDirectory(It.IsAny<string>())).Returns("c:\\");

                fixture.Register(() => fs.Object);

                var root = fixture.Create<ScriptServiceRoot>();

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

                var fixture = new Fixture().Customize(new AutoMoqCustomization());

                var fs = new Mock<IFileSystem>();
                fs.Setup(i => i.DirectoryExists(It.Is<string>(x => x.Contains(Constants.BinFolder)))).Returns(true);
                fs.Setup(i => i.GetWorkingDirectory(It.IsAny<string>())).Returns("c:/");
                fs.Setup(i => i.EnumerateFiles(It.IsAny<string>(), It.IsAny<string>())).Returns(new[] { "c:/file.dll", "c:/file2.dll" });

                fixture.Register(() => fs.Object);

                var root = fixture.Create<ScriptServiceRoot>();

                var factory = new CommandFactory(root);
                var result = factory.CreateCommand(args, new string[0]);

                result.Execute();

                fs.Verify(i => i.DeleteDirectory(It.Is<string>(x => x.Contains(Constants.BinFolder))), Times.Never());
            }

            [Fact]
            public void ShouldDeleteAllFilesResolvedFromPackages()
            {
                var args = new ScriptCsArgs { Clean = true };

                var fixture = new Fixture().Customize(new AutoMoqCustomization());

                var fs = new Mock<IFileSystem>();
                fs.Setup(i => i.DirectoryExists(It.Is<string>(x => x.Contains(Constants.BinFolder)))).Returns(true);
                fs.Setup(i => i.GetWorkingDirectory(It.IsAny<string>())).Returns("c:\\");
                fs.Setup(i => i.FileExists(It.IsAny<string>())).Returns(true);

                var resolver = new Mock<IPackageAssemblyResolver>();
                resolver.Setup(i => i.GetAssemblyNames(It.IsAny<string>(), It.IsAny<Action<string>>())).Returns(new[] { "c:\\file.dll", "c:\\file2.dll" });

                fixture.Register(() => fs.Object);
                fixture.Register(() => resolver.Object);

                var root = fixture.Create<ScriptServiceRoot>();

                var factory = new CommandFactory(root);
                var result = factory.CreateCommand(args, new string[0]);

                result.Execute();

                fs.Verify(i => i.FileDelete(It.IsAny<string>()), Times.Exactly(2));
            }
        }
    }
}
