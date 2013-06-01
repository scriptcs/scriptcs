using System;
using System.Reflection;
using System.Text;

using ScriptCs.Command;

using System.IO;

using Should;

using Xunit.Extensions;

namespace ScriptCs.Tests
{
    public class VersionCommandTests
    {
        public class ExecuteMethod
        {
            private readonly Version _currentVersion;

            private readonly StringBuilder _outputText;

            private readonly StringWriter _mockConsole;

            public ExecuteMethod()
            {
                _currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
                _outputText = new StringBuilder();
                _mockConsole = new StringWriter(_outputText);
                Console.SetOut(_mockConsole);
            }

            [Theory, ScriptCsAutoData]
            public void VersionCommandShouldOutputVersion(CommandFactory factory)
            {
                // Arrange
                var args = new ScriptCsArgs { Version = true };

                // Act
                factory.CreateCommand(args, new string[0]).Execute();

                // Assert
                _outputText.ToString().ShouldContain("scriptcs version " + _currentVersion);
            }
        }
    }
}
