using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptCs.Contracts;
using ScriptCs;
using Xunit;
using Should;
using Moq;
using PowerArgs;

namespace ScriptCs.Tests
{
    public class ScriptHostTests
    {
        public class TheGetMethod
        {
            private Mock<IScriptPackContext> _mockContext = new Mock<IScriptPackContext>();
            private Mock<IScriptPackManager> _mockScriptPackManager = new Mock<IScriptPackManager>();
            private ScriptHost _scriptHost; 

            public TheGetMethod()
            {
                _scriptHost = new ScriptHost("", _mockScriptPackManager.Object);
                _mockScriptPackManager.Setup(s => s.Get<IScriptPackContext>()).Returns(_mockContext.Object);
            }

            [Fact]
            public void ShouldGetScriptPackFromScriptPackManagerWhenInvoked()
            {
                var result = _scriptHost.Require<IScriptPackContext>();
                _mockScriptPackManager.Verify(s=>s.Get<IScriptPackContext>());
            }

            [Fact]
            public void ShouldReturnEmptyArgumentStringIfNull()
            {
                var host = new ScriptHost(null, null);
                Assert.Equal("", host.Args());
            }

            [Fact]
            public void ShouldReturnParsedArgumentStringUsingPowerArgs()
            {
                var host = new ScriptHost("'Josh Wink' -version 1111 -f", null);
                Assert.Equal("Josh Wink", host.Args<TestArgs>().Name);
                Assert.Equal(1111, host.Args<TestArgs>().Version);
            }

            public class TestArgs
            {
                [ArgPosition(0)]
                [ArgDescription("Positional arg")]
                public string Name { get;set; }

                [ArgShortcut("version")]
                [ArgDescription("Named arg with required value")]
                public int Version { get; set;}

                [ArgShortcut("f")]
                [ArgDescription("Named flag")]
                public bool Flag { get;set; }
            }
        }
    }
}
