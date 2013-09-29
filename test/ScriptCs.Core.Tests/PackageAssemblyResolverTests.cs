﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using Common.Logging;
using Moq;
using NuGet;
using ScriptCs.Contracts;
using ScriptCs.Exceptions;
using Should;
using Xunit;
using IFileSystem = ScriptCs.Contracts.IFileSystem;
using PackageReference = ScriptCs.Package.PackageReference;

namespace ScriptCs.Tests
{
    public class PackageAssemblyResolverTests
    {
        public class GetAssemblyNamesMethod
        {
            private readonly Mock<ILog> _logger;
            private readonly Mock<IFileSystem> _filesystem;
            private readonly Mock<IPackageObject> _package;
            private readonly Mock<IPackageContainer> _packageContainer;
            private readonly List<IPackageReference> _packageIds;
            private readonly string _workingDirectory;

            public GetAssemblyNamesMethod()
            {
                _workingDirectory = "c:\\test";

                _filesystem = new Mock<IFileSystem>();
                _filesystem.SetupGet(i => i.CurrentDirectory).Returns("c:\\test");
                _filesystem.Setup(i => i.DirectoryExists(It.IsAny<string>())).Returns(true);
                _filesystem.Setup(i => i.FileExists(It.IsAny<string>())).Returns(true);

                _logger = new Mock<ILog>();

                _package = new Mock<IPackageObject>();
                _package.Setup(i => i.GetCompatibleDlls(It.IsAny<FrameworkName>()))
                        .Returns(new List<string> { "test.dll", "test2.dll" });
                _package.SetupGet(i => i.Id).Returns("id");
                _package.SetupGet(i => i.Version).Returns(new Version("3.0"));
                _package.SetupGet(i => i.TextVersion).Returns("3.0");
                _package.SetupGet(i => i.FullName).Returns(_package.Object.Id + "." + _package.Object.Version);

                _packageIds = new List<IPackageReference>
                    {
                        new PackageReference("testId", VersionUtility.ParseFrameworkName("net40"), new Version("3.0"))
                    };

                _packageContainer = new Mock<IPackageContainer>();
                _packageContainer.Setup(i => i.FindReferences(It.IsAny<string>())).Returns(_packageIds);
                _packageContainer.Setup(i => i.FindPackage(It.IsAny<string>(), It.IsAny<IPackageReference>())).Returns(_package.Object);
            }

            [Fact]
            public void WhenManyPackagesAreMatchedAllMatchingDllsWithUniquePathsShouldBeReturned()
            {
                var resolver = new PackageAssemblyResolver(_filesystem.Object, _packageContainer.Object, _logger.Object);
                _packageIds.Add(new PackageReference("testId2", VersionUtility.ParseFrameworkName("net40"), new Version("3.0")));

                var found = resolver.GetAssemblyNames(_workingDirectory).ToList();

                found.ShouldNotBeEmpty();
                found.Count.ShouldEqual(2);
            }

            [Fact]
            public void WhenManyPackagesAreMatchedAllMatchingDllsShouldBeReturned()
            {
                _packageIds.Add(new PackageReference("testId2", VersionUtility.ParseFrameworkName("net40"), new Version("3.0")));
                var p = new Mock<IPackageObject>();
                p.Setup(i => i.GetCompatibleDlls(It.IsAny<FrameworkName>()))
                        .Returns(new List<string> { "test3.dll", "test4.dll" });
                p.SetupGet(i => i.Id).Returns("testId2");
                p.SetupGet(i => i.Version).Returns(new Version("3.0"));
                p.SetupGet(i => i.TextVersion).Returns("3.0");
                p.SetupGet(i => i.FullName).Returns(_package.Object.Id + "." + _package.Object.Version);

                _packageContainer.Setup(i => i.FindPackage(It.IsAny<string>(), It.Is<IPackageReference>(x => x.PackageId == "testId2"))).Returns(p.Object);

                var resolver = new PackageAssemblyResolver(_filesystem.Object, _packageContainer.Object, _logger.Object);
                var found = resolver.GetAssemblyNames(_workingDirectory).ToList();

                found.ShouldNotBeEmpty();
                found.Count.ShouldEqual(4);
            }

            [Fact]
            public void WhenPackageIsMatchedItsNonMatchingDllsShouldBeExcluded()
            {
                _package.Setup(
                    i =>
                    i.GetCompatibleDlls(
                        It.Is<FrameworkName>(x => x.FullName == VersionUtility.ParseFrameworkName("net40").FullName)))
                        .Returns(new List<string> { "test.dll" });

                var resolver = new PackageAssemblyResolver(_filesystem.Object, _packageContainer.Object, _logger.Object);

                var found = resolver.GetAssemblyNames(_workingDirectory).ToList();

                found.ShouldNotBeEmpty();
                found.Count.ShouldEqual(1);
            }

            [Fact]
            public void WhenManyPackagesAreMatchedAllNonMatchingDllsShouldBeExcluded()
            {
                _package.Setup(
                    i =>
                    i.GetCompatibleDlls(
                        It.Is<FrameworkName>(x => x.FullName == VersionUtility.ParseFrameworkName("net40").FullName)))
                        .Returns(new List<string> { "test.dll" });
                _packageIds.Add(new PackageReference("testId2", VersionUtility.ParseFrameworkName("net40"), new Version("3.0")));

                var p = new Mock<IPackageObject>();
                p.Setup(i => i.GetCompatibleDlls(It.IsAny<FrameworkName>()))
                        .Returns(new List<string> { "test3.dll" });
                p.SetupGet(i => i.Id).Returns("testId2");
                p.SetupGet(i => i.Version).Returns(new Version("3.0"));
                p.SetupGet(i => i.TextVersion).Returns("3.0");
                p.SetupGet(i => i.FullName).Returns(_package.Object.Id + "." + _package.Object.Version);

                _packageContainer.Setup(i => i.FindPackage(It.IsAny<string>(), It.Is<IPackageReference>(x => x.PackageId == "testId2"))).Returns(p.Object);

                var resolver = new PackageAssemblyResolver(_filesystem.Object, _packageContainer.Object, _logger.Object);

                var found = resolver.GetAssemblyNames(_workingDirectory).ToList();

                found.ShouldNotBeEmpty();
                found.Count.ShouldEqual(2);
            }

            [Fact]
            public void WhenDllsAreMatchedDllFilePathsAreCorrectlyConcatenated()
            {
                var resolver = new PackageAssemblyResolver(_filesystem.Object, _packageContainer.Object, _logger.Object);

                var found = resolver.GetAssemblyNames(_workingDirectory);

                found.First().ShouldEqual("c:\\test\\packages\\id.3.0\\test.dll");
                found.ElementAt(1).ShouldEqual("c:\\test\\packages\\id.3.0\\test2.dll");
            }

            [Fact]
            public void WhenNoPackagesAreFoundShouldThrowArgumentEx()
            {
                _packageContainer.Setup(i => i.FindPackage(It.IsAny<string>(), It.IsAny<IPackageReference>()))
                                 .Returns<List<IPackageObject>>(null);

                var resolver = new PackageAssemblyResolver(_filesystem.Object, _packageContainer.Object, _logger.Object);

                Assert.Throws<MissingAssemblyException>(() => resolver.GetAssemblyNames(_workingDirectory));
            }

            [Fact]
            public void WhenPackagesAreFoundButNoMatchingDllsExistShouldThrowArgumentEx()
            {
                _package.Setup(i => i.GetCompatibleDlls(It.IsAny<FrameworkName>())).Returns<List<string>>(null);

                var resolver = new PackageAssemblyResolver(_filesystem.Object, _packageContainer.Object, _logger.Object);

                Assert.Throws<MissingAssemblyException>(() => resolver.GetAssemblyNames(_workingDirectory));
            }

            [Fact]
            public void WhenPackageDirectoryDoesNotExistShouldReturnEmptyPackagesList()
            {
                var resolver = new PackageAssemblyResolver(_filesystem.Object, new Mock<IPackageContainer>().Object, _logger.Object);
                _filesystem.Setup(i => i.DirectoryExists(It.IsAny<string>())).Returns(false);

                var found = resolver.GetAssemblyNames(_workingDirectory);
                found.ShouldBeEmpty();
            }

            [Fact]
            public void WhenPackagesConfigDoesNotExistShouldReturnEmptyPackagesList()
            {
                var resolver = new PackageAssemblyResolver(_filesystem.Object, new Mock<IPackageContainer>().Object, _logger.Object);
                _filesystem.Setup(i => i.FileExists(It.IsAny<string>())).Returns(false);

                var found = resolver.GetAssemblyNames(_workingDirectory);
                found.ShouldBeEmpty();
            }

            [Fact]
            public void ShouldLoadAllDependenciesIfPackageHasAny()
            {
                var p = new Mock<IPackageObject>();
                p.Setup(i => i.GetCompatibleDlls(It.IsAny<FrameworkName>()))
                        .Returns(new List<string> { "test3.dll", "test4.dll" });
                p.SetupGet(i => i.Id).Returns("p2");
                p.SetupGet(i => i.Version).Returns(new Version("4.0"));
                p.SetupGet(i => i.TextVersion).Returns("4.0");
                p.SetupGet(i => i.FullName).Returns(_package.Object.Id + "." + _package.Object.Version);

                _package.Setup(i => i.Dependencies).Returns(new List<IPackageObject> { p.Object });
                _packageContainer.Setup(
                    i => i.FindPackage(It.IsAny<string>(), It.Is<IPackageReference>(x => x.PackageId == "p2")))
                                 .Returns(p.Object);

                var resolver = new PackageAssemblyResolver(_filesystem.Object, _packageContainer.Object, _logger.Object);

                var found = resolver.GetAssemblyNames(_workingDirectory).ToList();

                _packageContainer.Verify(i => i.FindPackage(It.IsAny<string>(), It.IsAny<IPackageReference>()), Times.Exactly(2));
                found.ShouldNotBeEmpty();
                found.Count.ShouldEqual(4);
            }

            [Fact]
            public void ShouldNotLoadDuplicateDependencies()
            {
                var p = new Mock<IPackageObject>();
                p.Setup(i => i.GetCompatibleDlls(It.IsAny<FrameworkName>()))
                        .Returns(new List<string> { "test3.dll", "test4.dll" });
                p.SetupGet(i => i.Id).Returns("p2");
                p.SetupGet(i => i.Version).Returns(new Version("4.0"));
                p.SetupGet(i => i.TextVersion).Returns("4.0");
                p.SetupGet(i => i.FullName).Returns(_package.Object.Id + "." + _package.Object.Version);

                _packageIds.Add(new PackageReference("p2", VersionUtility.ParseFrameworkName("net40"), new Version("3.0")));

                _package.Setup(i => i.Dependencies).Returns(new List<IPackageObject> { p.Object });
                _packageContainer.Setup(
                    i => i.FindPackage(It.IsAny<string>(), It.Is<IPackageReference>(x => x.PackageId == "p2")))
                                 .Returns(p.Object);

                var resolver = new PackageAssemblyResolver(_filesystem.Object, _packageContainer.Object, _logger.Object);

                var found = resolver.GetAssemblyNames(_workingDirectory).ToList();

                _packageContainer.Verify(i => i.FindPackage(It.IsAny<string>(), It.IsAny<IPackageReference>()), Times.Exactly(2));
                found.ShouldNotBeEmpty();
                found.Count.ShouldEqual(4);
            }

            [Fact]
            public void ShouldNotLoadIncompatibleDependenciesDlls()
            {
                var p = new Mock<IPackageObject>();
                p.Setup(i => i.GetCompatibleDlls(It.IsAny<FrameworkName>()))
                        .Returns(new List<string>());
                p.SetupGet(i => i.Id).Returns("p2");
                p.SetupGet(i => i.Version).Returns(new Version("4.0"));
                p.SetupGet(i => i.TextVersion).Returns("4.0");
                p.SetupGet(i => i.FullName).Returns(_package.Object.Id + "." + _package.Object.Version);

                _package.Setup(i => i.Dependencies).Returns(new List<IPackageObject> { p.Object });
                _packageContainer.Setup(
                    i => i.FindPackage(It.IsAny<string>(), It.Is<IPackageReference>(x => x.PackageId == "p2")))
                                 .Returns(p.Object);

                var resolver = new PackageAssemblyResolver(_filesystem.Object, _packageContainer.Object, _logger.Object);

                var found = resolver.GetAssemblyNames(_workingDirectory).ToList();

                _packageContainer.Verify(i => i.FindPackage(It.IsAny<string>(), It.IsAny<IPackageReference>()), Times.Exactly(2));
                found.ShouldNotBeEmpty();
                found.Count.ShouldEqual(2);
            }
        }

        public class GetPackagesMethod
        {
            private Mock<IFileSystem> _fs;
            private Mock<IPackageContainer> _pc;
            private Mock<ILog> _logger;

            public GetPackagesMethod()
            {
                _fs = new Mock<IFileSystem>();
                _pc = new Mock<IPackageContainer>();
                _logger = new Mock<ILog>();
            }

            [Fact]
            public void ShouldReturnEmptyIfPackagesConfigDoesNotExist()
            {
                _fs.Setup(i => i.FileExists(It.IsAny<string>())).Returns(false);

                var resolver = new PackageAssemblyResolver(_fs.Object, _pc.Object, _logger.Object);
                var result = resolver.GetPackages(@"c:/");

                result.ShouldBeEmpty();
            }

            [Fact]
            public void ShouldGetReferencesToPackages()
            {
                _fs.Setup(i => i.FileExists(It.IsAny<string>())).Returns(true);
                _pc.Setup(i => i.FindReferences(It.IsAny<string>())).Returns(new List<IPackageReference> { new PackageReference("id", VersionUtility.ParseFrameworkName("net40"), new Version("3.0")) });

                var resolver = new PackageAssemblyResolver(_fs.Object, _pc.Object, _logger.Object);
                var result = resolver.GetPackages(@"c:/");

                _pc.Verify(i => i.FindReferences(It.IsAny<string>()), Times.Once());
                result.Count().ShouldEqual(1);
            }
        }

        public class SavePackagesMethod
        {
            private Mock<IFileSystem> _fs;
            private Mock<IPackageContainer> _pc;
            private Mock<ILog> _logger;

            public SavePackagesMethod()
            {
                _fs = new Mock<IFileSystem>();
                _pc = new Mock<IPackageContainer>();
                _logger = new Mock<ILog>();
            }

            private IPackageAssemblyResolver GetResolver()
            {
                return new PackageAssemblyResolver(_fs.Object, _pc.Object, _logger.Object);
            }

            [Fact]
            public void ShouldNotSaveWhenThereIsPackagesFile()
            {
                _fs.Setup(i => i.CurrentDirectory).Returns("C:/");
                _fs.Setup(i => i.FileExists(It.IsAny<string>())).Returns(true);
                _fs.Setup(i => i.DirectoryExists(It.IsAny<string>())).Returns(true);
                var resolver = GetResolver();

                resolver.SavePackages();

                _pc.Verify(i => i.CreatePackageFile(), Times.Never());
                _logger.Verify(i => i.Info(It.Is<string>(x => x == "Packages.config already exists!")), Times.Once());
            }

            [Fact]
            public void ShouldNotSaveWhenThereIsNoPackagesFolder()
            {
                _fs.Setup(i => i.CurrentDirectory).Returns("C:/");
                _fs.Setup(i => i.FileExists(It.IsAny<string>())).Returns(false);
                _fs.Setup(i => i.DirectoryExists(It.IsAny<string>())).Returns(false);
                var resolver = GetResolver();

                resolver.SavePackages();

                _pc.Verify(i => i.CreatePackageFile(), Times.Never());
                _logger.Verify(i => i.Info(It.Is<string>(x => x == "Packages directory does not exist!")), Times.Once());
            }

            [Fact]
            public void ShouldSaveWhenThereIsNoPackagesConfigAndThereIsPackagesFolder()
            {
                _fs.Setup(i => i.CurrentDirectory).Returns("C:/");
                _fs.Setup(i => i.FileExists(It.IsAny<string>())).Returns(false);
                _fs.Setup(i => i.DirectoryExists(It.IsAny<string>())).Returns(true);
                _pc.Setup(i => i.CreatePackageFile()).Returns(new List<string> { "package" });
                var resolver = GetResolver();

                resolver.SavePackages();

                _pc.Verify(i => i.CreatePackageFile(), Times.Once());
                _logger.Verify(i => i.Info(It.Is<string>(x => x == "Packages.config successfully created!")), Times.Once());
                _logger.Verify(i => i.Info(It.Is<string>(x => x == "Added package")), Times.Once());
            }

            [Fact]
            public void ShouldDisplayErrorWhenNoPackagesFound()
            {
                _fs.Setup(i => i.CurrentDirectory).Returns("C:/");
                _fs.Setup(i => i.FileExists(It.IsAny<string>())).Returns(false);
                _fs.Setup(i => i.DirectoryExists(It.IsAny<string>())).Returns(true);
                _pc.Setup(i => i.CreatePackageFile()).Returns(new List<string>());
                var resolver = GetResolver();

                resolver.SavePackages();

                _pc.Verify(i => i.CreatePackageFile(), Times.Once());
                _logger.Verify(i => i.Info(It.Is<string>(x => x == "No packages found!")), Times.Once());
            }
        }
    }
}
