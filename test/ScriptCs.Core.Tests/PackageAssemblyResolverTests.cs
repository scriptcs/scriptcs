using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Moq;

namespace ScriptCs.Tests
{
    public class PackageAssemblyResolverTests
    {
        private Mock<IFileSystem> fileSystem = new Mock<IFileSystem>();

        /*
        public PackageAssemblyResolverTests()
        {
            var results = new List<string>()
                {
                    @"packages\package1\lib\net40\net40.dll",
                    @"c:\packages\package1\lib\net20\net20.dll"
                };

            fileSystem.Setup(f=>f.EnumerateFiles(It.IsAny<string>(), It.is))
        }
         */

        [Fact]
        public void ShouldReturnNet40PackageAssemblyNamesWhenGetAssemblyNamesIsCalled()
        {
            
        }

    }
}
