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
                var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;

                // Act
                factory.CreateCommand(args, new string[0]).Execute();

                // Assert
                console.Verify(x => x.WriteLine("scriptcs version " + currentVersion));
            }
        }
    }
}
