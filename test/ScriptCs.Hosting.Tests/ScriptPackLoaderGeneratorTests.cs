using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using Moq;
using NuGet;
using ScriptCs.Contracts;
using Should;
using Xunit;
using IFileSystem = ScriptCs.Contracts.IFileSystem;
using PackageReference = ScriptCs.Package.PackageReference;


namespace ScriptCs.Hosting.Tests
{
    public class ScriptPackLoaderGeneratorTests
    {
        public class TheGetLoaderScriptMethod
        {
            private readonly Mock<ILog> _logger;
            private readonly Mock<IFileSystem> _filesystem;
            private readonly Mock<IPackageObject> _package;
            private readonly Mock<IPackageContainer> _packageContainer;
            private readonly List<IPackageReference> _packageIds;
            private readonly string _workingDirectory;

            public TheGetLoaderScriptMethod()
            {
                _workingDirectory = "c:\\test";

                _filesystem = new Mock<IFileSystem>();
                _filesystem.SetupGet(i => i.CurrentDirectory).Returns("c:\\test");
                _filesystem.Setup(i => i.DirectoryExists(It.IsAny<string>())).Returns(true);
                _filesystem.Setup(i => i.FileExists(It.IsAny<string>())).Returns(true);
                
                _logger = new Mock<ILog>();
                
                _package = new Mock<IPackageObject>();
                _package.SetupGet(i => i.Id).Returns("Pack1");
                _package.SetupGet(i => i.Version).Returns(new Version("1.0"));
                _package.SetupGet(i => i.TextVersion).Returns("1.0");
                _package.SetupGet(i => i.FullName).Returns(_package.Object.Id + "." + _package.Object.Version);
                _package.Setup(i => i.GetScriptBasedScriptPack()).Returns(@"c:\test\Pack1\Pack1ScriptPack.csx");
                _packageIds = new List<IPackageReference>
                    {
                        new PackageReference("Pack1", VersionUtility.ParseFrameworkName("net40"), new Version("1.0")),
                        new PackageReference("Pack2", VersionUtility.ParseFrameworkName("net40"), new Version("1.0"))
                    };
                _packageContainer = new Mock<IPackageContainer>();
                _packageContainer.Setup(i => i.FindReferences(It.IsAny<string>())).Returns(_packageIds);
                _packageContainer.Setup(i => i.FindPackage(It.IsAny<string>(), It.IsAny<IPackageReference>())).Returns(_package.Object);

            }

            [Fact]
            public void ShouldReturnTopLevelPackageScript()
            {
                var generator = new ScriptPackLoaderGenerator(_filesystem.Object, _packageContainer.Object, _logger.Object);
                var loader = generator.GetLoaderScript(_workingDirectory);
                
            }

            [Fact]
            public void ShouldReturnDependentPackageScript()
            {
                _packageContainer.Setup(i => i.FindReferences(It.IsAny<string>())).Returns(_packageIds);
            
            }
        }
    }
}
