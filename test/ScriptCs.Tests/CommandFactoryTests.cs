using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Registration;
using ScriptCs.Command;
using ScriptCs.Engine.Roslyn;
using ScriptCs.Package;
using ScriptCs.Package.InstallationProvider;
using Should;
using Xunit;

namespace ScriptCs.Tests
{
    public class CommandFactoryTests
    {
        public class CreateCommandMethod
        {
            private static CompositionContainer CreateCompositionContainer()
            {
                var conventions = new RegistrationBuilder();
                conventions.ForTypesDerivedFrom<IScriptHostFactory>().Export<IScriptHostFactory>();
                conventions.ForTypesDerivedFrom<IFileSystem>().Export<IFileSystem>();
                conventions.ForTypesDerivedFrom<IPackageAssemblyResolver>().Export<IPackageAssemblyResolver>();
                conventions.ForTypesDerivedFrom<IPackageContainer>().Export<IPackageContainer>();
                conventions.ForTypesDerivedFrom<IFilePreProcessor>().Export<IFilePreProcessor>();
                conventions.ForTypesDerivedFrom<IPackageInstaller>().Export<IPackageInstaller>();
                conventions.ForTypesDerivedFrom<IInstallationProvider>().Export<IInstallationProvider>();
                conventions.ForType<ScriptExecutor>().Export<IScriptExecutor>();
                conventions.ForType<RoslynScriptEngine>().Export<IScriptEngine>();

                var catalog = new AggregateCatalog();
                catalog.Catalogs.Add(new AssemblyCatalog(typeof(ScriptExecutor).Assembly, conventions));
                catalog.Catalogs.Add(new AssemblyCatalog(typeof(RoslynScriptEngine).Assembly, conventions));

                var container = new CompositionContainer(catalog, CompositionOptions.DisableSilentRejection);

                return container;
            }

            [Fact]
            public void ShouldInstallWhenInstallFlagIsOn()
            {
                var args = new ScriptCsArgs
                {
                    AllowPreReleaseFlag = false,
                    Install = "",
                    ScriptName = null
                };

                var factory = new CommandFactory(CreateCompositionContainer());
                var result = factory.CreateCommand(args);

                result.ShouldImplement<IInstallCommand>();
            }

            [Fact]
            public void ShouldExecuteWhenScriptNameIsPassed()
            {
                var args = new ScriptCsArgs
                {
                    AllowPreReleaseFlag = false,
                    Install = null,
                    ScriptName = "test.csx"
                };

                var factory = new CommandFactory(CreateCompositionContainer());
                var result = factory.CreateCommand(args);

                result.ShouldImplement<IScriptCommand>();
            }

            [Fact]
            public void ShouldExecuteWhenBothNameAndInstallArePassed()
            {
                var args = new ScriptCsArgs
                {
                    AllowPreReleaseFlag = false,
                    Install = "",
                    ScriptName = "test.csx"
                };

                var factory = new CommandFactory(CreateCompositionContainer());
                var result = factory.CreateCommand(args);

                result.ShouldImplement<IScriptCommand>();
            }

            [Fact]
            public void ShouldReturnInvalidWhenNoNameOrInstallSet()
            {
                var args = new ScriptCsArgs
                {
                    AllowPreReleaseFlag = false,
                    Install = null,
                    ScriptName = null
                };

                var factory = new CommandFactory(CreateCompositionContainer());
                var result = factory.CreateCommand(args);

                result.ShouldImplement<IInvalidCommand>();
            }


        }
    }
}
