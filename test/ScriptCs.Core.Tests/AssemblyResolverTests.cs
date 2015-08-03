using System.IO;
using System.Linq;
using Moq;
using Ploeh.AutoFixture.Xunit;
using ScriptCs.Contracts;
using Should;
using Xunit.Extensions;

namespace ScriptCs.Tests
{
    public class AssemblyResolverTests
    {
        public class GetAssemblyPathsMethod
        {
            [Theory, ScriptCsAutoData]
            public void ShouldReturnAssembliesFromPackagesFolder(
                [Frozen] Mock<IFileSystem> fileSystemMock,
                [Frozen] Mock<IPackageAssemblyResolver> packageAssemblyResolverMock,
                [Frozen] Mock<IAssemblyUtility> assemblyUtilityMock,
                AssemblyResolver resolver
            )
            {
                const string WorkingDirectory = @"C:\";

                var packagesFolder = Path.Combine(WorkingDirectory, "packages");
                var assemblyFile = Path.Combine(packagesFolder, "MyAssembly.dll");

                assemblyUtilityMock.Setup(a => a.IsManagedAssembly(It.IsAny<string>())).Returns(true);
                fileSystemMock.Setup(x => x.DirectoryExists(packagesFolder)).Returns(true);
                fileSystemMock.SetupGet(x => x.PackagesFolder).Returns("packages");
                fileSystemMock.SetupGet(x => x.BinFolder).Returns("bin");

                packageAssemblyResolverMock.Setup(x => x.GetAssemblyNames(WorkingDirectory)).Returns(new[] { assemblyFile });

                var assemblies = resolver.GetAssemblyPaths(WorkingDirectory).ToList();

                assemblies.Count.ShouldEqual(1);
                assemblies[0].ShouldEqual(assemblyFile);
            }

            [Theory, ScriptCsAutoData]
            public void ShouldReturnAssembliesFromBinFolder(
                [Frozen] Mock<IFileSystem> fileSystemMock,
                [Frozen] Mock<IPackageAssemblyResolver> packageAssemblyResolverMock,
                [Frozen] Mock<IAssemblyUtility> assemblyUtilityMock,
                AssemblyResolver resolver
            )
            {
                const string WorkingDirectory = @"C:\";

                var binFolder = Path.Combine(WorkingDirectory, "bin");
                var assemblyFile = Path.Combine(binFolder, "MyAssembly.dll");

                fileSystemMock.Setup(x => x.DirectoryExists(binFolder)).Returns(true);
                fileSystemMock.SetupGet(x => x.PackagesFolder).Returns("packages");
                fileSystemMock.SetupGet(x => x.BinFolder).Returns("bin");
                fileSystemMock.Setup(x => x.EnumerateFiles(binFolder, It.IsAny<string>(), SearchOption.TopDirectoryOnly)).Returns(new[] { assemblyFile });

                assemblyUtilityMock.Setup(x => x.IsManagedAssembly(assemblyFile)).Returns(true);

                var assemblies = resolver.GetAssemblyPaths(WorkingDirectory).ToList();

                assemblies.Count.ShouldEqual(1);
                assemblies[0].ShouldEqual(assemblyFile);
            }

            [Theory, ScriptCsAutoData]
            public void ShouldNotReturnNonManagedAssemblies(
                [Frozen] Mock<IFileSystem> fileSystemMock,
                [Frozen] Mock<IPackageAssemblyResolver> packageAssemblyResolverMock,
                [Frozen] Mock<IAssemblyUtility> assemblyUtilityMock,
                AssemblyResolver resolver
            )
            {
                const string WorkingDirectory = @"C:\";

                var binFolder = Path.Combine(WorkingDirectory, "bin");
                var managed = Path.Combine(binFolder, "MyAssembly.dll");
                var nonManaged = Path.Combine(binFolder, "MyNonManagedAssembly.dll");

                fileSystemMock.Setup(x => x.DirectoryExists(binFolder)).Returns(true);
                fileSystemMock.SetupGet(x => x.PackagesFolder).Returns("packages");
                fileSystemMock.SetupGet(x => x.BinFolder).Returns("bin");
                fileSystemMock.Setup(x => x.EnumerateFiles(binFolder, It.IsAny<string>(), SearchOption.TopDirectoryOnly
                    ))
                    .Returns(new[] { managed, nonManaged });

                assemblyUtilityMock.Setup(x => x.IsManagedAssembly(managed)).Returns(true);
                assemblyUtilityMock.Setup(x => x.IsManagedAssembly(nonManaged)).Returns(false);

                var assemblies = resolver.GetAssemblyPaths(WorkingDirectory).ToList();

                assemblies.Count.ShouldEqual(1);
                assemblies[0].ShouldEqual(managed);
            }

            [Theory, ScriptCsAutoData]
            public void ShouldOnlyReturnBinariesWhenFlagIsSet(
                [Frozen] Mock<IPackageAssemblyResolver> packageAssemblyResolverMock, 
                [Frozen] Mock<IFileSystem> fileSystemMock, 
                [Frozen] Mock<IAssemblyUtility> assemblyUtilityMock,
                AssemblyResolver resolver)
            {
                const string WorkingDirectory = @"C:\";

                var binFolder = Path.Combine(WorkingDirectory, "bin");

                assemblyUtilityMock.Setup(a => a.IsManagedAssembly(It.IsAny<string>())).Returns(true);
                fileSystemMock.Setup(x => x.DirectoryExists(binFolder)).Returns(true);
                fileSystemMock.Setup(x => x.DirectoryExists(@"C:\packages")).Returns(true);
                fileSystemMock.SetupGet(x => x.PackagesFolder).Returns("packages");
                fileSystemMock.SetupGet(x => x.BinFolder).Returns("bin");
                fileSystemMock.Setup(x => x.EnumerateFiles(binFolder, It.IsAny<string>(), SearchOption.AllDirectories))
                    .Returns(Enumerable.Empty<string>());

                packageAssemblyResolverMock.Setup(p=>p.GetAssemblyNames(WorkingDirectory)).Returns(new string[] {"test.dll", "test.exe", "test.foo"});

                var assemblies = resolver.GetAssemblyPaths(WorkingDirectory, true).ToList();
                assemblies.ShouldNotContain("test.foo");
                assemblies.ShouldContain("test.dll");
                assemblies.ShouldContain("test.exe");
            }
        }
    }
}