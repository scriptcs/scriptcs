using System.IO;
using Ploeh.AutoFixture.Xunit;
using ScriptCs.Contracts;
using ScriptCs.Tests;
using Should;
using Moq;
using Xunit.Extensions;

namespace ScriptCs.Hosting.Tests
{
    public class FilePathFinderTests
    {
        public class TheFindPossibleAssemblyNamesMethod
        {
            [Theory, ScriptCsAutoData]
            public void ShouldReturnFragmentWhenNoPossiblePaths(string fragment, [Frozen] Mock<IFileSystem> fileSystemMock, FilePathFinder filePathFinder)
            {
                fileSystemMock.Setup(fs => fs.EnumerateFilesAndDirectories(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>())).Returns(new string[] {});

                var possiblePaths = filePathFinder.FindPossibleAssemblyNames(fragment, fileSystemMock.Object);

                possiblePaths.ShouldEqual(new [] { fragment });
            }

            [Theory, ScriptCsAutoData]
            public void ShouldReturnAssemblyNamesWhenPossible(string fragment, [Frozen] Mock<IFileSystem> fileSystemMock, FilePathFinder filePathFinder)
            {
                const string path = @"C:\dir1\dir2\";
                string firstPossibility = path + fragment + "e1.dll";
                string secondPossibility = path + fragment + "e2.dll";

                fileSystemMock.Setup(fs => fs.EnumerateFilesAndDirectories(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>())).Returns(new [] { firstPossibility, secondPossibility });

                var possiblePaths = filePathFinder.FindPossibleAssemblyNames(fragment, fileSystemMock.Object);

                possiblePaths.ShouldEqual(new[] { fragment + "e1.dll", fragment + "e2.dll" });
            }

            [Theory, ScriptCsAutoData]
            public void ShoulOnlyReturnNamesWithDllSuffix(string fragment, [Frozen] Mock<IFileSystem> fileSystemMock, FilePathFinder filePathFinder)
            {
                string path = @"C:\dir1\dir2\";
                string firstPossibility = path + fragment + "e1";
                string secondPossibility = path + fragment + "e2.dll";

                fileSystemMock.Setup(fs => fs.EnumerateFilesAndDirectories(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>())).Returns(new[] { firstPossibility, secondPossibility });

                var possiblePaths = filePathFinder.FindPossibleAssemblyNames(fragment, fileSystemMock.Object);

                possiblePaths.ShouldEqual(new[] { fragment + "e2.dll" });
            }
        }

        public class TheFindPossibleFilePathsMethod
        {
            [Theory, ScriptCsAutoData]
            public void ShouldReturnFragmentWhenNoPossiblePaths(string fragment, [Frozen] Mock<IFileSystem> fileSystemMock, FilePathFinder filePathFinder)
            {
                fileSystemMock.Setup(fs => fs.EnumerateFilesAndDirectories(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>())).Returns(new string[] { });

                var possiblePaths = filePathFinder.FindPossibleFilePaths(fragment, fileSystemMock.Object);

                possiblePaths.ShouldEqual(new[] { fragment });
            }

            [Theory, ScriptCsAutoData]
            public void ShouldReturnFilePathsWhenPossible(string fragment, [Frozen] Mock<IFileSystem> fileSystemMock, FilePathFinder filePathFinder)
            {
                const string path = @"C:\dir1\dir2\";
                string firstPossibility = path + fragment + "e1";
                string secondPossibility = path + fragment + "e2";

                fileSystemMock.Setup(fs => fs.EnumerateFilesAndDirectories(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>())).Returns(new[] { firstPossibility, secondPossibility });

                var possiblePaths = filePathFinder.FindPossibleFilePaths(fragment, fileSystemMock.Object);

                possiblePaths.ShouldEqual(new[] { fragment + "e1", fragment + "e2" });
            }

            [Theory, ScriptCsAutoData]
            public void ShouldRemoveDuplicates(string fragment, [Frozen] Mock<IFileSystem> fileSystemMock, FilePathFinder filePathFinder)
            {
                const string path = @"C:\dir1\dir2\";
                string firstPossibility = path + fragment + "e1";
                string secondPossibility = path + fragment + "e2";

                fileSystemMock.Setup(fs => fs.EnumerateFilesAndDirectories(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>())).Returns(new[] { firstPossibility, secondPossibility, firstPossibility });

                var possiblePaths = filePathFinder.FindPossibleFilePaths(fragment, fileSystemMock.Object);

                possiblePaths.ShouldEqual(new[] { fragment + "e1", fragment + "e2" });
            }

            [Theory, ScriptCsAutoData]
            public void ShouldSplitFragmentsWithPaths(string fragment, [Frozen] Mock<IFileSystem> fileSystemMock, FilePathFinder filePathFinder)
            {
                const string path = @"..\dir1\dir2\";
                const string currentDir = @"D:\foo";

                fileSystemMock.Setup(fs => fs.CurrentDirectory).Returns(currentDir);

                filePathFinder.FindPossibleFilePaths(path + fragment, fileSystemMock.Object);

                fileSystemMock.Verify(fs => fs.EnumerateFilesAndDirectories(currentDir + @"\" + path, fragment + "*", SearchOption.TopDirectoryOnly));
            }
        }
    }
}
