using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Moq;
using Should;

namespace Scriptcs.Tests
{
    public class PackageAssemblyResolverTests
    {
        private Mock<IFileSystem> fileSystem = new Mock<IFileSystem>();
        private readonly string Package1Net40AssemblyName = @"package1\lib\net40\net40.dll";
        private readonly string Package2Net35AssemblyName = @"package2\lib\net35\net35.dll";
        private IEnumerable<string> assemblyNames;

        public PackageAssemblyResolverTests()
        {
            var results = new List<string>()
                {
                    Package1Net40AssemblyName,
                    Package2Net35AssemblyName,
                };

            fileSystem.Setup(f => f.EnumerateFiles(It.IsAny<string>(), It.IsAny<string>())).Returns(() => results);
            fileSystem.Setup(f => f.DirectoryExists(It.IsAny<string>())).Returns(true);
            var packageAssemblyResolver = new PackageAssemblyResolver(fileSystem.Object);
            assemblyNames = packageAssemblyResolver.GetAssemblyNames();
        }


        [Fact]
        public void ShouldReturnNet40PackageAssemblyNamesWhenGetAssemblyNamesIsCalled_AndNoneHigherAreAvailable()
        {
            assemblyNames.ShouldContain(Package1Net40AssemblyName);
        }

        [Fact]
        public void ShouldReturnNet35PackageAssemblyNamesWhenGetAssemblyNamesIsCalled_WhenNoHigherIsAvailable()
        {
            assemblyNames.ShouldContain(Package2Net35AssemblyName);
        }
    }
}
