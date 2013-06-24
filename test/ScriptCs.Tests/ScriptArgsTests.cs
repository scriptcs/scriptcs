using System;
using Common.Logging;
using Moq;
using ScriptCs.Argument;
using ScriptCs.Command;
using ScriptCs.Package;
using Xunit;

namespace ScriptCs.Tests
{
    public class ScriptArgsTests
    {
        public class SplitScriptArgsMethod
        {
            [Fact]
            public void ShouldHandleEmptyArgs()
            {
                var args = new string[0];

                var sr = ArgumentHandler.SplitScriptArgs(args);

                Assert.Equal(new string[0], sr.CommandArguments);
                Assert.Equal(new string[0], sr.ScriptArguments);
            }

            [Fact]
            public void ShouldHandleMissingDoubledash()
            {
                var args = new string[] { "scriptname.csx", "-restore" };

                var sr = ArgumentHandler.SplitScriptArgs(args);

                Assert.Equal(new string[] { "scriptname.csx", "-restore" }, sr.CommandArguments);
                Assert.Equal(new string[0], sr.ScriptArguments);
            }

            [Fact]
            public void ShouldHandleArgsAndScriptArgs()
            {
                var args = new string[] { "scriptname.csx", "-restore", "--", "-port", "8080" };

                var sr = ArgumentHandler.SplitScriptArgs(args);

                Assert.Equal(new string[] { "scriptname.csx", "-restore" }, sr.CommandArguments);
                Assert.Equal(new string[] { "-port", "8080" }, sr.ScriptArguments);
            }

            [Fact]
            public void ShouldHandleJustScriptArgs()
            {
                var args = new string[] { "--", "-port", "8080" };

                var sr = ArgumentHandler.SplitScriptArgs(args);

                Assert.Equal(new string[0], sr.CommandArguments);
                Assert.Equal(new string[] { "-port", "8080" }, sr.ScriptArguments);
            }

            [Fact]
            public void ShouldHandleJustDoubledash()
            {
                var args = new string[] { "--" };

                var sr = ArgumentHandler.SplitScriptArgs(args);

                Assert.Equal(new string[0], sr.CommandArguments);
                Assert.Equal(new string[0], sr.ScriptArguments);
            }

            [Fact]
            public void ShouldHandleExtraDoubledash()
            {
                var args = new string[] { "scriptname.csx", "-restore", "--", "-port", "--", "8080" };

                var sr = ArgumentHandler.SplitScriptArgs(args);

                Assert.Equal(new string[] { "scriptname.csx", "-restore" }, sr.CommandArguments);
                Assert.Equal(new string[] { "-port", "--", "8080" }, sr.ScriptArguments);
            }

            [Fact]
            public void ShouldHandleTrailingDoubledash()
            {
                var args = new string[] { "scriptname.csx", "-restore", "--" };

                var sr = ArgumentHandler.SplitScriptArgs(args);

                Assert.Equal(new string[] { "scriptname.csx", "-restore" }, sr.CommandArguments);
                Assert.Equal(new string[0], sr.ScriptArguments);
            }
        }
    }
}