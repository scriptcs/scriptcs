using Moq;

using ScriptCs.Contracts;

using Should;

using Xunit;

namespace ScriptCs.Tests
{
    public class ScriptHostTests
    {
        public class TheGetMethod
        {
            private readonly Mock<IScriptPackContext> _mockContext = new Mock<IScriptPackContext>();
            private readonly Mock<IScriptPackManager> _mockScriptPackManager = new Mock<IScriptPackManager>();
            private readonly ScriptHost _scriptHost; 

            public TheGetMethod()
            {
                _scriptHost = new ScriptHost(_mockScriptPackManager.Object, new ScriptEnvironment(new string[0]));
                _mockScriptPackManager.Setup(s => s.Get<IScriptPackContext>()).Returns(_mockContext.Object);
            }

            [Fact]
            public void ShoulGetScriptPackFromScriptPackManagerWhenInvoked()
            {
                _scriptHost.Require<IScriptPackContext>();
                _mockScriptPackManager.Verify(s => s.Get<IScriptPackContext>());
            }
        }

        public class TheConstructor
        {
            [Fact]
            public void ShouldSetScriptEnvironment()
            {
                var environment = new ScriptEnvironment(new string[0]);
                var scriptHost = new ScriptHost(new Mock<IScriptPackManager>().Object, environment);

                scriptHost.Env.ShouldEqual(environment);
            }
        }
    }
}
