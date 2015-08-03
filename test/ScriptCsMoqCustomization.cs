using System.Collections.Generic;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using ScriptCs.Contracts;

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

            var logProvider = new TestLogProvider();
            fixture.Register(() => logProvider);
            fixture.Register<ILogProvider>(() => logProvider);

            fixture.Register(()=>new AppDomainAssemblyResolver(
                fixture.Create<ILogProvider>(),
                fixture.Create<IFileSystem>(),
                fixture.Create<IAssemblyResolver>(),
                fixture.Create<IAssemblyUtility>(),
                fixture.Create<IDictionary<string, AssemblyInfo>>()));

            fixture.Register(()=> new ScriptLibraryComposer(
                fixture.Create<IFileSystem>(),
                fixture.Create<IFilePreProcessor>(),
                fixture.Create<IPackageContainer>(),
                fixture.Create<IPackageAssemblyResolver>(),
                fixture.Create<ILogProvider>()));

            fixture.Register(() => new ScriptServices(
                fixture.Create<IFileSystem>(),
                fixture.Create<IPackageAssemblyResolver>(),
                fixture.Create<IScriptExecutor>(),
                fixture.Create<IRepl>(),
                fixture.Create<IScriptEngine>(),
                fixture.Create<IFilePreProcessor>(),
                fixture.Create<IScriptPackResolver>(),
                fixture.Create<IPackageInstaller>(),
                fixture.Create<IObjectSerializer>(),
                fixture.Create<ILogProvider>(),
                fixture.Create<IAssemblyResolver>(),
                fixture.Create<IEnumerable<IReplCommand>>(),
                fixture.Create<IFileSystemMigrator>(),
                fixture.Create<IConsole>(),
                fixture.Create<IInstallationProvider>(),
                fixture.Create<IScriptLibraryComposer>()));
        }
    }
}
