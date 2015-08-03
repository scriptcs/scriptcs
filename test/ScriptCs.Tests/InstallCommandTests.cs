using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Xunit;
using ScriptCs.Command;
using ScriptCs.Contracts;
using ScriptCs.Hosting;
using Xunit.Extensions;

namespace ScriptCs.Tests
{
    public class InstallCommandTests
    {
        public class ExecuteMethod
        {
            [Theory, ScriptCsAutoData]
            public void InstallCommandShouldInstallSinglePackageIfNamePassed(
                [Frozen] Mock<IFileSystem> fileSystem,
                [Frozen] Mock<IPackageInstaller> packageInstaller,
                [Frozen] Mock<IPackageAssemblyResolver> resolver,
                [Frozen] Mock<IInitializationServices> initializationServices,
                ScriptServices services)
            {
                // Arrange
                var args = new Config { AllowPreRelease = false, PackageName = "mypackage", };
                var fixture = new Fixture().Customize(new AutoMoqCustomization());
                var servicesBuilder = fixture.Freeze<Mock<IScriptServicesBuilder>>();

                initializationServices.Setup(i => i.GetFileSystem()).Returns(fileSystem.Object);
                initializationServices.Setup(i => i.GetPackageInstaller()).Returns(packageInstaller.Object);
                initializationServices.Setup(i => i.GetPackageAssemblyResolver()).Returns(resolver.Object);

                servicesBuilder.Setup(b => b.Build()).Returns(services);
                servicesBuilder.SetupGet(b => b.InitializationServices).Returns(initializationServices.Object);
                var factory = fixture.Create<CommandFactory>();

                // Act
                factory.CreateCommand(args, new string[0]).Execute();

                // Assert
                packageInstaller.Verify(
                    i => i.InstallPackages(
                        It.Is<IEnumerable<IPackageReference>>(x => x.Count() == 1 && x.First().PackageId == "mypackage"),
                        It.IsAny<bool>()),
                    Times.Once());
            }

            [Theory, ScriptCsAutoData]
            public void InstallCommandShouldInstallFromPackagesConfigIfNoNamePassed(
                [Frozen] Mock<IPackageInstaller> packageInstaller,
                [Frozen] Mock<IFileSystem> fileSystem,
                [Frozen] Mock<IPackageAssemblyResolver> resolver,
                [Frozen] Mock<IInitializationServices> initializationServices,
                [Frozen] Mock<IScriptServicesBuilder> servicesBuilder,
                ScriptServices services)
            {
                // Arrange
                var args = new Config { AllowPreRelease = false, PackageName = string.Empty, };

                initializationServices.Setup(i => i.GetFileSystem()).Returns(fileSystem.Object);
                initializationServices.Setup(i => i.GetPackageInstaller()).Returns(packageInstaller.Object);
                initializationServices.Setup(i => i.GetPackageAssemblyResolver()).Returns(resolver.Object);

                servicesBuilder.Setup(b => b.Build()).Returns(services);
                servicesBuilder.SetupGet(b => b.InitializationServices).Returns(initializationServices.Object);

                resolver.Setup(i => i.GetPackages(It.IsAny<string>())).Returns(new List<IPackageReference>
                    {
                        new PackageReference("a", new FrameworkName(".NETFramework,Version=v4.0"), new Version()),
                        new PackageReference("b", new FrameworkName(".NETFramework,Version=v4.0"), new Version())
                    });

                var factory = new CommandFactory(servicesBuilder.Object);
                var sut = factory.CreateCommand(args, new string[0]);

                // act
                sut.Execute();

                // Assert
                packageInstaller.Verify(i => i.InstallPackages(It.Is<IEnumerable<IPackageReference>>(x => x.Count() == 2), It.IsAny<bool>()), Times.Once());
            }
        }
    }
}
