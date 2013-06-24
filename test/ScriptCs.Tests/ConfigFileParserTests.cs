using ScriptCs.Argument;
using Xunit;

namespace ScriptCs.Tests
{
    public class ConfigFileParserTests
    {
        public class ParseMethod
        {
            [Fact]
            public void ShouldHandleConfigFile()
            {
                const string file = "{\"Install\": \"install test value\", \"script\": \"server.csx\" }";

                var parser = new ConfigFileParser();
                var result = parser.Parse(file);

                Assert.NotNull(result);
                Assert.Equal(result.ScriptName, "server.csx");
                Assert.Equal(result.Install, "install test value");
            }

            [Fact]
            public void ShouldHandleNull()
            {
                var parser = new ConfigFileParser();
                var result = parser.Parse(null);

                Assert.Null(result);
            }

            [Fact]
            public void ShouldHandleEmptyString()
            {
                var parser = new ConfigFileParser();
                var result = parser.Parse("");

                Assert.Null(result);
            }

            [Fact]
            public void ShouldHanldeArgumentTypeConversionBool()
            {
                const string file = "{\"Install\": \"install test value\", \"script\": \"server.csx\", \"debug\": \"true\" }";

                var parser = new ConfigFileParser();
                var result = parser.Parse(file);

                Assert.NotNull(result);
                Assert.Equal(result.ScriptName, "server.csx");
                Assert.Equal(result.Install, "install test value"); 
                Assert.Equal(result.Debug, true);
            }

            [Fact]
            public void ShouldHanldeArgumentTypeConversionEnum()
            {
                const string file = "{\"Install\": \"install test value\", \"script\": \"server.csx\", \"debug\": \"true\", \"log\": \"error\" }";

                var parser = new ConfigFileParser();
                var result = parser.Parse(file);

                Assert.NotNull(result);
                Assert.Equal(result.ScriptName, "server.csx");
                Assert.Equal(result.Install, "install test value");
                Assert.Equal(result.Debug, true);
                Assert.Equal(result.LogLevel, LogLevel.Error);
            }

            [Fact]
            public void ShouldHandleConfigArgumentsCaseInsensitive()
            {
                const string file = "{\"Install\": \"install test value\", \"script\": \"server.csx\", \"deBUg\": \"tRUe\", \"logLEVEL\": \"TRaCE\" }";

                var parser = new ConfigFileParser();
                var result = parser.Parse(file);

                Assert.NotNull(result);
                Assert.Equal(result.ScriptName, "server.csx");
                Assert.Equal(result.Install, "install test value");
                Assert.Equal(result.Debug, true);
                Assert.Equal(result.LogLevel, LogLevel.Trace);
            }

            [Fact]
            public void ShouldHandleConfigMalformedConfig()
            {
                const string file = "{\"Install\": \"install ";

                var parser = new ConfigFileParser();
                var result = parser.Parse(file);

                Assert.Null(result);
            }
        }         
    }
}