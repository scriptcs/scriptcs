using System;
using Common.Logging;
using Moq;
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
                string[] args = new string[0];
                string[] scriptArgs;

                ScriptCsArgs.SplitScriptArgs(ref args, out scriptArgs);

                Assert.Equal(new string[0], args);
                Assert.Equal(new string[0], scriptArgs);
            }

            [Fact]
            public void ShouldHandleMissingDoubledash()
            {
                string[] args = new string[] { "scriptname.csx", "-restore" };
                string[] scriptArgs;

                ScriptCsArgs.SplitScriptArgs(ref args, out scriptArgs);

                Assert.Equal(new string[] { "scriptname.csx", "-restore" }, args);
                Assert.Equal(new string[0], scriptArgs);
            }

            [Fact]
            public void ShouldHandleArgsAndScriptArgs()
            {
                string[] args = new string[] { "scriptname.csx", "-restore", "--", "-port", "8080" };
                string[] scriptArgs;

                ScriptCsArgs.SplitScriptArgs(ref args, out scriptArgs);

                Assert.Equal(new string[] { "scriptname.csx", "-restore" }, args);
                Assert.Equal(new string[] { "-port", "8080" }, scriptArgs);
            }

            [Fact]
            public void ShouldHandleJustScriptArgs()
            {
                string[] args = new string[] { "--", "-port", "8080" };
                string[] scriptArgs;

                ScriptCsArgs.SplitScriptArgs(ref args, out scriptArgs);

                Assert.Equal(new string[0], args);
                Assert.Equal(new string[] { "-port", "8080" }, scriptArgs);
            }

            [Fact]
            public void ShouldHandleJustDoubledash()
            {
                string[] args = new string[] { "--" };
                string[] scriptArgs;

                ScriptCsArgs.SplitScriptArgs(ref args, out scriptArgs);

                Assert.Equal(new string[0], args);
                Assert.Equal(new string[0], scriptArgs);
            }

            [Fact]
            public void ShouldHandleExtraDoubledash()
            {
                string[] args = new string[] { "scriptname.csx", "-restore", "--", "-port", "--", "8080" };
                string[] scriptArgs;

                ScriptCsArgs.SplitScriptArgs(ref args, out scriptArgs);

                Assert.Equal(new string[] { "scriptname.csx", "-restore" }, args);
                Assert.Equal(new string[] { "-port", "--", "8080" }, scriptArgs);
            }

            [Fact]
            public void ShouldHandleTrailingDoubledash()
            {
                string[] args = new string[] { "scriptname.csx", "-restore", "--" };
                string[] scriptArgs;

                ScriptCsArgs.SplitScriptArgs(ref args, out scriptArgs);

                Assert.Equal(new string[] { "scriptname.csx", "-restore" }, args);
                Assert.Equal(new string[0], scriptArgs);
            }

            public class ScriptArgsPlumbing {
                [Fact]
                public void ScriptArgsArePlumbedThrough()
                {

                }
            }
        }
    }
}