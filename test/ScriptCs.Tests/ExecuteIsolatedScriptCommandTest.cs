using System;
using System.Runtime.Serialization;
using Moq;
using ScriptCs.Command;
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
            
            public DataHolder DataHolder { get; set; }

            public void Execute()
            {
                DataHolder.AppDomainId = AppDomain.CurrentDomain.Id;
            }
        }
    }
}