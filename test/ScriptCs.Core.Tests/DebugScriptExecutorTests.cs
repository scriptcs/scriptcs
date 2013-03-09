using Moq;

namespace ScriptCs.Tests
{
    using Xunit;

    public class DebugScriptExecutorTests
    {
        public static ScriptExecutor CreateScriptExecutor(
            Mock<IFileSystem> fileSystem = null,
            Mock<IFilePreProcessor> fileProcessor = null,
            Mock<IScriptEngine> scriptEngine = null,
            Mock<IScriptHostFactory> scriptHostFactory = null)
        {
            fileSystem = fileSystem ?? new Mock<IFileSystem>();

            fileProcessor = fileProcessor ?? new Mock<IFilePreProcessor>();

            if (scriptEngine == null)
            {
                var mockSession = new Mock<ISession>();
                mockSession.Setup(s => s.AddReference(It.IsAny<string>()));
                mockSession.Setup(s => s.Execute(It.IsAny<string>())).Returns(new object());

                scriptEngine = new Mock<IScriptEngine>();
                scriptEngine.SetupProperty(e => e.BaseDirectory);
                scriptEngine.Setup(e => e.CreateSession()).Returns(mockSession.Object);
                scriptEngine.Setup(e => e.CreateSession(It.IsAny<ScriptHost>())).Returns(mockSession.Object);
            }

            if (scriptHostFactory == null)
            {
                return new ScriptExecutor(fileSystem.Object, fileProcessor.Object, scriptEngine.Object);
            }

            return new ScriptExecutor(
                fileSystem.Object, fileProcessor.Object, scriptEngine.Object, scriptHostFactory.Object);
        }

        public class TheExecuteMethod
        {
            [Fact(Skip = "No code yet")]
            public void ShouldAddSystemDiagnosticsUsingAutomaticallyWhenDebugging()
            {
            }
        }
    }
}
