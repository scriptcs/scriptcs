using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using Moq;
using ScriptCs.Contracts;
using ScriptCs.ReplCommands;
using Xunit;
using Xunit.Extensions;

namespace ScriptCs.Tests.ReplCommands
{
    public class InstallCommandTests
    {
        public class CommandNameProperty
        {
            [Fact]
            public void ReturnsInstall()
            {
                // act
                var cmd = new InstallCommand(
                    new Mock<IPackageInstaller>().Object,
                    new Mock<IPackageAssemblyResolver>().Object,
                    new TestLogProvider(),
                    new Mock<IInstallationProvider>().Object);

                // assert
                Assert.Equal("install", cmd.CommandName);
            }
        }

        public class ExecuteMethod
        {
            private readonly Mock<IPackageInstaller> _packageInstaller;
            private readonly Mock<IPackageAssemblyResolver> _packageAssemblyResolver;
            private readonly TestLogProvider _logProvider = new TestLogProvider();
            private readonly Mock<IInstallationProvider> _installationProvider;
            private readonly Mock<IRepl> _repl;

            public ExecuteMethod()
            {
                _packageInstaller = new Mock<IPackageInstaller>();
                _packageAssemblyResolver = new Mock<IPackageAssemblyResolver>();
                _installationProvider = new Mock<IInstallationProvider>();
                _repl = new Mock<IRepl>();

                var fs = new Mock<IFileSystem>();
                fs.Setup(x => x.CurrentDirectory).Returns(@"c:\dir");

                _repl.SetupGet(x => x.FileSystem).Returns(fs.Object);
                _repl.SetupGet(x => x.References).Returns(new AssemblyReferences());
            }

            private InstallCommand GetCommand()
            {
                return new InstallCommand(_packageInstaller.Object, _packageAssemblyResolver.Object, _logProvider, _installationProvider.Object);
            }

            [Fact]
            public void IfArgsAreEmptyReturns()
            {
                // arrange
                var cmd = GetCommand();

                // act
                var result = cmd.Execute(_repl.Object, null);

                // assert
                Assert.Null(result);
            }

            [Fact]
            public void InstallsPackageUsingArg0AsName()
            {
                // arrange
                var cmd = GetCommand();

                // act
                cmd.Execute(_repl.Object, new[] { "scriptcs" });

                // assert
                _packageInstaller.Verify(
                    x => x.InstallPackages(
                        It.Is<IEnumerable<IPackageReference>>(i => i.ElementAt(0).PackageId == "scriptcs"), false),
                    Times.Once);
            }

            [Fact]
            public void InstallsPackageUsingArg1AsVersion()
            {
                // arrange
                var cmd = GetCommand();

                // act
                cmd.Execute(_repl.Object, new[] { "scriptcs", "0.9" });

                // assert
                var packageRef = new PackageReference("scriptcs", new FrameworkName(".NETFramework,Version=v4.0"), "0.9");
                _packageInstaller.Verify(
                    x => x.InstallPackages(
                        It.Is<IEnumerable<IPackageReference>>(i => i.ElementAt(0).Version == packageRef.Version),
                        false),
                    Times.Once);
            }

            [Theory]
            [InlineData("pre")]
            [InlineData("Pre")]
            public void InstallsPackageUsingArg2AsPreReleaseFlag(string preReleaseFlag)
            {
                // arrange
                var cmd = GetCommand();

                // act
                cmd.Execute(_repl.Object, new[] { "scriptcs", "0.9", preReleaseFlag });

                // assert
                _packageInstaller.Verify(
                    x => x.InstallPackages(It.IsAny<IEnumerable<IPackageReference>>(), true), Times.Once);
            }

            [Fact]
            public void AfterInstallationSavesPackages()
            {
                // arrange
                var cmd = GetCommand();

                // act
                cmd.Execute(_repl.Object, new[] { "scriptcs" });

                // assert
                _packageAssemblyResolver.Verify(x => x.SavePackages(), Times.Once);
            }

            [Fact]
            public void AddsAssemlbyReferenceToRepl()
            {
                // arrange
                var dummyAssemblies = new[] { "assembly1.dll", "assembly2.dll" };
                _packageAssemblyResolver.Setup(x => x.GetAssemblyNames(@"c:\dir")).Returns(dummyAssemblies);

                var cmd = GetCommand();

                // act
                cmd.Execute(_repl.Object, new[] { "scriptcs" });

                // assert
                _repl.Verify(x => x.AddReferences(dummyAssemblies), Times.Once);
            }
        }
    }
}