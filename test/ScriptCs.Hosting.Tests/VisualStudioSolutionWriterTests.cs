using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using ScriptCs.Contracts;
using Should;
using Xunit;
using Xunit.Extensions;

namespace ScriptCs.Hosting.Tests
{
    public class VisualStudioSolutionWriterTests
    {
        public class TheWriteSolutionMethod
        {
            private Mock<IVisualStudioSolution> _solutionMock;
            private Mock<IFileSystem> _fsMock;
            private VisualStudioSolutionWriter _writer;
            private IList<ProjectItem> _nestedItems;
            private string _launcher;

            public TheWriteSolutionMethod()
            {
                _writer = new VisualStudioSolutionWriter();
                _solutionMock = new Mock<IVisualStudioSolution>();
                _fsMock = new Mock<IFileSystem>();
                _fsMock.SetupGet(fs => fs.PackagesFolder).Returns(@"packages");
                _fsMock.SetupGet(fs => fs.HostBin).Returns("bin");
                _fsMock.SetupGet(fs => fs.CurrentDirectory).Returns("root");
                _fsMock.Setup(fs=>fs.EnumerateFilesAndDirectories(It.IsAny<string>(), It.IsAny<string>(), SearchOption.AllDirectories)).Returns(new [] {Path.Combine("root","file1.csx"), Path.Combine("root", "child1", "file2.csx"), Path.Combine("root", "child1", "child2", "file3.csx")});
                _fsMock.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(false);
                _fsMock.SetupGet(fs => fs.TempPath).Returns("temp");
                _nestedItems = new List<ProjectItem>();
                _launcher = _writer.WriteSolution(_fsMock.Object, "test.csx", _solutionMock.Object, _nestedItems);
            }

            [Fact]
            public void ShouldAddTheScriptcsProject()
            {
                var scriptcsPath = Path.Combine("bin", "scriptcs.exe");
                _solutionMock.Verify(fs=>fs.AddScriptcsProject(scriptcsPath, "root", "test.csx -debug -loglevel info", false, It.IsAny<Guid>()));
            }

            [Fact]
            public void ShoulGetDirectoryInfo()
            {
                _writer._root.Files.ShouldContain("file1.csx");
                var child = _writer._root.Directories.Values.First();
                child.Files.ShouldContain("file2.csx");
                child = child.Directories.Values.First();
                child.Files.ShouldContain("file3.csx");
            }

            [Fact]
            public void ShouldCallAddDirectoryProjectForChild()
            {
                var child1 = _writer._root.Directories.Values.First();
                var child2 = child1.Directories.Values.First();
                _nestedItems.Where(i => i.Project == child1.Guid).Count().ShouldEqual(1);
                _nestedItems.Where(i => i.Project == child2.Guid).Count().ShouldEqual(1);
            }

            [Fact]
            public void ShouldAddGlobal()
            {
                _solutionMock.Verify(s=>s.AddGlobal(It.IsAny<Guid>(), _nestedItems));
            }

            [Fact]
            public void ShouldWriteTheSolution()
            {
                _fsMock.Verify(fs=>fs.WriteToFile(It.IsAny<string>(), It.IsAny<string>()));
            }

            [Fact]
            public void ShouldReturnALauncherInTheTempFolder()
            {
                _launcher.ShouldNotBeNull();
                _launcher.ShouldStartWith("temp");
            }           
        }
    }
}
