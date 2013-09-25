using System;
using System.IO;
using System.Linq;

using Common.Logging;

using Moq;

using ScriptCs.Contracts;

using Should;

using Xunit;

namespace ScriptCs.Tests
{
    public class AssemblyResolverTests
    {
        public class GetAssemblyPathsMethod
        {
            [Fact]
            public void ShouldReturnAssembliesFromPackagesFolder()
            {
                const string WorkingDirectory = @"C:\";

                var packagesFolder = Path.Combine(WorkingDirectory, Constants.PackagesFolder);
                var assemblyFile = Path.Combine(packagesFolder, "MyAssembly.dll");

                var fileSystem = new Mock<IFileSystem>();
                fileSystem.Setup(x => x.DirectoryExists(packagesFolder)).Returns(true);

                var packageAssemblyResolver = new Mock<IPackageAssemblyResolver>();
                packageAssemblyResolver.Setup(x => x.GetAssemblyNames(WorkingDirectory)).Returns(new[] { assemblyFile });

                var resolver = new AssemblyResolver(fileSystem.Object, packageAssemblyResolver.Object, Mock.Of<IAssemblyUtility>(), Mock.Of<ILog>());

                var assemblies = resolver.GetAssemblyPaths(WorkingDirectory).ToList();

                assemblies.Count.ShouldEqual(1);
                assemblies[0].ShouldEqual(assemblyFile);
            }

            [Fact]
            public void ShouldReturnAssembliesFromBinFolder()
            {
                const string WorkingDirectory = @"C:\";

                var binFolder = Path.Combine(WorkingDirectory, "bin");
                var assemblyFile = Path.Combine(binFolder, "MyAssembly.dll");

                var fileSystem = new Mock<IFileSystem>();
                fileSystem.Setup(x => x.DirectoryExists(binFolder)).Returns(true);
                fileSystem.Setup(x => x.EnumerateFiles(binFolder, It.IsAny<string>(), SearchOption.AllDirectories)).Returns(new[] { assemblyFile });

                var assemblyUtility = new Mock<IAssemblyUtility>();
                assemblyUtility.Setup(x => x.IsManagedAssembly(assemblyFile)).Returns(true);

                var resolver = new AssemblyResolver(fileSystem.Object, Mock.Of<IPackageAssemblyResolver>(), assemblyUtility.Object, Mock.Of<ILog>());

                var assemblies = resolver.GetAssemblyPaths(WorkingDirectory).ToList();

                assemblies.Count.ShouldEqual(1);
                assemblies[0].ShouldEqual(assemblyFile);
            }

            [Fact]
            public void ShouldNotReturnNonManagedAssemblies()
            {
                const string WorkingDirectory = @"C:\";

                var binFolder = Path.Combine(WorkingDirectory, "bin");
                var managed = Path.Combine(binFolder, "MyAssembly.dll");
                var nonManaged = Path.Combine(binFolder, "MyNonManagedAssembly.dll");

                var fileSystem = new Mock<IFileSystem>();
                fileSystem.Setup(x => x.DirectoryExists(binFolder)).Returns(true);
                fileSystem.Setup(x => x.EnumerateFiles(binFolder, It.IsAny<string>(), SearchOption.AllDirectories))
                    .Returns(new[] { managed, nonManaged });

                var assemblyUtility = new Mock<IAssemblyUtility>();
                assemblyUtility.Setup(x => x.IsManagedAssembly(managed)).Returns(true);
                assemblyUtility.Setup(x => x.IsManagedAssembly(nonManaged)).Returns(false);

                var resolver = new AssemblyResolver(fileSystem.Object, Mock.Of<IPackageAssemblyResolver>(), assemblyUtility.Object, Mock.Of<ILog>());

                var assemblies = resolver.GetAssemblyPaths(WorkingDirectory).ToList();

                assemblies.Count.ShouldEqual(1);
                assemblies[0].ShouldEqual(managed);
            }

            [Fact]
            public void ShouldNotReturnFilesThatMatchscriptcs_build_dllPattern()
            {
                const string WorkingDirectory = @"C:\";

                var binFolder = Path.Combine(WorkingDirectory, "bin");
                var scriptDll = Path.Combine(binFolder, string.Format("myscript{0}", Constants.CompiledDllSuffix));

                var fileSystem = new Mock<IFileSystem>();
                fileSystem.Setup(x => x.DirectoryExists(binFolder)).Returns(true);
                fileSystem.Setup(x => x.EnumerateFiles(binFolder, It.IsAny<string>(), SearchOption.AllDirectories))
                    .Returns(new[] { scriptDll });

                var assemblyUtility = new Mock<IAssemblyUtility>();
                assemblyUtility.Setup(x => x.IsManagedAssembly(scriptDll)).Returns(true);

                var resolver = new AssemblyResolver(fileSystem.Object, Mock.Of<IPackageAssemblyResolver>(), assemblyUtility.Object, Mock.Of<ILog>());

                var assemblies = resolver.GetAssemblyPaths(WorkingDirectory).ToList();

                assemblies.Count.ShouldEqual(0);
            }

            [Fact]
            public void ShouldReturnsAllOtherDllsIfScriptDllIsNull()
            {
                const string WorkingDirectory = @"C:\";

                var binFolder = Path.Combine(WorkingDirectory, "bin");

                var fileSystem = new Mock<IFileSystem>();
                fileSystem.Setup(x => x.DirectoryExists(binFolder)).Returns(true);
                fileSystem.Setup(x => x.EnumerateFiles(binFolder, It.IsAny<string>(), SearchOption.AllDirectories))
                    .Returns(new[] { "assembly.dll" });

                var assemblyUtility = new Mock<IAssemblyUtility>();
                assemblyUtility.Setup(x => x.IsManagedAssembly("assembly.dll")).Returns(true);

                var resolver = new AssemblyResolver(fileSystem.Object, Mock.Of<IPackageAssemblyResolver>(), assemblyUtility.Object, Mock.Of<ILog>());

                var assemblies = resolver.GetAssemblyPaths(WorkingDirectory).ToList();

                assemblies.Count.ShouldEqual(1);
            }
        }
    }
}