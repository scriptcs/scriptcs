using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
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


namespace ScriptCs.Tests
{
    public class ScriptedScriptPackFinderTests
    {
        public class TheGetScriptedScriptPacksMethod
        {
            private readonly Mock<ILog> _logger;
            private readonly Mock<IFileSystem> _filesystem;
            private readonly Mock<IPackageObject> _package1;
            private readonly Mock<IPackageObject> _package2;
            private readonly Mock<IPackageContainer> _packageContainer;
            private readonly string _workingDirectory;

            public TheGetScriptedScriptPacksMethod()
            {
                _workingDirectory = "c:\\test";

                _filesystem = new Mock<IFileSystem>();
                _filesystem.SetupGet(i => i.CurrentDirectory).Returns("c:\\test");
                _filesystem.Setup(i => i.DirectoryExists(It.IsAny<string>())).Returns(true);
                _filesystem.Setup(i => i.FileExists(It.IsAny<string>())).Returns(true);
                
                _logger = new Mock<ILog>();
                
                _package1 = new Mock<IPackageObject>();
                _package1.SetupGet(i => i.Id).Returns("Pack1");
                _package1.SetupGet(i => i.Version).Returns(new Version("1.0"));
                _package1.SetupGet(i => i.TextVersion).Returns("1.0");
                _package1.SetupGet(i => i.FullName).Returns(_package1.Object.Id + "." + _package1.Object.Version);
                _package1.Setup(i => i.GetScriptBasedScriptPack()).Returns(@"Pack1\Pack1ScriptPack.csx");

                _package2 = new Mock<IPackageObject>();
                _package2.SetupGet(i => i.Id).Returns("Pack2");
                _package2.SetupGet(i => i.Version).Returns(new Version("1.0"));
                _package2.SetupGet(i => i.TextVersion).Returns("1.0");
                _package2.SetupGet(i => i.FullName).Returns(_package2.Object.Id + "." + _package2.Object.Version);
                _package2.Setup(i => i.GetScriptBasedScriptPack()).Returns(@"Pack2\Pack2ScriptPack.csx");
                _package2.Setup(i => i.FrameworkName).Returns(VersionUtility.ParseFrameworkName("net40"));
                _package1.Setup(i => i.Dependencies).Returns(new List<IPackageObject>() { _package2.Object });


                var ref1 = new PackageReference("Pack1", VersionUtility.ParseFrameworkName("net40"), new Version("1.0"));
                var ref2 = new PackageReference("Pack2", VersionUtility.ParseFrameworkName("net40"), new Version("1.0"));

                _packageContainer = new Mock<IPackageContainer>();
                _packageContainer.Setup(i => i.FindReferences(It.IsAny<string>())).Returns(new List<IPackageReference>{ref1});

                _packageContainer.Setup(i => i.FindPackage(It.IsAny<string>(), It.Is<PackageReference>(p=>p.PackageId == "Pack1"))).Returns(_package1.Object);
                _packageContainer.Setup(i => i.FindPackage(It.IsAny<string>(), It.Is<PackageReference>(p => p.PackageId == "Pack2"))).Returns(_package2.Object);
            }

            [Fact]
            public void ShouldReturnTopLevelPackageScript()
            {
                var generator = new ScriptedScriptPackFinder(_filesystem.Object, _packageContainer.Object, _logger.Object);
                var packs = generator.GetScriptedScriptPacks(_workingDirectory);
                packs.ShouldContain(@"c:\test\packages\Pack1\Pack1ScriptPack.csx");
            }

            [Fact]
            public void ShouldReturnDependentPackageScript()
            {
                var generator = new ScriptedScriptPackFinder(_filesystem.Object, _packageContainer.Object, _logger.Object);
                var packs = generator.GetScriptedScriptPacks(_workingDirectory);
                packs.ShouldContain(@"c:\test\packages\Pack2\Pack2ScriptPack.csx");
            }
        }
    }
}
