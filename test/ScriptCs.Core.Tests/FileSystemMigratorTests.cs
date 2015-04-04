using Moq;
using Ploeh.AutoFixture.Xunit;
using ScriptCs.Contracts;
using Xunit.Extensions;

namespace ScriptCs.Tests
{
    public class FileSystemMigratorTests
    {
        public class TheMigrateMethod
        {
            [Theory]
            [ScriptCsAutoData("scriptcs_bin")]
            [ScriptCsAutoData(".scriptcs_cache")]
            [ScriptCsAutoData("scriptcs_packages")]
            public void DoesNotMigrateWhenACurrentDirectoryIsFound(
                string fileName, [Frozen] Mock<IFileSystem> fileSystem)
            {
                // arrange
                SetupUnmigrated(fileSystem);
                fileSystem.Setup(f => f.DirectoryExists(fileName)).Returns(true);

                var sut = new FileSystemMigrator(fileSystem.Object, new TestLogProvider());

                // act
                sut.Migrate();

                // assert
                VerifyNoMigration(fileSystem);
            }

            [Theory]
            [ScriptCsAutoData("scriptcs_packages.config")]
            [ScriptCsAutoData("scriptcs_nuget.config")]
            public void DoesNotMigrateWhenACurrentFileIsFound(
                string fileName, [Frozen] Mock<IFileSystem> fileSystem)
            {
                // arrange
                SetupUnmigrated(fileSystem);
                fileSystem.Setup(f => f.FileExists(fileName)).Returns(true);

                var sut = new FileSystemMigrator(fileSystem.Object, new TestLogProvider());

                // act
                sut.Migrate();

                // assert
                VerifyNoMigration(fileSystem);
            }

            private static void SetupUnmigrated(Mock<IFileSystem> fileSystem)
            {
                fileSystem.Setup(f => f.FileExists(It.IsAny<string>())).Returns(true);
                fileSystem.Setup(f => f.DirectoryExists(fileSystem.Object.BinFolder)).Returns(false);
                fileSystem.Setup(f => f.DirectoryExists(fileSystem.Object.DllCacheFolder)).Returns(false);
                fileSystem.Setup(f => f.FileExists(fileSystem.Object.NugetFile)).Returns(false);
                fileSystem.Setup(f => f.FileExists(fileSystem.Object.PackagesFile)).Returns(false);
                fileSystem.Setup(f => f.DirectoryExists(fileSystem.Object.PackagesFolder)).Returns(false);
            }

            private static void VerifyNoMigration(Mock<IFileSystem> fileSystem)
            {
                fileSystem.Verify(f =>
                    f.CopyDirectory(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);

                fileSystem.Verify(f =>
                    f.MoveDirectory(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

                fileSystem.Verify(f =>
                    f.Copy(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);

                fileSystem.Verify(f =>
                    f.Move(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            }
        }
    }
}
