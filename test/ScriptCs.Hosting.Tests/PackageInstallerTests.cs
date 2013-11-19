﻿using System;
using System.Collections.Generic;
using System.Linq;
using Common.Logging;
using Moq;
using NuGet;
using ScriptCs.Contracts;
using ScriptCs.Hosting.Package;
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
                var installer = new PackageInstaller(new Mock<IInstallationProvider>().Object, new Mock<ILog>().Object);
                Assert.Throws<ArgumentNullException>(() => installer.InstallPackages(null));
            }

            [Fact]
            public void ShouldInstallAllPassedPackages()
            {
                var logger = new Mock<ILog>();
                var provider = new Mock<IInstallationProvider>();
                provider.Setup(
                    i => i.InstallPackage(It.IsAny<IPackageReference>(), It.IsAny<bool>()))
                        .Returns(true);

                var references = new List<IPackageReference> 
                {
                    new PackageReference("testId", VersionUtility.ParseFrameworkName("net40"), new Version("3.0")),
                    new PackageReference("testId2", VersionUtility.ParseFrameworkName("net40"), new Version("4.0")),
                    new PackageReference("testId3", VersionUtility.ParseFrameworkName("net40"), new Version("5.0"))
                };

                var installer = new PackageInstaller(provider.Object, logger.Object);
                installer.InstallPackages(references);

                provider.Verify(i => i.InstallPackage(It.IsAny<IPackageReference>(), It.IsAny<bool>()), Times.Exactly(3));
            }

            [Fact]
            public void ShouldShowErrorIfOneOfPackagesFail()
            {
                var logger = new Mock<ILog>();
                var provider = new Mock<IInstallationProvider>();
                provider.Setup(
                    i => i.InstallPackage(It.IsAny<IPackageReference>(), It.IsAny<bool>()))
                        .Returns(true);
                provider.Setup(
                    i => i.InstallPackage(It.Is<IPackageReference>(x => x.PackageId == "testId"), It.IsAny<bool>()))
                        .Returns(false);

                var references = new List<IPackageReference> 
                {
                    new PackageReference("testId", VersionUtility.ParseFrameworkName("net40"), new Version("3.0")),
                    new PackageReference("testId2", VersionUtility.ParseFrameworkName("net40"), new Version("4.0")),
                    new PackageReference("testId3", VersionUtility.ParseFrameworkName("net40"), new Version("5.0"))
                };

                var installer = new PackageInstaller(provider.Object, logger.Object);
                installer.InstallPackages(references, true);

                provider.Verify(i => i.InstallPackage(It.IsAny<IPackageReference>(), It.IsAny<bool>()), Times.Exactly(3));
                logger.Verify(i => i.Info(It.Is<string>(x => x.EndsWith("unsuccessful."))), Times.Once());
            }

            [Fact]
            public void ShouldShowSuccessIfNoneOfPackagesFail()
            {
                var logger = new Mock<ILog>();
                var provider = new Mock<IInstallationProvider>();
                provider.Setup(
                    i => i.InstallPackage(It.IsAny<IPackageReference>(), It.IsAny<bool>()))
                        .Returns(true);

                var references = new List<IPackageReference> 
                {
                    new PackageReference("testId", VersionUtility.ParseFrameworkName("net40"), new Version("3.0")),
                    new PackageReference("testId2", VersionUtility.ParseFrameworkName("net40"), new Version("4.0")),
                    new PackageReference("testId3", VersionUtility.ParseFrameworkName("net40"), new Version("5.0"))
                };

                var installer = new PackageInstaller(provider.Object, logger.Object);
                installer.InstallPackages(references, true);

                logger.Verify(i => i.Info(It.Is<string>(x => x.EndsWith("successful."))), Times.Once());
            }

            [Fact]
            public void ShouldNotInstallExistingPackages()
            {
                var logger = new Mock<ILog>();
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

                var installer = new PackageInstaller(provider.Object, logger.Object);
                installer.InstallPackages(references);

                provider.Verify(i => i.InstallPackage(It.Is<IPackageReference>(x => x.PackageId == "testId"), It.IsAny<bool>()), Times.Never());
            }
        }
    }
}