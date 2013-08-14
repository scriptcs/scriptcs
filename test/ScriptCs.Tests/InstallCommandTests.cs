using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;

using Moq;

using Ploeh.AutoFixture.Xunit;

using ScriptCs.Command;
using ScriptCs.Contracts;
using ScriptCs.Package;

using Xunit.Extensions;

namespace ScriptCs.Tests
{
    public class InstallCommandTests
    {
        public class ExecuteMethod
        {
            private const string CurrentDirectory = @"C:\";

            [Theory, ScriptCsAutoData]
            public void InstallCommandShouldInstallSinglePackageIfNamePassed(
                [Frozen] Mock<IFileSystem> fileSystem,
                [Frozen] Mock<IPackageInstaller> packageInstaller,
                CommandFactory factory)
            {
                // Arrange
                var args = new ScriptCsArgs { AllowPreRelease = false, Install = "mypackage", ScriptName = null };

                fileSystem.Setup(x => x.GetWorkingDirectory(It.IsAny<string>())).Returns(CurrentDirectory);
                fileSystem.SetupGet(x => x.CurrentDirectory).Returns(CurrentDirectory);

                // Act
                factory.CreateCommand(args, new string[0]).Execute();

                // Assert
                packageInstaller.Verify(i =>
                    i.InstallPackages(
                        It.Is<IEnumerable<IPackageReference>>(x => x.Count() == 1 && x.First().PackageId == "mypackage"),
                        It.IsAny<bool>(),
                        It.IsAny<Action<string>>()),
                    Times.Once());
            }

            [Theory, ScriptCsAutoData]
            public void InstallCommandShouldInstallFromPackagesConfigIfNoNamePassed(
                [Frozen] Mock<IFileSystem> fileSystem,
                [Frozen] Mock<IPackageAssemblyResolver> resolver,
                [Frozen] Mock<IPackageInstaller> packageInstaller,
                CommandFactory factory)
            {
                // Arrange
                var args = new ScriptCsArgs { AllowPreRelease = false, Install = "", ScriptName = null };

                fileSystem.Setup(x => x.GetWorkingDirectory(It.IsAny<string>())).Returns(CurrentDirectory);
                fileSystem.SetupGet(x => x.CurrentDirectory).Returns(CurrentDirectory);

                resolver.Setup(i => i.GetPackages(It.IsAny<string>())).Returns(new List<IPackageReference>
                    {
                        new PackageReference("a", new FrameworkName(".NETFramework,Version=v4.0"), new Version()),
                        new PackageReference("b", new FrameworkName(".NETFramework,Version=v4.0"), new Version())
                    });

                // Act
                factory.CreateCommand(args, new string[0]).Execute();

                // Assert
                packageInstaller.Verify(i => i.InstallPackages(It.Is<IEnumerable<IPackageReference>>(x => x.Count() == 2), It.IsAny<bool>(), It.IsAny<Action<string>>()), Times.Once());
            }
        }
    }
}
