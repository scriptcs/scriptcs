using System;
using System.Collections.Generic;
using Moq;
using NuGet;
using ScriptCs.Contracts;
using ScriptCs.Hosting.Package;
using ScriptCs.Tests;
using Should;
using Xunit;

namespace ScriptCs.Hosting.Tests
{
    public class PackageInstallerTests
    {
        public class InstallPackagesMethod
        {
            [Fact]
            public void ShouldThrowArgumentNullExWhenNoPackageIdsPassed()
            {
                var installer = new PackageInstaller(new Mock<IInstallationProvider>().Object, new TestLogProvider());
                Assert.Throws<ArgumentNullException>(() => installer.InstallPackages(null));
            }

            [Fact]
            public void ShouldInstallAllPassedPackages()
            {
                var provider = new Mock<IInstallationProvider>();

                var references = new List<IPackageReference> 
                {
                    new PackageReference("testId", VersionUtility.ParseFrameworkName("net40"), new Version("3.0")),
                    new PackageReference("testId2", VersionUtility.ParseFrameworkName("net40"), new Version("4.0")),
                    new PackageReference("testId3", VersionUtility.ParseFrameworkName("net40"), new Version("5.0"))
                };

                var installer = new PackageInstaller(provider.Object, new TestLogProvider());
                installer.InstallPackages(references);

                provider.Verify(i => i.InstallPackage(It.IsAny<IPackageReference>(), It.IsAny<bool>()), Times.Exactly(3));
            }

            [Fact]
            public void ShouldShowErrorIfOneOfPackagesFail()
            {
                var provider = new Mock<IInstallationProvider>();
                provider.Setup(
                    i => i.InstallPackage(It.Is<IPackageReference>(x => x.PackageId == "testId"), It.IsAny<bool>()))
                        .Throws<Exception>();

                var references = new List<IPackageReference> 
                {
                    new PackageReference("testId", VersionUtility.ParseFrameworkName("net40"), new Version("3.0")),
                    new PackageReference("testId2", VersionUtility.ParseFrameworkName("net40"), new Version("4.0")),
                    new PackageReference("testId3", VersionUtility.ParseFrameworkName("net40"), new Version("5.0"))
                };

                var installer = new PackageInstaller(provider.Object, new TestLogProvider());
                var exception = Record.Exception(() => installer.InstallPackages(references, true));

                provider.Verify(i => i.InstallPackage(It.IsAny<IPackageReference>(), It.IsAny<bool>()), Times.Exactly(3));
                exception.ShouldBeType<AggregateException>();
                ((AggregateException)exception).InnerExceptions.Count.ShouldEqual(1);
            }

            [Fact]
            public void ShouldNotInstallExistingPackages()
            {
                var provider = new Mock<IInstallationProvider>();
                provider.Setup(
                    i => i.IsInstalled(It.Is<IPackageReference>(x => x.PackageId == "testId"), It.IsAny<bool>()))
                        .Returns(true);

                var references = new List<IPackageReference>
                {
                    new PackageReference("testId", VersionUtility.ParseFrameworkName("net40"), new Version("3.0")),
                    new PackageReference("testId2", VersionUtility.ParseFrameworkName("net40"), new Version("4.0")),
                    new PackageReference("testId3", VersionUtility.ParseFrameworkName("net40"), new Version("5.0"))
                };

                var installer = new PackageInstaller(provider.Object, new TestLogProvider());
                installer.InstallPackages(references);

                provider.Verify(i => i.InstallPackage(It.Is<IPackageReference>(x => x.PackageId == "testId"), It.IsAny<bool>()), Times.Never());
            }
        }
    }
}