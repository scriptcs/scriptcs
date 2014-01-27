using Common.Logging;
using Moq;
using ScriptCs.Contracts;
using ScriptCs.Engine.Roslyn;
using System.IO;
using Should;
using Xunit.Extensions;

namespace ScriptCs.Tests
{
    public class DiskAssemblyLoaderTests
    {
        public class TheShouldCompileMethod
        {
            [Theory, ScriptCsAutoData]
            public void ShouldReturnTrueIfDllDoesNotExistInDisk()
            {
                // arrange
                var hostFactory = new Mock<IScriptHostFactory>();
                var logger = new Mock<ILog>();
                var fileSystem = new Mock<IFileSystem>(MockBehavior.Strict);

                var loader = new DiskAssemblyLoader(hostFactory.Object, logger.Object, fileSystem.Object);

                const string FileName = "script.csx";
                const string CacheDirectory = "c:";
                string cachedDllFullPath = Path.Combine(CacheDirectory, "script.dll");

                loader.SetContext(FileName, CacheDirectory);

                fileSystem.Setup(fs => fs.FileExists(cachedDllFullPath)).Returns(false);

                // act + assert
                loader.ShouldCompile().ShouldBeTrue();
            }

            [Theory, ScriptCsAutoData]
            public void ShouldReturnFalseIfDllExistsInDisk()
            {
                // arrange
                var hostFactory = new Mock<IScriptHostFactory>();
                var logger = new Mock<ILog>();
                var fileSystem = new Mock<IFileSystem>(MockBehavior.Strict);

                var loader = new DiskAssemblyLoader(hostFactory.Object, logger.Object, fileSystem.Object);

                const string FileName = "script.csx";
                const string CacheDirectory = "c:";
                string cachedDllFullPath = Path.Combine(CacheDirectory, "script.dll");

                loader.SetContext(FileName, CacheDirectory);

                fileSystem.Setup(fs => fs.FileExists(cachedDllFullPath)).Returns(true);

                // act + assert
                loader.ShouldCompile().ShouldBeFalse();
            }
        }
    }
}
