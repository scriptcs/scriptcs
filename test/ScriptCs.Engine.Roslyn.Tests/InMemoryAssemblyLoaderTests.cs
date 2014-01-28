using Common.Logging;
using Moq;
using ScriptCs.Contracts;
using ScriptCs.Engine.Roslyn;
using Should;
using Xunit.Extensions;

namespace ScriptCs.Tests
{
    public class InMemoryAssemblyLoaderTests
    {
        public class TheShouldCompileMethod
        {
            [Theory, ScriptCsAutoData]
            public void ShouldReturnTrue()
            {
                var hostFactory = new Mock<IScriptHostFactory>();
                var logger = new Mock<ILog>();

                var loader = new InMemoryAssemblyLoader(hostFactory.Object, logger.Object);

                loader.ShouldCompile().ShouldBeTrue();
            }
        }
    }
}
