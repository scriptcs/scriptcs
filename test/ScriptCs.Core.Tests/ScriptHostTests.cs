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
                _scriptHost = new ScriptHost(_mockScriptPackManager.Object, new string[0]);
                _mockScriptPackManager.Setup(s => s.Get<IScriptPackContext>()).Returns(_mockContext.Object);
            }

            [Fact]
            public void ShoulGetScriptPackFromScriptPackManagerWhenInvoked()
            {
                var result = _scriptHost.Require<IScriptPackContext>();
                _mockScriptPackManager.Verify(s => s.Get<IScriptPackContext>());
            }
        }

        public class TheConstructor
        {
            [Fact]
            public void ShouldSetScriptArgsWhenConstructed()
            {
                var scriptArgs = new string[0];
                var scriptHost = new ScriptHost(null, scriptArgs);
                scriptHost.ScriptArgs.ShouldEqual(scriptArgs);
            }
        }
    }
}
