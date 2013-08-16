using System;
using System.Linq.Expressions;
using Common.Logging;
using Moq;
using Moq.Language.Flow;
using ScriptCs.Command;
using ScriptCs.Contracts;
using ScriptCs.Package;
using Xunit;

namespace ScriptCs.Tests
{
    public class ExecuteIsolatedScriptCommandTests
    {
        public class ExecuteMethod
        {
            [Fact]
            public void ShouldExecuteInAnotherAppDomain()
            {
                var args = new ScriptCsArgs { ScriptName = "test.csx", Isolated = true };
                var scriptArgs = new string[0];
                
                var commandInfo = CommandInfo.Create(root => root.FileSystem.SetupGet(x => x.CurrentDirectory).Returns("C:\\"), args, scriptArgs);
                var command = (IIsolatedScriptCommand) commandInfo.Command;
                var helper = new IsolatedHelper { DataHolder = new DataHolder() };
                var oldHelper = command.IsolatedHelper;
                command.IsolatedHelper = helper;
                
                command.Execute();

                Assert.Equal(0, command.IsolatedHelper.AssemblyPaths.Length);
                Assert.Equal(args, oldHelper.CommandArgs);
                Assert.Equal(args.ScriptName, oldHelper.Script);
                Assert.Equal(scriptArgs, oldHelper.ScriptArgs);
                Assert.True(helper.DataHolder.AppDomainId != AppDomain.CurrentDomain.Id, "Execute should be called in another appdomain");
            }
        }

        public class DataHolder : MarshalByRefObject
        {
            public int AppDomainId { get; set; }
        }

        [Serializable]
        public class IsolatedHelper : IIsolatedHelper
        {
            public ScriptCsArgs CommandArgs { get; set; }
            public string[] AssemblyPaths { get; set; }
            public string Script { get; set; }
            public string[] ScriptArgs { get; set; }
            public ScriptResult Result { get; set; }
            
            public DataHolder DataHolder { get; set; }

            public void Execute()
            {
                DataHolder.AppDomainId = AppDomain.CurrentDomain.Id;
            }
        }

        public class CommandInfo
        {
            public ScriptServices Root { get; set; }
            public ICommand Command { get; set; }

            public CommandResult Execute()
            {
                return Command.Execute();
            }

            public static CommandInfo Create(Action<ScriptServices> setup)
            {
                return Create(setup, new ScriptCsArgs { ScriptName = "test.csx" }, new string[0]);
            }

            public static CommandInfo Create(Action<ScriptServices> setup, ScriptCsArgs args, string[] scriptArgs)
            {
                var fs = new Mock<IFileSystem>();
                var resolver = new Mock<IPackageAssemblyResolver>();
                var executor = new Mock<IScriptExecutor>();
                var engine = new Mock<IScriptEngine>();
                var scriptpackResolver = new Mock<IScriptPackResolver>();
                var packageInstaller = new Mock<IPackageInstaller>();
                var logger = new Mock<ILog>();
                var filePreProcessor = new Mock<IFilePreProcessor>();
                var assemblyName = new Mock<IAssemblyResolver>();
                var installationProvider = new Mock<IInstallationProvider>();
                var console = new Mock<IConsole>();
                var root = new ScriptServices(fs.Object, resolver.Object, executor.Object, engine.Object, filePreProcessor.Object, scriptpackResolver.Object, packageInstaller.Object, logger.Object, assemblyName.Object, console.Object, installationProvider.Object);

                setup(root);

                var factory = new CommandFactory(root);
                return new CommandInfo { Command = factory.CreateCommand(args, scriptArgs), Root = root };
            }
        }
    }

    public static class MockExtesions
    {
        public static ISetupGetter<TMocked, TProperty> SetupGet<TMocked, TProperty>(this TMocked obj, Expression<Func<TMocked, TProperty>> expression) where TMocked : class
        {
            return Mock.Get(obj).SetupGet(expression);
        }

        public static ISetup<TMocked, TResult> Setup<TMocked, TResult>(this TMocked obj, Expression<Func<TMocked, TResult>> expression) where TMocked : class
        {
            return Mock.Get(obj).Setup(expression);
        }

        public static void Verify<TMocked>(this TMocked obj, Expression<Action<TMocked>> expression, Times times) where TMocked : class
        {
            Mock.Get(obj).Verify(expression, times);
        }
    }
}