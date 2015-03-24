using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using ScriptCs.Contracts;
using ScriptCs.Logging;

namespace ScriptCs.Tests
{
    public class ScriptCsMoqCustomization : AutoMoqCustomization, ICustomization
    {
        void ICustomization.Customize(IFixture fixture)
        {
            this.Customize(fixture);

            fixture.Register(() =>
                {
                    var fileSystem = new Mock<IFileSystem>();
                    fileSystem.SetupGet(f => f.PackagesFile).Returns("scriptcs_packages.config");
                    fileSystem.SetupGet(f => f.PackagesFolder).Returns("scriptcs_packages");
                    fileSystem.SetupGet(f => f.BinFolder).Returns("scriptcs_bin");
                    fileSystem.SetupGet(f => f.DllCacheFolder).Returns(".scriptcs_cache");
                    fileSystem.SetupGet(f => f.NugetFile).Returns("scriptcs_nuget.config");
                    fileSystem.SetupGet(f => f.CurrentDirectory).Returns("workingdirectory");
                    fileSystem.Setup(f => f.FileExists(@"workingdirectory\scriptcs_packages\PackageScripts.csx")).Returns(false);
                    fileSystem.Setup(f => f.DirectoryExists(@"workingdirectory\scriptcs_packages")).Returns(true);
                    fileSystem.Setup(f => f.GetWorkingDirectory(It.IsAny<string>())).Returns("workingdirectory");
                    return fileSystem;
                });

            fixture.Register(() =>
                {
                    var composer = new Mock<IScriptLibraryComposer>();
                    composer.SetupGet(c => c.ScriptLibrariesFile).Returns("ScriptLibraries.csx");
                    return composer;
                });

            fixture.Register<ILog>(fixture.Create<TestLogger>);
        }
    }
}
