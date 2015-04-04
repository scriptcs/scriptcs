using Should;
using Xunit.Extensions;

namespace ScriptCs.Tests
{
    using ScriptCs.Contracts;

    public class ConfigTests
    {
        public class TheApplyMethod
        {
            [Theory]
            [InlineData(true, null, LogLevel.Error, LogLevel.Debug)]
            [InlineData(false, null, LogLevel.Error, LogLevel.Error)]
            [InlineData(true, LogLevel.Error, LogLevel.Trace, LogLevel.Error)]
            [InlineData(true, null, LogLevel.Trace, LogLevel.Trace)]
            public void CalculatesTheLogLevel(
                bool debug, LogLevel? logLevel, LogLevel currentLogLevel, LogLevel expectedLogLevel)
            {
                // arrange
                var mask = new ConfigMask { Debug = debug, LogLevel = logLevel };
                var config = new Config { LogLevel = currentLogLevel };

                // act
                config = config.Apply(mask);

                // assert
                config.LogLevel.ShouldEqual(expectedLogLevel);
            }

            [Theory]
            [InlineData(null, null)]
            [InlineData(".csx", ".csx")]
            [InlineData(".fsx", ".fsx")]
            [InlineData("a", "a.csx")] // :eyes: here it is!
            [InlineData("a.", "a.")]
            [InlineData("a.csx", "a.csx")]
            [InlineData("a.fsx", "a.fsx")]
            public void AddsTheDefaultExtension(string scriptName, string expectedScriptName)
            {
                // arrange
                var mask = new ConfigMask { ScriptName = scriptName };
                var config = new Config();

                // act
                config = config.Apply(mask);

                // assert
                config.ScriptName.ShouldEqual(expectedScriptName);
            }
        }
    }
}
