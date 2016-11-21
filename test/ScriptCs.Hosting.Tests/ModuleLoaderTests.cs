using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using Moq;
using ScriptCs.Contracts;
using Should;
using Xunit;

namespace ScriptCs.Hosting.Tests
{
    using ScriptCs.Tests;

    public class ModuleLoaderTests
    {
        public class TheLoadMethod
        {
            private Mock<IAssemblyResolver> _mockAssemblyResolver = new Mock<IAssemblyResolver>();
            private IList<Lazy<IModule, IModuleMetadata>> _modules = new List<Lazy<IModule, IModuleMetadata>>();
            private Func<CompositionContainer, IEnumerable<Lazy<IModule, IModuleMetadata>>> _getModules;
            private TestLogProvider _logProvider = new TestLogProvider();
            private Mock<IModule> _mockModule1 = new Mock<IModule>();
            private Mock<IModule> _mockModule2 = new Mock<IModule>();
            private Mock<IModule> _mockModule3 = new Mock<IModule>();
            private Mock<IModule> _mockModule4 = new Mock<IModule>();
            private Mock<IFileSystem> _mockFileSystem = new Mock<IFileSystem>();
            private Mock<IAssemblyUtility> _mockAssemblyUtility = new Mock<IAssemblyUtility>();

            public TheLoadMethod()
            {
                var paths = new[] { "path1.dll", "path2.dll" };
                _mockAssemblyResolver.Setup(r => r.GetAssemblyPaths(It.IsAny<string>(), true)).Returns(paths);
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
                _mockAssemblyUtility.Setup(a => a.IsManagedAssembly(It.IsAny<string>())).Returns(true);
            }

            [Fact]
            public void ShouldResolvePathsFromTheAssemblyResolver()
            {
                var loader = new ModuleLoader(_mockAssemblyResolver.Object, _logProvider, (p, c) => { }, c => Enumerable.Empty<Lazy<IModule, IModuleMetadata>>(), _mockFileSystem.Object, _mockAssemblyUtility.Object);
                loader.Load(null, new[] { "c:\test" }, null, null);
                _mockAssemblyResolver.Verify(r => r.GetAssemblyPaths("c:\test", true));
            }

            [Fact]
            public void ShouldInvokeTheCatalogActionForEachFile()
            {
                var assemblies = new List<Assembly>();
                var loader = new ModuleLoader(_mockAssemblyResolver.Object, _logProvider, (a, c) => assemblies.Add(a), c => Enumerable.Empty<Lazy<IModule, IModuleMetadata>>(), _mockFileSystem.Object, _mockAssemblyUtility.Object);
                loader.Load(null, new[] { "c:\test" }, null, null);
                assemblies.Count.ShouldEqual(2);
            }

            [Fact]
            public void ShouldIgnoreLoadingNativeAssemblies()
            {
                _mockAssemblyUtility.Setup(a => a.IsManagedAssembly(It.Is<string>(f => f == "managed.dll"))).Returns(true);
                _mockAssemblyUtility.Setup(a => a.IsManagedAssembly(It.Is<string>(f => f == "native.dll"))).Returns(false);
                var mockAssemblies = new List<string> {"managed.dll", "native.dll"};
                _mockAssemblyResolver.Setup(a => a.GetAssemblyPaths(It.IsAny<string>(), true)).Returns(mockAssemblies);
                var assemblies = new List<Assembly>();
                var loader = new ModuleLoader(_mockAssemblyResolver.Object, _logProvider, (a, c) => assemblies.Add(a), c => Enumerable.Empty<Lazy<IModule, IModuleMetadata>>(), _mockFileSystem.Object, _mockAssemblyUtility.Object);
                loader.Load(null, new[] { "c:\test" }, null, null);
                assemblies.Count.ShouldEqual(1);
            }

            [Fact]
            public void ShouldInitializeModulesThatMatchOnExtension()
            {
                var loader = new ModuleLoader(_mockAssemblyResolver.Object, _logProvider, (a, c) => { }, _getModules, _mockFileSystem.Object, _mockAssemblyUtility.Object);
                loader.Load(null, new string[0], null, "ext1");
                _mockModule1.Verify(m => m.Initialize(It.IsAny<IModuleConfiguration>()), Times.Once());
                _mockModule2.Verify(m => m.Initialize(It.IsAny<IModuleConfiguration>()), Times.Never());
                _mockModule3.Verify(m => m.Initialize(It.IsAny<IModuleConfiguration>()), Times.Never());
            }

            [Fact]
            public void ShouldInitializeModulesThatMatchBasedOnName()
            {
                var loader = new ModuleLoader(_mockAssemblyResolver.Object, _logProvider, (a, c) => { }, _getModules, _mockFileSystem.Object, _mockAssemblyUtility.Object);
                loader.Load(null, new string[0], null, null, "module3");
                _mockModule1.Verify(m => m.Initialize(It.IsAny<IModuleConfiguration>()), Times.Never());
                _mockModule2.Verify(m => m.Initialize(It.IsAny<IModuleConfiguration>()), Times.Never());
                _mockModule3.Verify(m => m.Initialize(It.IsAny<IModuleConfiguration>()), Times.Once());
            }

            [Fact]
            public void ShouldInitializeModulesThatAreSetToAutoload()
            {
                var loader = new ModuleLoader(_mockAssemblyResolver.Object, _logProvider, (a, c) => { }, _getModules, _mockFileSystem.Object, _mockAssemblyUtility.Object);
                loader.Load(null, new string[0], null, null);
                _mockModule4.Verify(m => m.Initialize(It.IsAny<IModuleConfiguration>()), Times.Once());
            }

            [Fact]
            public void ShouldNotInitializeModulesThatAreNotSetToAutoload()
            {
                var loader = new ModuleLoader(_mockAssemblyResolver.Object, _logProvider, (a, c) => { }, _getModules, _mockFileSystem.Object, _mockAssemblyUtility.Object);
                loader.Load(null, new string[0], null, null);
                _mockModule1.Verify(m => m.Initialize(It.IsAny<IModuleConfiguration>()), Times.Never());
            }

            [Fact]
            public void ShouldLoadEngineAssemblyByHandIfItsTheOnlyModule()
            {
                var path = Path.Combine("c:\\foo", ModuleLoader.DefaultCSharpModules["roslyn"]);
                _mockAssemblyUtility.Setup(x => x.LoadFile(path));
                var loader = new ModuleLoader(_mockAssemblyResolver.Object, _logProvider, (a, c) => { }, _getModules, _mockFileSystem.Object, _mockAssemblyUtility.Object);
                loader.Load(null, new string[0], "c:\\foo", ModuleLoader.DefaultCSharpExtension, "roslyn");

                _mockAssemblyUtility.Verify(x => x.LoadFile(path), Times.Once());
            }

            [Fact]
            public void ShouldLoadEngineModuleFromFile()
            {
                _mockAssemblyUtility.Setup(x => x.LoadFile(It.IsAny<string>())).Returns(typeof (DummyModule).Assembly);
                var loader = new ModuleLoader(_mockAssemblyResolver.Object, _logProvider, (a, c) => { }, _getModules, _mockFileSystem.Object, _mockAssemblyUtility.Object);

                var config = new ModuleConfiguration(true, string.Empty, false, LogLevel.Debug, true,
                    new Dictionary<Type, object> {{typeof (string), "not loaded"}});
                loader.Load(config, new string[0], "c:\\foo", ModuleLoader.DefaultCSharpExtension, "roslyn");

                config.Overrides[typeof(string)].ShouldEqual("module loaded");
            }

            [Fact]
            public void ShouldNotLoadEngineAssemblyByHandIfItsTheOnlyModuleButExtensionIsNotDefault()
            {
                var path = Path.Combine("c:\\foo", ModuleLoader.DefaultCSharpModules["roslyn"]);
                _mockAssemblyUtility.Setup(x => x.LoadFile(path));
                var loader = new ModuleLoader(_mockAssemblyResolver.Object, _logProvider, (a, c) => { }, _getModules, _mockFileSystem.Object, _mockAssemblyUtility.Object);

                loader.Load(null, new string[0], "c:\\foo", ".fsx", "roslyn");
                _mockAssemblyUtility.Verify(x => x.LoadFile(It.IsAny<string>()), Times.Never);
            }

            public class ModuleMetadata : IModuleMetadata
            {
                public string Name { get; set; }

                public string Extensions { get; set; }

                public bool Autoload { get; set; }
            }

            public class DummyModule : IModule
            {
                public void Initialize(IModuleConfiguration config)
                {
                    config.Overrides[typeof (string)] = "module loaded";
                }
            }
        }
    }
}
