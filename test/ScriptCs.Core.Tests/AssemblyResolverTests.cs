using System.IO;
using System.Linq;

using Common.Logging;

using Moq;

using Should;

using Xunit;

namespace ScriptCs.Tests
{
    public class AssemblyResolverTests
    {
        public class GetAssemblyPathsMethod
        {
            [Fact]
            public void ShouldReturnAssembliesFromManifestFile()
            {
                const string WorkingDirectory = @"C:\";

                var assemblyFile = Path.Combine(WorkingDirectory, @"C:\MyAssembly.dll");
                var manifestFile = Path.Combine(WorkingDirectory, Constants.ManifestFile);

                var fileSystem = new Mock<IFileSystem>();
                fileSystem.Setup(x => x.FileExists(manifestFile)).Returns(true);
                fileSystem.Setup(x => x.ReadFile(manifestFile)).Returns(@"{ ""PackageAssemblies"": [ " + assemblyFile + " ] }");

                var resolver = new AssemblyResolver(fileSystem.Object, Mock.Of<IAssemblyUtility>(), Mock.Of<ILog>());

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

                var resolver = new AssemblyResolver(fileSystem.Object, assemblyUtility.Object, Mock.Of<ILog>());

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
                var nonManaged = Path.Combine(binFolder, "MyAssembly.dll");

                var fileSystem = new Mock<IFileSystem>();
                fileSystem.Setup(x => x.DirectoryExists(binFolder)).Returns(true);
                fileSystem.Setup(x => x.EnumerateFiles(binFolder, It.IsAny<string>(), SearchOption.AllDirectories))
                    .Returns(new[] { managed, nonManaged });

                var assemblyUtility = new Mock<IAssemblyUtility>();
                assemblyUtility.Setup(x => x.IsManagedAssembly(managed)).Returns(true);

                var resolver = new AssemblyResolver(fileSystem.Object, assemblyUtility.Object, Mock.Of<ILog>());

                var assemblies = resolver.GetAssemblyPaths(WorkingDirectory).ToList();

                assemblies.Count.ShouldEqual(1);
                assemblies[0].ShouldEqual(managed);
            }
        }
    }
}