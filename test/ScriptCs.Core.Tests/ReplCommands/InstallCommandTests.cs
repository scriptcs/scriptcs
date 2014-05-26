using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using Common.Logging;
using Moq;
using ScriptCs.Contracts;
using ScriptCs.ReplCommands;
using Xunit;

namespace ScriptCs.Tests.ReplCommands
{
    public class InstallCommandTests
    {
        public class CommandNameProperty
        {
            [Fact]
            public void ReturnsInstall()
            {
                var cmd = new InstallCommand(null, null, null, null);
                Assert.Equal("install", cmd.CommandName);
            }
        }

        public class ExecuteMethod
        {
            private readonly Mock<ILog> _logger;
            private readonly Mock<IPackageAssemblyResolver> _packageAssemblyResolver;
            private readonly Mock<IPackageInstaller> _packageInstaller;
            private readonly Mock<IInstallationProvider> _installationProvider;
            private Mock<IScriptExecutor> _executor;

            public ExecuteMethod()
            {
                _packageInstaller = new Mock<IPackageInstaller>();
                _installationProvider = new Mock<IInstallationProvider>();
                _packageAssemblyResolver = new Mock<IPackageAssemblyResolver>();
                _logger = new Mock<ILog>();

                var fs = new Mock<IFileSystem>();
                fs.Setup(x => x.CurrentDirectory).Returns(@"c:\dir");

                _executor = new Mock<IScriptExecutor>();
                _executor.SetupGet(x => x.FileSystem).Returns(fs.Object);
                _executor.SetupGet(x => x.References).Returns(new AssemblyReferences());
            }

            private InstallCommand GetCommand()
            {
                return new InstallCommand(_packageInstaller.Object, _packageAssemblyResolver.Object, _logger.Object, _installationProvider.Object);
            }

            [Fact]
            public void IfArgsAreEmptyReturns()
            {
                var cmd = GetCommand();
                var result = cmd.Execute(_executor.Object, null);

                Assert.Null(result);
            }

            [Fact]
            public void InstallsPackageUsingArg0AsName()
            {
                var cmd = GetCommand();
                var result = cmd.Execute(_executor.Object, new []{ "scriptcs"});

                _packageInstaller.Verify(x => x.InstallPackages(It.Is<IEnumerable<IPackageReference>>(i => i.ElementAt(0).PackageId == "scriptcs"), false), Times.Once);
            }

            [Fact]
            public void InstallsPackageUsingArg1AsVersion()
            {
                var cmd = GetCommand();
                var result = cmd.Execute(_executor.Object, new[] { "scriptcs", "0.9" });
                var packageRef = new PackageReference("scriptcs", new FrameworkName(".NETFramework,Version=v4.0"), "0.9");

                _packageInstaller.Verify(x => x.InstallPackages(It.Is<IEnumerable<IPackageReference>>(i => i.ElementAt(0).Version == packageRef.Version), false), Times.Once);
            }

            [Fact]
            public void InstallsPackageUsingArg2AsPreReleaseFlag()
            {
                var cmd = GetCommand();
                var result = cmd.Execute(_executor.Object, new[] { "scriptcs", "0.9", "pre" });

                _packageInstaller.Verify(x => x.InstallPackages(It.IsAny<IEnumerable<IPackageReference>>(), true), Times.Once);
            }

            [Fact]
            public void AfterInstallationSavesPackages()
            {
                var cmd = GetCommand();
                var result = cmd.Execute(_executor.Object, new[] { "scriptcs" });

                _packageAssemblyResolver.Verify(x => x.SavePackages(), Times.Once);
            }

            [Fact]
            public void AddsAssemlbyReferenceToRepl()
            {
                var dummyAssemblies = new[] {"assembly1.dll", "assembly2.dll"};
                _packageAssemblyResolver.Setup(x => x.GetAssemblyNames(@"c:\dir")).Returns(dummyAssemblies);

                var cmd = GetCommand();
                var result = cmd.Execute(_executor.Object, new[] { "scriptcs" });

                _executor.Verify(x => x.AddReferences(dummyAssemblies), Times.Once);
            }
        }
    }
}