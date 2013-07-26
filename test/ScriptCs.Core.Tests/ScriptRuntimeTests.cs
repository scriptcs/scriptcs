using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core.Registration;
using Common.Logging;
using Moq;
using ScriptCs.Contracts;
using ScriptCs.Package;
using ScriptCs.Package.InstallationProvider;
using Xunit;
using Should;

namespace ScriptCs.Tests
{
    public class ScriptRuntimeTests
    {
        public class TheInitializeMethod
        {
            private Mock<ILog> _mockLogger = new Mock<ILog>();
            private ScriptServices _scriptServices = new ScriptServices(null, null, null, null,null,null,null,null,null,null);
            private Mock<IRuntimeContainerFactory> _mockFactory = new Mock<IRuntimeContainerFactory>();
            private ScriptRuntime _runtime = null;

            public TheInitializeMethod()
            {
                var builder = new ContainerBuilder();
                builder.RegisterInstance<ILog>(_mockLogger.Object);
                builder.RegisterInstance<ScriptServices>(_scriptServices);
                var container = builder.Build();
                _mockFactory.SetupGet(f => f.Container).Returns(container);
                _runtime = new ScriptRuntime(_mockFactory.Object);
            }

            [Fact]
            public void ShouldResolveLogger()
            {
                _runtime.Logger.ShouldEqual(_mockLogger.Object);
            }

            [Fact]
            public void ShouldResolveScriptServices()
            {
                _runtime.ScriptServices.ShouldEqual(_scriptServices);

            }

        }
    }
}
