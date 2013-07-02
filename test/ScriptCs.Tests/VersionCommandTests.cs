using System.Diagnostics;
using System.Reflection;

using Moq;

using Ploeh.AutoFixture.Xunit;

using ScriptCs.Command;
using ScriptCs.Contracts;

using Xunit.Extensions;

namespace ScriptCs.Tests
{
    public class VersionCommandTests
    {
        public class ExecuteMethod
        {
            [Theory, ScriptCsAutoData]
            public void VersionCommandShouldOutputVersion([Frozen] Mock<IConsole> console, CommandFactory factory)
            {
                // Arrange
                var args = new ScriptCsArgs { Version = true };

                var assembly = typeof(ScriptCsArgs).Assembly;
                var currentVersion = FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;

                // Act
                factory.CreateCommand(args, new string[0]).Execute();

                // Assert
                console.Verify(x => x.WriteLine(It.Is<string>(y => y.Contains(currentVersion.ToString()))));
            }
        }
    }
}
