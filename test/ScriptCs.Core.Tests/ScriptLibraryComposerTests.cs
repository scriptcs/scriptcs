using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCs.Contracts;
using Moq;
using Moq.Protected;
using Ploeh.AutoFixture.Xunit;
using Xunit.Extensions;
using Should;

namespace ScriptCs.Tests
{
    public class ScriptLibraryComposerTests
    {
        public class TheGetMainScriptMethod
        {
            [Theory, ScriptCsAutoData]
            public void ShouldReturnTheScript(Mock<IPackageObject> package, ScriptLibraryComposer composer)
            {
                var files = new[] {"file.csx", "fileMain.csx", "file"};
                package.Setup(p => p.GetContentFiles()).Returns(files);
                var script = composer.GetMainScript(package.Object);
                script.ShouldEqual("fileMain.csx");
            }
        }

        public class TheProcessPackageMethod
        {
            [Theory, ScriptCsAutoData]
            public void ShouldFindThePackage(
                [Frozen] Mock<IPackageContainer> packageContainer, 
                ScriptLibraryComposer composer, 
                Mock<IPackageReference> reference, 
                Mock<IPackageObject> package)
            {
                packageContainer.Setup(c => c.FindPackage(It.IsAny<string>(), It.IsAny<IPackageReference>()))
                    .Returns(package.Object);
                package.Setup(p=>p.GetContentFiles()).Returns(Enumerable.Empty<string>());
                composer.ProcessPackage("", reference.Object, new StringBuilder(), new List<string>(), new List<string>());
                packageContainer.Verify(c => c.FindPackage(It.IsAny<string>(), It.IsAny<IPackageReference>()));  
            }

            [Theory, ScriptCsAutoData]
            public void ShouldLogAWarningIfThePackageIsMissing(
                [Frozen] TestLogProvider logProvider,
                [Frozen] Mock<IPackageContainer> packageContainer,
                ScriptLibraryComposer composer,
                Mock<IPackageReference> reference,
                Mock<IPackageObject> package)
            {
                packageContainer.Setup(c => c.FindPackage(It.IsAny<string>(), It.IsAny<IPackageReference>()))
                    .Returns((IPackageObject)null);
                reference.SetupGet(r => r.PackageId).Returns("test");
                composer.ProcessPackage("", reference.Object, new StringBuilder(), new List<string>(), new List<string>());
                packageContainer.Verify(c => c.FindPackage(It.IsAny<string>(), It.IsAny<IPackageReference>()));
                logProvider.Output.ShouldContain("WARN: Package missing: test");
            }

            

            [Theory, ScriptCsAutoData]
            public void ShouldPreProcessTheScriptFile(
                [Frozen] Mock<IPackageContainer> packageContainer,
                [Frozen] Mock<IFilePreProcessor> preProcessor,
                ScriptLibraryComposer composer,
                Mock<IPackageReference> reference,
                Mock<IPackageObject> package)
            {
                packageContainer.Setup(c => c.FindPackage(It.IsAny<string>(), It.IsAny<IPackageReference>()))
                    .Returns(package.Object);
                package.Setup(p => p.GetContentFiles()).Returns(new List<string> {"TestMain.csx"});
                preProcessor.Setup(p => p.ProcessFile(It.IsAny<string>())).Returns(new FilePreProcessorResult());
                composer.ProcessPackage(@"c:\packages", reference.Object, new StringBuilder(), new List<string>(), new List<string>());
                preProcessor.Verify(p=> p.ProcessFile(It.IsAny<string>()));                
            }

            [Theory, ScriptCsAutoData]
            public void ShouldWarnIfMultipleMainFilesArePresent(
                [Frozen] Mock<IPackageContainer> packageContainer,
                [Frozen] Mock<IFilePreProcessor> preProcessor,
                [Frozen] TestLogProvider logProvider,
                ScriptLibraryComposer composer,
                Mock<IPackageReference> reference,
                Mock<IPackageObject> package)
            {
                packageContainer.Setup(c => c.FindPackage(It.IsAny<string>(), It.IsAny<IPackageReference>()))
                    .Returns(package.Object);
                package.SetupGet(p => p.FullName).Returns("Test");
                package.Setup(p => p.GetContentFiles()).Returns(new List<string> { "Test1Main.csx", "Test2Main.csx" });
                preProcessor.Setup(p => p.ProcessFile(It.IsAny<string>())).Returns(new FilePreProcessorResult());
                composer.ProcessPackage(@"c:\packages", reference.Object, new StringBuilder(), new List<string>(), new List<string>());
                logProvider.Output.ShouldContain(
                    "WARN: Script Libraries in 'Test' ignored due to multiple Main files being present");
            }



            [Theory, ScriptCsAutoData]
            public void ShouldAppendTheClassWrapper(
                [Frozen] Mock<IPackageContainer> packageContainer,
                [Frozen] Mock<IFilePreProcessor> preProcessor,
                ScriptLibraryComposer composer,
                Mock<IPackageReference> reference,
                Mock<IPackageObject> package)
            {
                packageContainer.Setup(c => c.FindPackage(It.IsAny<string>(), It.IsAny<IPackageReference>()))
                    .Returns(package.Object);
                package.Setup(p => p.GetContentFiles()).Returns(new List<string> {"TestMain.csx"});
                preProcessor.Setup(p => p.ProcessFile(It.IsAny<string>())).Returns(new FilePreProcessorResult());
                var builder = new StringBuilder();
                composer.ProcessPackage(@"c:\packages", reference.Object, builder, new List<string>(), new List<string>());
                builder.ToString().ShouldContain("public class Test : ScriptCs.ScriptLibraryWrapper {");
            }

            [Theory, ScriptCsAutoData]
            public void ShouldAppendThePreProcessedCode(
                [Frozen] Mock<IPackageContainer> packageContainer,
                [Frozen] Mock<IFilePreProcessor> preProcessor,
                ScriptLibraryComposer composer,
                Mock<IPackageReference> reference,
                Mock<IPackageObject> package)
            {
                packageContainer.Setup(c => c.FindPackage(It.IsAny<string>(), It.IsAny<IPackageReference>()))
                    .Returns(package.Object);
                package.Setup(p => p.GetContentFiles()).Returns(new List<string> { "TestMain.csx" });
                preProcessor.Setup(p => p.ProcessFile(It.IsAny<string>())).Returns(new FilePreProcessorResult {Code="TestCode"});
                var builder = new StringBuilder();
                composer.ProcessPackage(@"c:\packages", reference.Object, builder, new List<string>(), new List<string>());
                builder.ToString().EndsWith("TestCode}");
            }

            [Theory, ScriptCsAutoData]
            public void ShouldAddsReferences(
                [Frozen] Mock<IPackageContainer> packageContainer,
                [Frozen] Mock<IFilePreProcessor> preProcessor,
                ScriptLibraryComposer composer,
                Mock<IPackageReference> reference,
                Mock<IPackageObject> package)
            {
                packageContainer.Setup(c => c.FindPackage(It.IsAny<string>(), It.IsAny<IPackageReference>()))
                    .Returns(package.Object);
                package.Setup(p => p.GetContentFiles()).Returns(new List<string> { "TestMain.csx" });
                preProcessor.Setup(p => p.ProcessFile(It.IsAny<string>())).Returns(new FilePreProcessorResult { Code = "TestCode", References = new List<string> {"ref1", "ref2"}});
                var refs = new List<string>();
                composer.ProcessPackage(@"c:\packages", reference.Object, new StringBuilder(), refs, new List<string>());
                refs.ShouldContain("ref1");
                refs.ShouldContain("ref2");
            }

            [Theory, ScriptCsAutoData]
            public void ShouldAddNamespaces(
                [Frozen] Mock<IPackageContainer> packageContainer,
                [Frozen] Mock<IFilePreProcessor> preProcessor,
                ScriptLibraryComposer composer,
                Mock<IPackageReference> reference,
                Mock<IPackageObject> package)
            {
                packageContainer.Setup(c => c.FindPackage(It.IsAny<string>(), It.IsAny<IPackageReference>()))
                    .Returns(package.Object);
                package.Setup(p => p.GetContentFiles()).Returns(new List<string> { "TestMain.csx" });
                preProcessor.Setup(p => p.ProcessFile(It.IsAny<string>())).Returns(new FilePreProcessorResult { Code = "TestCode", Namespaces = new List<string> { "ns1", "ns2" } });
                var ns = new List<string>();
                composer.ProcessPackage(@"c:\packages", reference.Object, new StringBuilder(), new List<string>(), ns);
                ns.ShouldContain("ns1");
                ns.ShouldContain("ns2");
            }
        }

        public class TheComposeMethod
        {
            [Theory, ScriptCsAutoData]
            public void ShouldProcessEachPackage(
                [Frozen] Mock<IPackageAssemblyResolver> resolver,
                Mock<ScriptLibraryComposer> composer,
                Mock<IPackageReference> reference1,
                Mock<IPackageReference> reference2)
            {
                composer.Protected();
                resolver.Setup(r=>r.GetPackages(It.IsAny<string>())).Returns(new List<IPackageReference> { reference1.Object, reference2.Object });
                composer.Setup(c=>c.ProcessPackage(It.IsAny<string>(), It.IsAny<IPackageReference>(), It.IsAny<StringBuilder>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()));
                composer.Object.Compose("workingdirectory");
                composer.Verify(c=>c.ProcessPackage(It.IsAny<string>(),reference1.Object, It.IsAny<StringBuilder>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()));
            }

            [Theory, ScriptCsAutoData]
            public void ShouldAddEachNamespace(
                [Frozen] Mock<IPackageAssemblyResolver> resolver,
                Mock<ScriptLibraryComposer> composer,
                Mock<IPackageReference> reference1
            )
            {
                composer.Protected();
                var builder = new StringBuilder();
                composer.Setup(
                    c =>
                        c.ProcessPackage(It.IsAny<string>(), It.IsAny<IPackageReference>(), It.IsAny<StringBuilder>(),
                            It.IsAny<List<string>>(), It.IsAny<List<string>>()))
                    .Callback((string p, IPackageReference r, StringBuilder b, List<string> refs, List<string> ns) =>
                    {
                        ns.Add("ns1");
                        ns.Add("ns2");
                    });
                resolver.Setup(r=>r.GetPackages(It.IsAny<string>())).Returns(new List<IPackageReference> { reference1.Object });
                composer.Object.Compose("workingdirectory", builder);
                var code = builder.ToString();
                code.ShouldContain(string.Format("using ns1;{0}", Environment.NewLine));
                code.ShouldContain(string.Format("using ns2;{0}", Environment.NewLine));
            }

            [Theory, ScriptCsAutoData]
            public void ShouldAddEachReference(
                [Frozen] Mock<IPackageAssemblyResolver> resolver,
                Mock<ScriptLibraryComposer> composer,
                Mock<IPackageReference> reference1
            )
            {
                composer.Protected();
                var builder = new StringBuilder();
                composer.Setup(
                    c =>
                        c.ProcessPackage(It.IsAny<string>(), It.IsAny<IPackageReference>(), It.IsAny<StringBuilder>(),
                            It.IsAny<List<string>>(), It.IsAny<List<string>>()))
                    .Callback((string p, IPackageReference r, StringBuilder b, List<string> refs, List<string> ns) =>
                    {
                        refs.Add("ref1");
                        refs.Add("ref2");
                    });
                resolver.Setup(r => r.GetPackages(It.IsAny<string>())).Returns(new List<IPackageReference> { reference1.Object });
                composer.Object.Compose("workingdirectory", builder);
                var code = builder.ToString();
                code.ShouldContain(string.Format("#r ref1{0}", Environment.NewLine));
                code.ShouldContain(string.Format("#r ref2{0}", Environment.NewLine));
            }

            [Theory, ScriptCsAutoData]
            public void ShouldWriteTheScriptLibrariesFile(
                [Frozen] Mock<IFileSystem> fileSystem,
                [Frozen] Mock<IPackageAssemblyResolver> resolver,
                ScriptLibraryComposer composer
            )
            {
                var builder = new StringBuilder("Test");
                fileSystem.Setup(fs => fs.WriteToFile(It.IsAny<string>(), It.IsAny<string>()));
                resolver.Setup(r => r.GetPackages(It.IsAny<string>())).Returns(Enumerable.Empty<IPackageReference>());
                composer.Compose("workingdirectory", builder);
                fileSystem.Verify(fs => fs.WriteToFile(It.IsAny<string>(), It.Is<string>(v=>v.Equals("Test"))));   
            }
        }
    }
}
