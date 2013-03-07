using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using ScriptCs.Exceptions;
using ScriptCs.Package;
using Should;
using NuGet;
using Xunit;
using Moq;
using PackageReference = ScriptCs.Package.PackageReference;

namespace ScriptCs.Core.Tests
{
    public class PackageAssemblyResolverTests
    {
        public class GetAssemblyNamesMethod
        {
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

                _package = new Mock<IPackageObject>();
                _package.Setup(i => i.GetCompatibleDlls(It.IsAny<FrameworkName>()))
                        .Returns(new List<string> {"test.dll", "test2.dll"});
                _package.SetupGet(i => i.Id).Returns("id");
                _package.SetupGet(i => i.Version).Returns("3.0");
                _package.SetupGet(i => i.FullName).Returns(_package.Object.Id + "." + _package.Object.Version);

                _packageIds = new List<IPackageReference>
                    {
                        new PackageReference("testId", VersionUtility.ParseFrameworkName("net40"))
                    };

                _packageContainer = new Mock<IPackageContainer>();
                _packageContainer.Setup(i => i.FindReferences(It.IsAny<string>())).Returns(_packageIds);
                _packageContainer.Setup(i => i.FindPackage(It.IsAny<string>(), It.IsAny<string>())).Returns(_package.Object);
            }

            [Fact]
            public void WhenPackageIsMatchedItsMatchingDllsShouldBeReturned()
            {
                var resolver = new PackageAssemblyResolver(_filesystem.Object, _packageContainer.Object);

                var found = resolver.GetAssemblyNames(_workingDirectory).ToList();

                found.ShouldNotBeEmpty();
                found.Count.ShouldEqual(2);
            }

            [Fact]
            public void WhenManyPackagesAreMatchedAllMatchingDllsShouldBeReturned()
            {
                var resolver = new PackageAssemblyResolver(_filesystem.Object, _packageContainer.Object);
                _packageIds.Add(new PackageReference("testId2", VersionUtility.ParseFrameworkName("net40")));

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
                        .Returns(new List<string> {"test.dll"});

                var resolver = new PackageAssemblyResolver(_filesystem.Object, _packageContainer.Object);

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
                        .Returns(new List<string> {"test.dll"});
                _packageIds.Add(new PackageReference("testId2", VersionUtility.ParseFrameworkName("net40")));

                var resolver = new PackageAssemblyResolver(_filesystem.Object, _packageContainer.Object);

                var found = resolver.GetAssemblyNames(_workingDirectory).ToList();

                found.ShouldNotBeEmpty();
                found.Count.ShouldEqual(2);
            }

            [Fact]
            public void WhenDllsAreMatchedDllFilePathsAreCorrectlyConcatenated()
            {
                var resolver = new PackageAssemblyResolver(_filesystem.Object, _packageContainer.Object);

                var found = resolver.GetAssemblyNames(_workingDirectory);

                found.First().ShouldEqual("c:\\test\\packages\\id.3.0\\test.dll");
                found.ElementAt(1).ShouldEqual("c:\\test\\packages\\id.3.0\\test2.dll");
            }

            [Fact]
            public void WhenNoPackagesAreFoundShouldThrowArgumentEx()
            {
                _packageContainer.Setup(i => i.FindPackage(It.IsAny<string>(), It.IsAny<string>()))
                                 .Returns<List<IPackageObject>>(null);

                var resolver = new PackageAssemblyResolver(_filesystem.Object, _packageContainer.Object);

                Assert.Throws<MissingAssemblyException>(() => resolver.GetAssemblyNames(_workingDirectory));
            }

            [Fact]
            public void WhenPackagesAreFoundButNoMatchingDllsExistShouldThrowArgumentEx()
            {
                _package.Setup(i => i.GetCompatibleDlls(It.IsAny<FrameworkName>())).Returns<List<string>>(null);

                var resolver = new PackageAssemblyResolver(_filesystem.Object, _packageContainer.Object);

                Assert.Throws<MissingAssemblyException>(() => resolver.GetAssemblyNames(_workingDirectory));
            }

            [Fact]
            public void WhenPackageDirectoryDoesNotExistShouldReturnEmptyPackagesList()
            {
                var resolver = new PackageAssemblyResolver(_filesystem.Object, new Mock<IPackageContainer>().Object);
                _filesystem.Setup(i => i.DirectoryExists(It.IsAny<string>())).Returns(false);

                var found = resolver.GetAssemblyNames(_workingDirectory);
                found.ShouldBeEmpty();
            }

            [Fact]
            public void WhenPackagesConfigDoesNotExistShouldReturnEmptyPackagesList()
            {
                var resolver = new PackageAssemblyResolver(_filesystem.Object, new Mock<IPackageContainer>().Object);
                _filesystem.Setup(i => i.FileExists(It.IsAny<string>())).Returns(false);

                var found = resolver.GetAssemblyNames(_workingDirectory);
                found.ShouldBeEmpty();
            }
        }
    }
}
