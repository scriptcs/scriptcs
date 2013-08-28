using Xunit;

namespace ScriptCs.Tests
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Moq;

    using ScriptCs.Contracts;

    using Should;

    public class FileScriptSourceTests
    {
        public class PathProperty
        {
            [Fact]
            public void ShouldReturnPathProvidedInConstructorThroughProperty()
            {
                const string ExpectedPath = @"C:\";
                var scriptSource = new FileScriptSource(ExpectedPath, null);

                scriptSource.Path.ShouldEqual(ExpectedPath);
            }
        }

        public class GetLinesMethod
        {
            [Fact]
            public async Task ShouldReturnLinesFromFileSystemFile()
            {
                const string ExpectedPath = @"C:\";
                var fileSystem = new Mock<IFileSystem>();

                var expectedLines = new List<string> { "line1", "line2", "line3" };

                fileSystem.Setup(fs => fs.ReadFileLines(ExpectedPath)).Returns(expectedLines.ToArray());

                var scriptSource = new FileScriptSource(ExpectedPath, fileSystem.Object);

                var lines = await scriptSource.GetLines();

                lines.Count.ShouldEqual(3);

                lines.ShouldContain("line1");
                lines.ShouldContain("line2");
                lines.ShouldContain("line3");
            }
        }
    }
}
