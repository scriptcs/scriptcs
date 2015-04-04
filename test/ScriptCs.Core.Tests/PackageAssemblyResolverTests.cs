﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using Moq;
using NuGet;
using ScriptCs.Contracts;
﻿using Should;
using Xunit;
using IFileSystem = ScriptCs.Contracts.IFileSystem;

namespace ScriptCs.Tests
{
    public class PackageAssemblyResolverTests
    {
        public class GetAssemblyNamesMethod
        {
            private readonly TestLogProvider _logProvider=new TestLogProvider();
            private readonly Mock<IFileSystem> _filesystem;
            private readonly Mock<IPackageObject> _package;
            private readonly Mock<IPackageContainer> _packageContainer;
            private readonly Mock<IAssemblyUtility> _assemblyUtility;
            private readonly List<IPackageReference> _packageIds;
            private readonly string _workingDirectory;

            public GetAssemblyNamesMethod()
            {
                _workingDirectory = Path.GetTempPath();

                _filesystem = new Mock<IFileSystem>();

                _filesystem.SetupGet(i => i.CurrentDirectory).Returns(_workingDirectory);
                _filesystem.SetupGet(i => i.PackagesFile).Returns("packages.config");
                _filesystem.SetupGet(i => i.PackagesFolder).Returns("packages");

                _filesystem.Setup(i => i.DirectoryExists(It.IsAny<string>())).Returns(true);
                _filesystem.Setup(i => i.FileExists(It.IsAny<string>())).Returns(true);

                _package = new Mock<IPackageObject>();
                _package.Setup(i => i.GetCompatibleDlls(It.IsAny<FrameworkName>()))
                        .Returns(new List<string> { "test.dll", "test2.dll" });
                _package.SetupGet(i => i.Id).Returns("id");
                _package.SetupGet(i => i.Version).Returns(new Version("3.0"));
                _package.SetupGet(i => i.TextVersion).Returns("3.0");
                _package.SetupGet(i => i.FullName).Returns(_package.Object.Id + "." + _package.Object.Version);
                _package.SetupGet(i => i.FrameworkAssemblies).Returns(new[] { "System.Net.Http", "System.ComponentModel" });

                _packageIds = new List<IPackageReference>
                    {
                        new PackageReference("testId", VersionUtility.ParseFrameworkName("net40"), new Version("3.0"))
                    };

                _packageContainer = new Mock<IPackageContainer>();
                _packageContainer.Setup(i => i.FindReferences(It.IsAny<string>())).Returns(_packageIds);
                _packageContainer.Setup(i => i.FindPackage(It.IsAny<string>(), It.IsAny<IPackageReference>())).Returns(_package.Object);

                _assemblyUtility = new Mock<IAssemblyUtility>();
                _assemblyUtility.Setup(u => u.IsManagedAssembly(It.IsAny<string>())).Returns(true);
            }

            [Fact]
            public void WhenManyPackagesAreMatchedAllMatchingDllsWithUniquePathsShouldBeReturned()
            {
                var resolver = new PackageAssemblyResolver(_filesystem.Object, _packageContainer.Object, _logProvider, _assemblyUtility.Object);
                _packageIds.Add(new PackageReference("testId2", VersionUtility.ParseFrameworkName("net40"), new Version("3.0")));

                var found = resolver.GetAssemblyNames(_workingDirectory).ToList();

                found.ShouldNotBeEmpty();
                found.Count.ShouldEqual(4);
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

                var resolver = new PackageAssemblyResolver(_filesystem.Object, _packageContainer.Object, _logProvider, _assemblyUtility.Object);
                var found = resolver.GetAssemblyNames(_workingDirectory).ToList();

                found.ShouldNotBeEmpty();
                found.Count.ShouldEqual(6);
            }

            [Fact]
            public void WhenPackageIsMatchedItsNonMatchingDllsShouldBeExcluded()
            {
                _package.Setup(
                    i =>
                    i.GetCompatibleDlls(
                        It.Is<FrameworkName>(x => x.FullName == VersionUtility.ParseFrameworkName("net40").FullName)))
                        .Returns(new List<string> { "test.dll" });

                var resolver = new PackageAssemblyResolver(_filesystem.Object, _packageContainer.Object, _logProvider, _assemblyUtility.Object);

                var found = resolver.GetAssemblyNames(_workingDirectory).ToList();

                found.ShouldNotBeEmpty();
                found.Count.ShouldEqual(3);
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

                var resolver = new PackageAssemblyResolver(_filesystem.Object, _packageContainer.Object, _logProvider, _assemblyUtility.Object);

                var found = resolver.GetAssemblyNames(_workingDirectory).ToList();

                found.ShouldNotBeEmpty();
                found.Count.ShouldEqual(4);
            }

            [Fact]
            public void WhenDllsAreMatchedDllFilePathsAreCorrectlyConcatenated()
            {
                var resolver = new PackageAssemblyResolver(_filesystem.Object, _packageContainer.Object, _logProvider, _assemblyUtility.Object);

                var found = resolver.GetAssemblyNames(_workingDirectory);

                found.First().ShouldEqual(
                    Path.Combine(_workingDirectory, "packages", "id.3.0", "test.dll"));
                found.ElementAt(1).ShouldEqual(
                    Path.Combine(_workingDirectory, "packages", "id.3.0", "test2.dll"));
            }

            [Fact]
            public void WhenNoPackagesAreFoundShouldLogWarning()
            {
                // arrange
                _packageContainer.Setup(i => i.FindPackage(It.IsAny<string>(), It.IsAny<IPackageReference>()))
                                 .Returns<List<IPackageObject>>(null);

                var resolver = new PackageAssemblyResolver(_filesystem.Object, _packageContainer.Object, _logProvider, _assemblyUtility.Object);

                // act
                resolver.GetAssemblyNames(_workingDirectory);

                // assert
                _logProvider.Output.ShouldContain("WARN: Cannot find: testId 3.0");
            }

            [Fact]
            public void WhenPackagesAreFoundButNoMatchingDllsExistShouldLogWarning()
            {
                // arrange
                _package.Setup(i => i.GetCompatibleDlls(It.IsAny<FrameworkName>())).Returns<List<string>>(null);

                var resolver = new PackageAssemblyResolver(_filesystem.Object, _packageContainer.Object, _logProvider, _assemblyUtility.Object);

                // act
                resolver.GetAssemblyNames(_workingDirectory);

                // assert
                _logProvider.Output.ShouldContain(
                    "WARN: Cannot find compatible binaries for .NETFramework,Version=v4.0 in: testId 3.0");
            }

            [Fact]
            public void WhenPackageDirectoryDoesNotExistShouldReturnEmptyPackagesList()
            {
                var resolver = new PackageAssemblyResolver(_filesystem.Object, new Mock<IPackageContainer>().Object, _logProvider, _assemblyUtility.Object);
                _filesystem.Setup(i => i.DirectoryExists(It.IsAny<string>())).Returns(false);

                var found = resolver.GetAssemblyNames(_workingDirectory);
                found.ShouldBeEmpty();
            }

            [Fact]
            public void WhenPackagesConfigDoesNotExistShouldReturnEmptyPackagesList()
            {
                var resolver = new PackageAssemblyResolver(_filesystem.Object, new Mock<IPackageContainer>().Object, _logProvider, _assemblyUtility.Object);
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

                var resolver = new PackageAssemblyResolver(_filesystem.Object, _packageContainer.Object, _logProvider, _assemblyUtility.Object);

                var found = resolver.GetAssemblyNames(_workingDirectory).ToList();

                _packageContainer.Verify(i => i.FindPackage(It.IsAny<string>(), It.IsAny<IPackageReference>()), Times.Exactly(2));
                found.ShouldNotBeEmpty();
                found.Count.ShouldEqual(6);
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

                var resolver = new PackageAssemblyResolver(_filesystem.Object, _packageContainer.Object, _logProvider, _assemblyUtility.Object);

                var found = resolver.GetAssemblyNames(_workingDirectory).ToList();

                _packageContainer.Verify(i => i.FindPackage(It.IsAny<string>(), It.IsAny<IPackageReference>()), Times.Exactly(2));
                found.ShouldNotBeEmpty();
                found.Count.ShouldEqual(6);
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

                var resolver = new PackageAssemblyResolver(_filesystem.Object, _packageContainer.Object, _logProvider, _assemblyUtility.Object);

                var found = resolver.GetAssemblyNames(_workingDirectory).ToList();

                _packageContainer.Verify(i => i.FindPackage(It.IsAny<string>(), It.IsAny<IPackageReference>()), Times.Exactly(2));
                found.ShouldNotBeEmpty();
                found.Count.ShouldEqual(4);
            }

            [Fact]
            public void ShouldIgnoreUnmanagedAssemblies()
            {
                var resolver = new PackageAssemblyResolver(_filesystem.Object, _packageContainer.Object, _logProvider, _assemblyUtility.Object);
                _packageIds.Add(new PackageReference("testId2", VersionUtility.ParseFrameworkName("net40"), new Version("3.0")));
                _assemblyUtility.Setup(u => u.IsManagedAssembly(It.Is<string>(s => Path.GetFileName(s) == "test.dll"))).Returns(false);

                var found = resolver.GetAssemblyNames(_workingDirectory).ToList();

                found.ShouldNotBeEmpty();
                found.Count.ShouldEqual(3);
            }
        }

        public class GetPackagesMethod
        {
            private Mock<IFileSystem> _fs;
            private Mock<IPackageContainer> _pc;
            private TestLogProvider _logProvider = new TestLogProvider();
            private Mock<IAssemblyUtility> _assemblyUtility;

            public GetPackagesMethod()
            {
                _fs = new Mock<IFileSystem>();
                _fs.SetupGet(f => f.PackagesFolder).Returns("packages");
                _fs.SetupGet(f => f.PackagesFile).Returns("packages.config");

                _pc = new Mock<IPackageContainer>();
                _assemblyUtility = new Mock<IAssemblyUtility>();
            }

            [Fact]
            public void ShouldReturnEmptyIfPackagesConfigDoesNotExist()
            {
                _fs.Setup(i => i.FileExists(It.IsAny<string>())).Returns(false);

                var resolver = new PackageAssemblyResolver(_fs.Object, _pc.Object, _logProvider, _assemblyUtility.Object);
                var result = resolver.GetPackages(@"c:/");

                result.ShouldBeEmpty();
            }

            [Fact]
            public void ShouldGetReferencesToPackages()
            {
                _fs.Setup(i => i.FileExists(It.IsAny<string>())).Returns(true);
                _pc.Setup(i => i.FindReferences(It.IsAny<string>())).Returns(new List<IPackageReference> { new PackageReference("id", VersionUtility.ParseFrameworkName("net40"), new Version("3.0")) });

                var resolver = new PackageAssemblyResolver(_fs.Object, _pc.Object, _logProvider, _assemblyUtility.Object);
                var result = resolver.GetPackages(@"c:/");

                _pc.Verify(i => i.FindReferences(It.IsAny<string>()), Times.Once());
                result.Count().ShouldEqual(1);
            }
        }

        public class SavePackagesMethod
        {
            private Mock<IFileSystem> _fs;
            private Mock<IPackageContainer> _pc;
            private readonly TestLogProvider _logProvider = new TestLogProvider();
            private Mock<IAssemblyUtility> _assemblyUtility;

            public SavePackagesMethod()
            {
                _fs = new Mock<IFileSystem>();
                _fs.SetupGet(f => f.PackagesFile).Returns("packages.config");
                _fs.SetupGet(f => f.PackagesFolder).Returns("packages");

                _pc = new Mock<IPackageContainer>();
                _assemblyUtility = new Mock<IAssemblyUtility>();
            }

            private IPackageAssemblyResolver GetResolver()
            {
                return new PackageAssemblyResolver(_fs.Object, _pc.Object, _logProvider, _assemblyUtility.Object);
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
                _logProvider.Output.ShouldContain("WARN: Packages directory does not exist!");
            }

            [Fact]
            public void ShouldSaveWhenThereIsNoPackagesConfigAndThereIsPackagesFolder()
            {
                _fs.Setup(i => i.CurrentDirectory).Returns("C:/");
                _fs.Setup(i => i.DirectoryExists(It.IsAny<string>())).Returns(true);
                var resolver = GetResolver();

                resolver.SavePackages();

                _pc.Verify(i => i.CreatePackageFile(), Times.Once());
            }
        }
    }
}
