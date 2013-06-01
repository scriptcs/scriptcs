using System;
using System.Reflection;
using System.Text;

using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;

using ScriptCs.Command;

using System.IO;

using Should;

using Xunit;

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

            [Fact]
            public void VersionCommandShouldOutputVersion()
            {
                var args = new ScriptCsArgs { Version = true };

                var fixture = new Fixture().Customize(new AutoMoqCustomization());

                var root = fixture.Create<ScriptServiceRoot>();

                var factory = new CommandFactory(root);
                var result = factory.CreateCommand(args, new string[0]);

                // clear the fake console output
                _outputText.Clear();

                result.Execute();

                _outputText.ToString().ShouldContain("scriptcs version " + _currentVersion);
            }
        }
    }
}
