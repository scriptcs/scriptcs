using System.Collections.Generic;
using System.IO;
using Common.Logging;
using Moq;
using ScriptCs.Command;
using ScriptCs.Contracts;
using ScriptCs.Package;
using Xunit;
using System;
using System.Linq;
using Moq.Language.Flow;
using System.Linq.Expressions;

namespace ScriptCs.Tests
{
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

    public class ExecuteScriptCommandTests
    {
        public class ExecuteMethod
        {
            public class CommandInfo
            {
                public ScriptServiceRoot Root { get; set; }
                public ICommand Command { get; set; }

                public CommandResult Execute()
                {
                    return Command.Execute();
                }
            }

            public CommandInfo CreateCommand(Action<ScriptServiceRoot> setup)
            {
                var args = new ScriptCsArgs
                {
                    AllowPreRelease = false,
                    Install = "",
                    ScriptName = "test.csx"
                };
                var fs = new Mock<IFileSystem>();
                var resolver = new Mock<IPackageAssemblyResolver>();
                var executor = new Mock<IScriptExecutor>();
                var engine = new Mock<IScriptEngine>();
                var scriptpackResolver = new Mock<IScriptPackResolver>();
                var packageInstaller = new Mock<IPackageInstaller>();
                var logger = new Mock<ILog>();
                var filePreProcessor = new Mock<IFilePreProcessor>();
                var assemblyName = new Mock<IAssemblyName>();
                var root = new ScriptServiceRoot(fs.Object, resolver.Object, executor.Object, engine.Object, filePreProcessor.Object, scriptpackResolver.Object, packageInstaller.Object, logger.Object, assemblyName.Object);

                setup(root);

                var factory = new CommandFactory(root);
                return new CommandInfo { Command = factory.CreateCommand(args, new string[0]), Root = root };
            }

            [Fact]
            public void ScriptExecCommandShouldInvokeWithScriptPassedFromArgs()
            {
                var commandInfo = CreateCommand(root => root.FileSystem.SetupGet(x => x.CurrentDirectory).Returns("C:\\"));

                commandInfo.Execute();

                commandInfo.Root.Executor.Verify(i => i.Execute(It.Is<string>(x => x == "test.csx"), It.IsAny<string[]>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<IScriptPack>>()), Times.Once());
            }

            [Fact]
            public void ShouldCreateMissingBinFolder()
            {
                const string WorkingDirectory = @"C:\";

                var binFolder = Path.Combine(WorkingDirectory, "bin");

                var commandInfo = CreateCommand(root =>
                {
                    var fs = root.FileSystem;
                    fs.Setup(x => x.GetWorkingDirectory(It.IsAny<string>())).Returns(WorkingDirectory);
                    fs.SetupGet(x => x.CurrentDirectory).Returns(WorkingDirectory);
                    fs.Setup(x => x.DirectoryExists(binFolder)).Returns(false);
                });
                
                commandInfo.Execute();

                commandInfo.Root.FileSystem.Verify(x => x.CreateDirectory(binFolder), Times.Once());
            }

            [Fact]
            public void NonManagedAssembliesAreExcluded()
            {
                const string nonManaged = "non-managed.dll";

                var commandInfo = CreateCommand(root =>
                {
                    var fs = root.FileSystem;
                    fs.SetupGet(x => x.CurrentDirectory).Returns("C:\\");
                    fs.Setup(x => x.EnumerateFiles(It.IsAny<string>(), It.IsAny<string>())).Returns(new[] {"managed.dll", nonManaged });
                    root.AssemblyName.Setup(x => x.GetAssemblyName(It.Is<string>(y => y == nonManaged))).Throws(new BadImageFormatException());
                });

                commandInfo.Execute();

                commandInfo.Root.Executor.Verify(i => i.Execute(It.IsAny<string>(), It.IsAny<string[]>(), It.Is<IEnumerable<string>>(x => !x.Contains(nonManaged)), It.IsAny<IEnumerable<IScriptPack>>()), Times.Once());
            }
        }
    }
}
