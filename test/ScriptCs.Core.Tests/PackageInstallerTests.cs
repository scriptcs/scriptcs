using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NuGet;
using ScriptCs.Package;
using ScriptCs.Package.InstallationProvider;
using Should;
using Xunit;
using PackageReference = ScriptCs.Package.PackageReference;

namespace ScriptCs.Tests
{
    public class PackageInstallerTests
    {
        public class InstallPackagesMethod
        {
            [Fact]
            public void ShouldThrowArgumentNullExWhenNoPackageIdsPassed()
            {
                var installer = new PackageInstaller(new Mock<IInstallationProvider>().Object);

                Assert.Throws<ArgumentNullException>(() => installer.InstallPackages(null));
            }

            [Fact]
            public void ShouldInstallAllPassedPackages()
            {
                var provider = new Mock<IInstallationProvider>();
                provider.Setup(
                    i => i.InstallPackage(It.IsAny<IPackageReference>(), It.IsAny<bool>(), It.IsAny<Action<string>>()))
                        .Returns(true);

                var references = new List<IPackageReference> {
                    new PackageReference("testId", VersionUtility.ParseFrameworkName("net40"), new Version("3.0")),
                    new PackageReference("testId2", VersionUtility.ParseFrameworkName("net40"), new Version("4.0")),
                    new PackageReference("testId3", VersionUtility.ParseFrameworkName("net40"), new Version("5.0"))
                };

                var installer = new PackageInstaller(provider.Object);
                installer.InstallPackages(references);

                provider.Verify(i => i.InstallPackage(It.IsAny<IPackageReference>(), It.IsAny<bool>(), It.IsAny<Action<string>>()), Times.Exactly(3));
            }

            [Fact]
            public void ShouldShowErrorIfOneOfPackagesFail()
            {
                var callbacks = new List<string>();
                var provider = new Mock<IInstallationProvider>();
                provider.Setup(
                    i => i.InstallPackage(It.IsAny<IPackageReference>(), It.IsAny<bool>(), It.IsAny<Action<string>>()))
                        .Returns(true);
                provider.Setup(
                    i => i.InstallPackage(It.Is<IPackageReference>(x => x.PackageId == "testId"), It.IsAny<bool>(), It.IsAny<Action<string>>()))
                        .Returns(false);

                var references = new List<IPackageReference> {
                    new PackageReference("testId", VersionUtility.ParseFrameworkName("net40"), new Version("3.0")),
                    new PackageReference("testId2", VersionUtility.ParseFrameworkName("net40"), new Version("4.0")),
                    new PackageReference("testId3", VersionUtility.ParseFrameworkName("net40"), new Version("5.0"))
                };

                var installer = new PackageInstaller(provider.Object);
                installer.InstallPackages(references, true, msg => callbacks.Add(msg));

                provider.Verify(i => i.InstallPackage(It.IsAny<IPackageReference>(), It.IsAny<bool>(), It.IsAny<Action<string>>()), Times.Exactly(3));
                callbacks.Count.ShouldEqual(1);
                callbacks.Count(x => x.EndsWith("unsuccessful.")).ShouldEqual(1);
            }

            [Fact]
            public void ShouldShowSuccessIfNoneOfPackagesFail()
            {
                var callbacks = new List<string>();
                var provider = new Mock<IInstallationProvider>();
                provider.Setup(
                    i => i.InstallPackage(It.IsAny<IPackageReference>(), It.IsAny<bool>(), It.IsAny<Action<string>>()))
                        .Returns(true);

                var references = new List<IPackageReference> {
                    new PackageReference("testId", VersionUtility.ParseFrameworkName("net40"), new Version("3.0")),
                    new PackageReference("testId2", VersionUtility.ParseFrameworkName("net40"), new Version("4.0")),
                    new PackageReference("testId3", VersionUtility.ParseFrameworkName("net40"), new Version("5.0"))
                };

                var installer = new PackageInstaller(provider.Object);
                installer.InstallPackages(references, true, msg => callbacks.Add(msg));

                callbacks.Count.ShouldEqual(1);
                callbacks.Count(x => x.EndsWith("successful.")).ShouldEqual(1);
            }

            [Fact]
            public void ShouldNotInstallExistingPackages()
            {
                var callbacks = new List<string>();
                var provider = new Mock<IInstallationProvider>();
                provider.Setup(
                    i => i.IsInstalled(It.Is<IPackageReference>(x => x.PackageId == "testId"), It.IsAny<bool>()))
                        .Returns(true);

                var references = new List<IPackageReference> {
                    new PackageReference("testId", VersionUtility.ParseFrameworkName("net40"), new Version("3.0")),
                    new PackageReference("testId2", VersionUtility.ParseFrameworkName("net40"), new Version("4.0")),
                    new PackageReference("testId3", VersionUtility.ParseFrameworkName("net40"), new Version("5.0"))
                };

                var installer = new PackageInstaller(provider.Object);
                installer.InstallPackages(references);

                provider.Verify(i => i.InstallPackage(It.Is<IPackageReference>(x => x.PackageId == "testId"), It.IsAny<bool>(), It.IsAny<Action<string>>()), Times.Never());
            }
        }
    }
}