using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using Common.Logging;
using Moq;
using ScriptCs.Contracts;
using Should;
using Xunit;

namespace ScriptCs.Hosting.Tests
{
    public class ModuleLoaderTests
    {
        public class TheLoadMethod
        {
            private Mock<IAssemblyResolver> _mockAssemblyResolver = new Mock<IAssemblyResolver>();
            private IList<string> _paths = new List<string>();
            private IList<Lazy<IModule, IModuleMetadata>> _modules = new List<Lazy<IModule, IModuleMetadata>>();
            private Func<CompositionContainer, IEnumerable<Lazy<IModule, IModuleMetadata>>> _getModules;
            private Mock<ILog> _mockLogger = new Mock<ILog>();
            private Mock<IModule> _mockModule1 = new Mock<IModule>();
            private Mock<IModule> _mockModule2 = new Mock<IModule>();
            private Mock<IModule> _mockModule3 = new Mock<IModule>();
            private Mock<IModule> _mockModule4 = new Mock<IModule>();
            private Mock<IFileSystem> _mockFileSystem = new Mock<IFileSystem>();

            public TheLoadMethod()
            {
                var paths = new[] { "path1", "path2" };
                _mockAssemblyResolver.Setup(r => r.GetAssemblyPaths(It.IsAny<string>())).Returns(paths);
                _modules.Add(
                    new Lazy<IModule, IModuleMetadata>(
                        () => _mockModule1.Object, new ModuleMetadata { Extensions = "ext1,ext2", Name = "module1" }));
                _modules.Add(
                    new Lazy<IModule, IModuleMetadata>(
                        () => _mockModule2.Object, new ModuleMetadata { Extensions = "ext3,ext4", Name = "module2" }));
                _modules.Add(new Lazy<IModule, IModuleMetadata>(() => _mockModule3.Object, new ModuleMetadata { Name = "module3" }));
                _modules.Add(new Lazy<IModule, IModuleMetadata>(() => _mockModule4.Object, new ModuleMetadata { Name = "module4", Autoload = true }));
                _getModules = c => _modules;
                _mockFileSystem.Setup(f=>f.EnumerateFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>())).Returns(Enumerable.Empty<string>());
            }

            [Fact]
            public void ShouldResolvePathsFromTheAssemblyResolver()
            {
                var loader = new ModuleLoader(_mockAssemblyResolver.Object, _mockLogger.Object, (p, c) => { }, c => Enumerable.Empty<Lazy<IModule, IModuleMetadata>>(), _mockFileSystem.Object);
                loader.Load(null, new[] { "c:\test" }, null, null);
                _mockAssemblyResolver.Verify(r => r.GetAssemblyPaths("c:\test"));
            }

            [Fact]
            public void ShouldInvokeTheCatalogActionForEachFile()
            {
                var loader = new ModuleLoader(_mockAssemblyResolver.Object, _mockLogger.Object, (p, c) => _paths.Add(p), c => Enumerable.Empty<Lazy<IModule, IModuleMetadata>>(), _mockFileSystem.Object);
                loader.Load(null, new[] { "c:\test" }, null, null);
                _paths.Count.ShouldEqual(2);
            }

            [Fact]
            public void ShouldInitializeModulesThatMatchOnExtension()
            {
                var loader = new ModuleLoader(_mockAssemblyResolver.Object, _mockLogger.Object, (p, c) => _paths.Add(p), _getModules, _mockFileSystem.Object);
                loader.Load(null, new string[0], null, "ext1");
                _mockModule1.Verify(m => m.Initialize(It.IsAny<IModuleConfiguration>()), Times.Once());
                _mockModule2.Verify(m => m.Initialize(It.IsAny<IModuleConfiguration>()), Times.Never());
                _mockModule3.Verify(m => m.Initialize(It.IsAny<IModuleConfiguration>()), Times.Never());
            }

            [Fact]
            public void ShouldInitializeModulesThatMatchBasedOnName()
            {
                var loader = new ModuleLoader(_mockAssemblyResolver.Object, _mockLogger.Object, (p, c) => _paths.Add(p), _getModules, _mockFileSystem.Object);
                loader.Load(null, new string[0], null, null, "module3");
                _mockModule1.Verify(m => m.Initialize(It.IsAny<IModuleConfiguration>()), Times.Never());
                _mockModule2.Verify(m => m.Initialize(It.IsAny<IModuleConfiguration>()), Times.Never());
                _mockModule3.Verify(m => m.Initialize(It.IsAny<IModuleConfiguration>()), Times.Once());
            }

            [Fact]
            public void ShouldInitializeModulesThatAreSetToAutoload()
            {
                var loader = new ModuleLoader(_mockAssemblyResolver.Object, _mockLogger.Object, (p, c) => _paths.Add(p), _getModules, _mockFileSystem.Object);
                loader.Load(null, new string[0], null, null);
                _mockModule4.Verify(m => m.Initialize(It.IsAny<IModuleConfiguration>()), Times.Once());
            }

            [Fact]
            public void ShouldNotInitializeModulesThatAreNotSetToAutoload()
            {
                var loader = new ModuleLoader(_mockAssemblyResolver.Object, _mockLogger.Object, (p, c) => _paths.Add(p), _getModules, _mockFileSystem.Object);
                loader.Load(null, new string[0], null, null);
                _mockModule1.Verify(m => m.Initialize(It.IsAny<IModuleConfiguration>()), Times.Never());
            }

            public class ModuleMetadata : IModuleMetadata
            {
                public string Name { get; set; }

                public string Extensions { get; set; }

                public bool Autoload { get; set; }
            }
        }
    }
}
