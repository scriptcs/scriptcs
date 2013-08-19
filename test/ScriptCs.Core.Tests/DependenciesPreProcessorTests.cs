namespace ScriptCs.Tests
{
    using System.Collections.Generic;
    using System.Linq;

    using Moq;

    using Ploeh.AutoFixture.Xunit;

    using ScriptCs.Contracts;

    using Should;

    using Xunit.Extensions;

    public class DependenciesPreProcessorTests
    {
        public class GetDependenciesMethod
        {
            [Theory]
            [ScriptCsAutoData]
            public void ShouldRetrieveReferencesWhenFileExists(
                [Frozen] Mock<IReferenceLineProcessor> referenceLineProcessor,
                [Frozen] Mock<ILoadLineProcessor> loadLineProcessor,
                [Frozen] Mock<IFileSystem> fileSystem,
                DependenciesPreProcessor preProcessor)
            {
                const string Path = "script.csx";

                var scriptLines = new List<string> { "#r System", "#r MyAssembly.dll", "#r MyAssembly2.dll", "Console.WriteLine(\"test\");"};

                fileSystem.Setup(fs => fs.ReadAllLines(Path)).Returns(scriptLines).Verifiable();

                referenceLineProcessor.Setup(rlp => rlp.GetDirectiveArgument("#r System"))
                                      .Returns("System").Verifiable();
                referenceLineProcessor.Setup(rlp => rlp.GetArgumentFullPath("System"))
                                      .Returns("System").Verifiable();

                referenceLineProcessor.Setup(rlp => rlp.GetDirectiveArgument("#r MyAssembly.dll"))
                                      .Returns("MyAssembly.dll").Verifiable();
                referenceLineProcessor.Setup(rlp => rlp.GetArgumentFullPath("MyAssembly.dll"))
                                      .Returns("MyAssembly.dll").Verifiable();

                referenceLineProcessor.Setup(rlp => rlp.GetDirectiveArgument("#r MyAssembly2.dll"))
                                      .Returns("MyAssembly2.dll").Verifiable();
                referenceLineProcessor.Setup(rlp => rlp.GetArgumentFullPath("MyAssembly2.dll"))
                                      .Returns("MyAssembly2.dll").Verifiable();

                fileSystem.Setup(fs => fs.FileExists("MyAssembly.dll")).Returns(true).Verifiable();
                fileSystem.Setup(fs => fs.FileExists("MyAssembly2.dll")).Returns(true).Verifiable();
                fileSystem.Setup(fs => fs.FileExists("System")).Returns(false).Verifiable();

                var dependencies = preProcessor.GetDependencies(Path).ToList();

                dependencies.Count.ShouldEqual(2);

                dependencies.ShouldContain("MyAssembly.dll");
                dependencies.ShouldContain("MyAssembly2.dll");

                fileSystem.Verify(fs => fs.FileExists("MyAssembly.dll"), Times.Once());
                fileSystem.Verify(fs => fs.FileExists("MyAssembly2.dll"), Times.Once());
                fileSystem.Verify(fs => fs.FileExists("System"), Times.Once());

                fileSystem.Verify(fs => fs.ReadAllLines(Path), Times.Once());
                referenceLineProcessor.Verify(rlp => rlp.GetArgumentFullPath("MyAssembly.dll"), Times.Once());
                referenceLineProcessor.Verify(rlp => rlp.GetArgumentFullPath("MyAssembly2.dll"), Times.Once());
            }

            [Theory]
            [ScriptCsAutoData]
            public void ShouldRetrieveLoadFilesFromFirstFile(
                [Frozen] Mock<IReferenceLineProcessor> referenceLineProcessor,
                [Frozen] Mock<ILoadLineProcessor> loadLineProcessor,
                [Frozen] Mock<IFileSystem> fileSystem,
                DependenciesPreProcessor preProcessor)
            {
                const string Path = "script.csx";

                var scriptLines = new List<string> { "#load file.csx", "#load file2.csx", "Console.WriteLine(\"test\");" };

                fileSystem.Setup(fs => fs.ReadAllLines(Path)).Returns(scriptLines).Verifiable();

                loadLineProcessor.Setup(rlp => rlp.GetDirectiveArgument("#load file.csx"))
                                      .Returns("file.csx").Verifiable();
                loadLineProcessor.Setup(rlp => rlp.GetArgumentFullPath("file.csx"))
                                      .Returns("file.csx").Verifiable();

                loadLineProcessor.Setup(rlp => rlp.GetDirectiveArgument("#load file2.csx"))
                                      .Returns("file2.csx").Verifiable();
                loadLineProcessor.Setup(rlp => rlp.GetArgumentFullPath("file2.csx"))
                                      .Returns("file2.csx").Verifiable();

                var dependencies = preProcessor.GetDependencies(Path).ToList();

                dependencies.Count.ShouldEqual(2);

                dependencies.ShouldContain("file.csx");
                dependencies.ShouldContain("file2.csx");

                fileSystem.Verify(fs => fs.ReadAllLines(Path), Times.Once());
                loadLineProcessor.Verify(rlp => rlp.GetArgumentFullPath("file.csx"), Times.Once());
                loadLineProcessor.Verify(rlp => rlp.GetArgumentFullPath("file2.csx"), Times.Once());
            }

            [Theory]
            [ScriptCsAutoData]
            public void ShouldRetrieveLoadAndReferencesFromRecursiveFiles(
                [Frozen] Mock<IReferenceLineProcessor> referenceLineProcessor,
                [Frozen] Mock<ILoadLineProcessor> loadLineProcessor,
                [Frozen] Mock<IFileSystem> fileSystem,
                DependenciesPreProcessor preProcessor)
            {
                const string Path = "script.csx";

                var scriptLines = new List<string> { "#r System", "#r Test.dll", "#load file.csx", "#load file2.csx", "Console.WriteLine(\"test\");" };
                var file = new List<string> { "#r System.Xml", "#r Test2.dll", "#load file3.csx", "Console.WriteLine(\"test\");" };
                var file2 = new List<string> { "#r System.ComponentModel.Composition", "#r Test3.dll", "#load file4.csx", "Console.WriteLine(\"test\");" };
                var file3 = new List<string> { "#r System.Core", "Console.WriteLine(\"test\");" };
                var file4 = new List<string> { "#r System.Data", "Console.WriteLine(\"test\");" };

                fileSystem.Setup(fs => fs.ReadAllLines(Path)).Returns(scriptLines).Verifiable();
                fileSystem.Setup(fs => fs.ReadAllLines("file.csx")).Returns(file).Verifiable();
                fileSystem.Setup(fs => fs.ReadAllLines("file2.csx")).Returns(file2).Verifiable();
                fileSystem.Setup(fs => fs.ReadAllLines("file3.csx")).Returns(file3).Verifiable();
                fileSystem.Setup(fs => fs.ReadAllLines("file4.csx")).Returns(file4).Verifiable();

                referenceLineProcessor.Setup(rlp => rlp.GetDirectiveArgument("#r System"))
                                      .Returns("System").Verifiable();
                referenceLineProcessor.Setup(rlp => rlp.GetArgumentFullPath("System"))
                                      .Returns("System").Verifiable();

                referenceLineProcessor.Setup(rlp => rlp.GetDirectiveArgument("#r System.Data"))
                                      .Returns("System.Data").Verifiable();
                referenceLineProcessor.Setup(rlp => rlp.GetArgumentFullPath("System.Data"))
                                      .Returns("System.Data").Verifiable();

                referenceLineProcessor.Setup(rlp => rlp.GetDirectiveArgument("#r System.Xml"))
                                      .Returns("System.Xml").Verifiable();
                referenceLineProcessor.Setup(rlp => rlp.GetArgumentFullPath("System.Xml"))
                                      .Returns("System.Xml").Verifiable();

                referenceLineProcessor.Setup(rlp => rlp.GetDirectiveArgument("#r System.ComponentModel.Composition"))
                                      .Returns("System.ComponentModel.Composition").Verifiable();
                referenceLineProcessor.Setup(rlp => rlp.GetArgumentFullPath("System.ComponentModel.Composition"))
                                      .Returns("System.ComponentModel.Composition").Verifiable();

                fileSystem.Setup(fs => fs.FileExists("Test.dll")).Returns(true).Verifiable();
                fileSystem.Setup(fs => fs.FileExists("Test2.dll")).Returns(true).Verifiable();
                fileSystem.Setup(fs => fs.FileExists("Test3.dll")).Returns(true).Verifiable();
                fileSystem.Setup(fs => fs.FileExists("System")).Returns(false).Verifiable();
                fileSystem.Setup(fs => fs.FileExists("System.Core")).Returns(false).Verifiable();
                fileSystem.Setup(fs => fs.FileExists("System.Data")).Returns(false).Verifiable();
                fileSystem.Setup(fs => fs.FileExists("System.ComponentModel.Composition")).Returns(false).Verifiable();

                loadLineProcessor.Setup(llp => llp.GetDirectiveArgument("#load file.csx"))
                                      .Returns("file.csx").Verifiable();
                loadLineProcessor.Setup(llp => llp.GetArgumentFullPath("file.csx"))
                                      .Returns("file.csx").Verifiable();

                loadLineProcessor.Setup(llp => llp.GetDirectiveArgument("#load file2.csx"))
                                      .Returns("file2.csx").Verifiable();
                loadLineProcessor.Setup(llp => llp.GetArgumentFullPath("file2.csx"))
                                      .Returns("file2.csx").Verifiable();

                referenceLineProcessor.Setup(rlp => rlp.GetDirectiveArgument("#r Test.dll"))
                                      .Returns("Test.dll").Verifiable();
                referenceLineProcessor.Setup(rlp => rlp.GetArgumentFullPath("Test.dll"))
                                      .Returns("Test.dll").Verifiable();

                referenceLineProcessor.Setup(rlp => rlp.GetDirectiveArgument("#r Test2.dll"))
                                      .Returns("Test2.dll").Verifiable();
                referenceLineProcessor.Setup(rlp => rlp.GetArgumentFullPath("Test2.dll"))
                                      .Returns("Test2.dll").Verifiable();

                loadLineProcessor.Setup(llp => llp.GetDirectiveArgument("#load file3.csx"))
                                      .Returns("file3.csx").Verifiable();
                loadLineProcessor.Setup(llp => llp.GetArgumentFullPath("file3.csx"))
                                      .Returns("file3.csx").Verifiable();

                referenceLineProcessor.Setup(rlp => rlp.GetDirectiveArgument("#r Test3.dll"))
                                      .Returns("Test3.dll").Verifiable();
                referenceLineProcessor.Setup(rlp => rlp.GetArgumentFullPath("Test3.dll"))
                                      .Returns("Test3.dll").Verifiable();

                loadLineProcessor.Setup(llp => llp.GetDirectiveArgument("#load file4.csx"))
                                     .Returns("file4.csx").Verifiable();
                loadLineProcessor.Setup(llp => llp.GetArgumentFullPath("file4.csx"))
                                      .Returns("file4.csx").Verifiable();

                var dependencies = preProcessor.GetDependencies(Path).ToList();

                dependencies.Count.ShouldEqual(7);

                dependencies.ShouldContain("file.csx");
                dependencies.ShouldContain("file2.csx");
                dependencies.ShouldContain("file3.csx");
                dependencies.ShouldContain("file4.csx");
                dependencies.ShouldContain("Test.dll");
                dependencies.ShouldContain("Test2.dll");
                dependencies.ShouldContain("Test3.dll");

                fileSystem.Verify(fs => fs.ReadAllLines(Path), Times.Once());
                fileSystem.Verify(fs => fs.ReadAllLines("file.csx"), Times.Once());
                fileSystem.Verify(fs => fs.ReadAllLines("file2.csx"), Times.Once());
                fileSystem.Verify(fs => fs.ReadAllLines("file3.csx"), Times.Once());
                fileSystem.Verify(fs => fs.ReadAllLines("file4.csx"), Times.Once());

                loadLineProcessor.Verify(rlp => rlp.GetArgumentFullPath("file.csx"), Times.Once());
                loadLineProcessor.Verify(rlp => rlp.GetArgumentFullPath("file2.csx"), Times.Once());
                loadLineProcessor.Verify(rlp => rlp.GetArgumentFullPath("file3.csx"), Times.Once());
                loadLineProcessor.Verify(rlp => rlp.GetArgumentFullPath("file4.csx"), Times.Once());
                referenceLineProcessor.Verify(rlp => rlp.GetArgumentFullPath("Test.dll"), Times.Once());
                referenceLineProcessor.Verify(rlp => rlp.GetArgumentFullPath("Test2.dll"), Times.Once());
                referenceLineProcessor.Verify(rlp => rlp.GetArgumentFullPath("Test3.dll"), Times.Once());
                referenceLineProcessor.Verify(rlp => rlp.GetArgumentFullPath("System"), Times.Once());
                referenceLineProcessor.Verify(rlp => rlp.GetArgumentFullPath("System.ComponentModel.Composition"), Times.Once());
                referenceLineProcessor.Verify(rlp => rlp.GetArgumentFullPath("System.Data"), Times.Once());
                referenceLineProcessor.Verify(rlp => rlp.GetArgumentFullPath("System.Xml"), Times.Once());
            }
        }
    }
}
