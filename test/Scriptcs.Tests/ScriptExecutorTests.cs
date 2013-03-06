namespace Scriptcs.Tests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;

    using Moq;

    using Scriptcs.Contracts;

    using Should;
    
    using Xunit;

    public class ScriptExecutorTests
    {
        [Fact]
        public void ShouldAddSystemAndSystemCoreReferencesToEngine()
        {
            // arrange
            var fileSystem = new Mock<IFileSystem>();
            var scriptEngine = new Mock<IScriptEngine>();
            var session = new Mock<ISession>();

            scriptEngine.Setup(e => e.AddReference("System")).Verifiable();
            scriptEngine.Setup(e => e.AddReference("System.Core")).Verifiable();
            scriptEngine.Setup(e => e.CreateSession()).Returns(session.Object);

            var currentDirectory = @"C:\";
            fileSystem.Setup(fs => fs.CurrentDirectory).Returns(currentDirectory);

            var scriptExecutor = this.CreateScriptExecutor(
                fileSystem.Object,
                new ExportFactory<IScriptEngine>(
                    () => new Tuple<IScriptEngine, Action>(scriptEngine.Object, null)));

            var scriptName = "script.csx";
            var paths = new string[0];
            IEnumerable<IScriptcsRecipe> recipes = null;

            // act
            scriptExecutor.Execute(scriptName, paths, recipes);

            // assert
            scriptEngine.Verify(e => e.AddReference("System"), Times.Once());
            scriptEngine.Verify(e => e.AddReference("System.Core"), Times.Once());
        }

        [Fact]
        public void ShouldSetEngineBaseDirectoryBasedOnCurrentDirectoryAndBinFolder()
        {
            // arrange
            var scriptEngine = new Mock<IScriptEngine>();
            var fileSystem = new Mock<IFileSystem>();
            var session = new Mock<ISession>();
            scriptEngine.Setup(e => e.CreateSession()).Returns(session.Object);

            var currentDirectory = @"C:\";
            fileSystem.Setup(fs => fs.CurrentDirectory).Returns(currentDirectory);

            scriptEngine.SetupProperty(e => e.BaseDirectory);

            var scriptExecutor = this.CreateScriptExecutor(
                fileSystem.Object,
                new ExportFactory<IScriptEngine>(
                    () => new Tuple<IScriptEngine, Action>(scriptEngine.Object, null)));

            var scriptName = "script.csx";
            var paths = new string[0];
            IEnumerable<IScriptcsRecipe> recipes = null;

            // act
            scriptExecutor.Execute(scriptName, paths, recipes);

            // assert
            string expectedBaseDirectory = currentDirectory + @"\bin";
            expectedBaseDirectory.ShouldEqual(scriptEngine.Object.BaseDirectory);
        }

        [Fact]
        public void ShouldCreateCurrentDirectoryIfItDoesNotExist()
        {
            // arrange
            var scriptEngine = new Mock<IScriptEngine>();
            var fileSystem = new Mock<IFileSystem>();
            var session = new Mock<ISession>();

            scriptEngine.Setup(e => e.CreateSession()).Returns(session.Object);

            var currentDirectory = @"C:\";
            fileSystem.Setup(fs => fs.CurrentDirectory).Returns(currentDirectory);

            var binDirectory = currentDirectory + @"\bin";

            fileSystem.Setup(fs => fs.DirectoryExists(binDirectory)).Returns(false).Verifiable();
            fileSystem.Setup(fs => fs.CreateDirectory(binDirectory)).Verifiable();

            var scriptExecutor = this.CreateScriptExecutor(
                fileSystem.Object,
                new ExportFactory<IScriptEngine>(
                    () => new Tuple<IScriptEngine, Action>(scriptEngine.Object, null)));

            var scriptName = "script.csx";
            var paths = new string[0];
            IEnumerable<IScriptcsRecipe> recipes = null;

            // act
            scriptExecutor.Execute(scriptName, paths, recipes);

            // assert
            fileSystem.Verify(fs => fs.DirectoryExists(binDirectory), Times.Once());
            fileSystem.Verify(fs => fs.CreateDirectory(binDirectory), Times.Once());
        }

        [Fact]
        public void ShouldExecuteScriptReadFromFileInSession()
        {
            // arrange
            var scriptEngine = new Mock<IScriptEngine>();
            var fileSystem = new Mock<IFileSystem>();
            var session = new Mock<ISession>();

            string code = Guid.NewGuid().ToString();

            scriptEngine.Setup(e => e.CreateSession()).Returns(session.Object);

            session.Setup(s => s.Execute(code)).Returns(null).Verifiable();

            var currentDirectory = @"C:\";
            fileSystem.Setup(fs => fs.CurrentDirectory).Returns(currentDirectory);

            var scriptExecutor = this.CreateScriptExecutor(
                fileSystem.Object,
                new ExportFactory<IScriptEngine>(
                    () => new Tuple<IScriptEngine, Action>(scriptEngine.Object, null)));

            var scriptName = "script.csx";
            var paths = new string[0];
            IEnumerable<IScriptcsRecipe> recipes = null;

            fileSystem.Setup(fs => fs.ReadFile(currentDirectory + @"\" + scriptName)).Returns(code).Verifiable();

            // act
            scriptExecutor.Execute(scriptName, paths, recipes);

            // assert
            fileSystem.Verify(fs => fs.ReadFile(currentDirectory + @"\" + scriptName), Times.Once());
            session.Verify(s => s.Execute(code), Times.Once());
        }

        private ScriptExecutor CreateScriptExecutor(IFileSystem fileSystem, ExportFactory<IScriptEngine> scriptEngineFactory) 
        {
            return new ScriptExecutor(fileSystem, scriptEngineFactory);
        }
    }
}