using ScriptCs.Argument;
using ScriptCs.Contracts;
using ScriptCs.Hosting;

using Xunit;
using Should;

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

                var parser = new ConfigFileParser(new ScriptConsole());
                var result = parser.Parse(file);

                result.ShouldNotBeNull();
                result.ScriptName.ShouldEqual("server.csx");
                result.Install.ShouldEqual("install test value");
            }

            [Fact]
            public void ShouldHandleNull()
            {
                var parser = new ConfigFileParser(new ScriptConsole());
                var result = parser.Parse(null);

                result.ShouldBeNull();
            }

            [Fact]
            public void ShouldHandleEmptyString()
            {
                var parser = new ConfigFileParser(new ScriptConsole());
                var result = parser.Parse("");

                result.ShouldBeNull();
            }

            [Fact]
            public void ShouldHanldeArgumentTypeConversionBool()
            {
                const string file = "{\"Install\": \"install test value\", \"script\": \"server.csx\", \"cache\": \"true\" }";

                var parser = new ConfigFileParser(new ScriptConsole());
                var result = parser.Parse(file);

                result.ShouldNotBeNull();
                result.ScriptName.ShouldEqual("server.csx");
                result.Install.ShouldEqual("install test value");
                result.Cache.ShouldEqual(true);
            }

            [Fact]
            public void ShouldHanldeArgumentTypeConversionEnum()
            {
                const string file = "{\"Install\": \"install test value\", \"script\": \"server.csx\", \"cache\": \"true\", \"log\": \"error\" }";

                var parser = new ConfigFileParser(new ScriptConsole());
                var result = parser.Parse(file);

                result.ShouldNotBeNull();
                result.ScriptName.ShouldEqual("server.csx");
                result.Install.ShouldEqual("install test value");
                result.Cache.ShouldEqual(true);
                result.LogLevel.ShouldEqual(LogLevel.Error);
            }

            [Fact]
            public void ShouldHandleConfigArgumentsCaseInsensitive()
            {
                const string file = "{\"Install\": \"install test value\", \"script\": \"server.csx\", \"cache\": \"tRUe\", \"logLEVEL\": \"TRaCE\" }";

                var parser = new ConfigFileParser(new ScriptConsole());
                var result = parser.Parse(file);

                result.ShouldNotBeNull();
                result.ScriptName.ShouldEqual("server.csx");
                result.Install.ShouldEqual("install test value");
                result.Cache.ShouldEqual(true);
                result.LogLevel.ShouldEqual(LogLevel.Trace);
            }

            [Fact]
            public void ShouldHandleConfigMalformedConfig()
            {
                const string file = "{\"Install\": \"install ";

                var parser = new ConfigFileParser(new ScriptConsole());
                var result = parser.Parse(file);

                result.ShouldBeNull();
            }
        }         
    }
}