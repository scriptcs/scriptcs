using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Moq;
using Should;

namespace Scriptcs.Tests
{
    public class PackageAssemblyResolverFacts
    {
        public class TheGetAssemblyNamesMethod
        { 
            private Mock<IFileSystem> _fileSystem = new Mock<IFileSystem>();
            private readonly string _package1Net40AssemblyName = @"C:\someapp\packages\package1\lib\net40\net40.dll";
            private readonly string _package2Net35AssemblyName = @"C:\someapp\packages\package2\lib\net35\net35.dll";
            private readonly string _package1Net35AssemblyName = @"C:\someapp\packages\package1\lib\net35\net35.dll";
            private readonly string _package3Net20AssemblyName = @"C:\someapp\packages\package3\lib\net20\net20.dll";
            private IEnumerable<string> _assemblyNames;

            public TheGetAssemblyNamesMethod()
            {
                var results = new List<string>()
                    {
                        _package1Net40AssemblyName,
                        _package2Net35AssemblyName,
                        _package1Net35AssemblyName,
                        _package3Net20AssemblyName
                    };

                _fileSystem.Setup(f => f.EnumerateFiles(It.IsAny<string>(), It.IsAny<string>())).Returns(() => results);
                _fileSystem.Setup(f => f.DirectoryExists(It.IsAny<string>())).Returns(true);
                _fileSystem.Setup(f => f.CurrentDirectory).Returns(@"C:\someapp");
                var packageAssemblyResolver = new PackageAssemblyResolver(_fileSystem.Object);
                _assemblyNames = packageAssemblyResolver.GetAssemblyNames();
            }


            [Fact]
            public void ShouldReturnNet40PackageAssemblyNames_WhenNoHigherAreAvailable()
            {
                _assemblyNames.ShouldContain(_package1Net40AssemblyName);
            }

            [Fact]
            public void ShouldReturnNet35PackageAssemblyNames_WhenNoHigherAreAvailable()
            {
                _assemblyNames.ShouldContain(_package2Net35AssemblyName);
            }
        
            [Fact]
            public void ShouldReturnNet20PackageAssemblyNames_WhenNoHigherAreAvailable()
            {
                _assemblyNames.ShouldContain(_package3Net20AssemblyName);
            }

            [Fact]
            public void ShouldNotReturnNet35PackageAssemblyNames_WhenNet40AssemblyNameIsAvailable()
            {
                _assemblyNames.ShouldNotContain(_package1Net35AssemblyName);
            }
        }
    }
}
