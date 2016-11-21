using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using ScriptCs.Contracts;
using ScriptCs.Hosting.ReplCommands;
using Xunit;

namespace ScriptCs.Hosting.Tests.ReplCommands
{
    public class OpenVsCommandTests
    {
        public class TheExecuteCommand
        {
            private Mock<OpenVsCommand> _mockCommand;
            private OpenVsCommand _command;
            private Mock<IConsole> _mockConsole;
            private Mock<IVisualStudioSolutionWriter> _mockWriter;
            private Mock<IRepl> _mockRepl;

            public TheExecuteCommand()
            {
                _mockConsole = new Mock<IConsole>();
                _mockWriter = new Mock<IVisualStudioSolutionWriter>();
                _mockRepl = new Mock<IRepl>();
                _mockRepl.SetupGet(r => r.FileSystem).Returns(new FileSystem());
                _mockCommand = new Mock<OpenVsCommand>(_mockConsole.Object, _mockWriter.Object);
                _mockCommand.Setup(c => c.PlatformID).Returns(PlatformID.Win32NT);
                _mockCommand.Setup(c => c.LaunchSolution(It.IsAny<string>()));
                _mockCommand.Protected();
                _command = _mockCommand.Object;
            }

            [Fact]
            public void OutputsAMessageIfNotWindows8()
            {
                _mockCommand.Setup(c => c.PlatformID).Returns(PlatformID.MacOSX);
                _command.Execute(_mockRepl.Object, new object[] {"test.csx"});
                _mockConsole.Verify(c=>c.WriteLine("Requires Windows 8 or later to run"));
            }

            [Fact]
            public void CreatesTheSolution()
            {
                _command.Execute(_mockRepl.Object, new object[] {"test.csx"});
                _mockWriter.Verify(
                    w =>
                        w.WriteSolution(It.IsAny<IFileSystem>(), It.IsAny<string>(), It.IsAny<IVisualStudioSolution>(),
                            It.IsAny<IList<ProjectItem>>()));
            }

            [Fact]
            public void LaunchesTheSolution()
            {
                _command.Execute(_mockRepl.Object, new object[] { "test.csx" });
                _mockCommand.Verify(c=>c.LaunchSolution(It.IsAny<string>()));
            }

        }
    }
}
