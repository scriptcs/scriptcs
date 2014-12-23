using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using ScriptCs.Contracts;
using Should;
using Xunit;

namespace ScriptCs.Engine.Mono.Tests
{
    public class MonoModuleTest
    {
        public class TheInitializeMethod
        {
            private Mock<IModuleConfiguration> _configMock = new Mock<IModuleConfiguration>();
            private IModuleConfiguration _config;
            private MonoModule _module = new MonoModule();
            private IDictionary<Type, object> _overrides = new Dictionary<Type, object>();

            public TheInitializeMethod()
            {
                _configMock.SetupGet(c => c.Debug).Returns(false);
                _configMock.SetupGet(c => c.IsRepl).Returns(false);
                _configMock.SetupGet(c => c.Cache).Returns(false);
                _configMock.SetupGet(c => c.Overrides).Returns(_overrides);
                _config = _configMock.Object;
            }


            [Fact]
            public void ShouldNotOverrideTheEngineIfOneIsRegistered()
            {
                var engine = new Mock<IScriptEngine>();
                _overrides[typeof(IScriptEngine)] = engine.Object;
                _module.Initialize(_config);
                _overrides[typeof(IScriptEngine)].ShouldEqual(engine.Object);
            }

            [Fact]
            public void ShouldRegisterTheEngine()
            {
                _module.Initialize(_config);
                _configMock.Verify(c=>c.ScriptEngine<IScriptEngine>(), Times.Once);
            }
        }
    }
}
