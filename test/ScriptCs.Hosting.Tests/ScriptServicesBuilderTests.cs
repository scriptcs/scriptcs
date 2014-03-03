using Common.Logging;
using Moq;
using ScriptCs.Contracts;
using ScriptCs.Hosting;

using Should;

using Xunit;

namespace ScriptCs.Tests
{
    public class ScriptServicesBuilderTests
    {
        public class TheBuildMethod
        {
            private Mock<ILog> _mockLogger = new Mock<ILog>();

            private ScriptServices _scriptServices = new ScriptServices(null, null, null, null, null, null, null, null, null, null);
            private Mock<IRuntimeServices> _mockFactory = new Mock<IRuntimeServices>();
            private Mock<IConsole> _mockConsole = new Mock<IConsole>();
            private ScriptServicesBuilder _builder = null;

            public TheBuildMethod()
            {
                _mockFactory.Setup(f => f.GetScriptServices()).Returns(_scriptServices);
                _builder = new ScriptServicesBuilder(_mockConsole.Object, _mockLogger.Object, _mockFactory.Object);
            }

            [Fact]
            public void ShouldResolveScriptServices()
            {
                _builder.Build().ShouldEqual(_scriptServices);
            }
        }
    }
}
