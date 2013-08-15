using ScriptCs.Argument;
using Should;
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

                sr.CommandArguments.ShouldEqual(new string[0]);
                sr.ScriptArguments.ShouldEqual(new string[0]);
            }

            [Fact]
            public void ShouldHandleMissingDoubledash()
            {
                var args = new string[] { "scriptname.csx", "-restore" };

                var sr = ArgumentHandler.SplitScriptArgs(args);

                sr.CommandArguments.ShouldEqual(new[] { "scriptname.csx", "-restore" });
                sr.ScriptArguments.ShouldEqual(new string[0]);
            }

            [Fact]
            public void ShouldHandleArgsAndScriptArgs()
            {
                var args = new string[] { "scriptname.csx", "-restore", "--", "-port", "8080" };

                var sr = ArgumentHandler.SplitScriptArgs(args);

                sr.CommandArguments.ShouldEqual(new[] { "scriptname.csx", "-restore" });
                sr.ScriptArguments.ShouldEqual(new[] { "-port", "8080" });
            }

            [Fact]
            public void ShouldHandleJustScriptArgs()
            {
                var args = new string[] { "--", "-port", "8080" };

                var sr = ArgumentHandler.SplitScriptArgs(args);

                sr.CommandArguments.ShouldEqual(new string[0]);
                sr.ScriptArguments.ShouldEqual(new[] { "-port", "8080" });
            }

            [Fact]
            public void ShouldHandleJustDoubledash()
            {
                var args = new string[] { "--" };

                var sr = ArgumentHandler.SplitScriptArgs(args);

                sr.CommandArguments.ShouldEqual(new string[0]);
                sr.ScriptArguments.ShouldEqual(new string[0]);
            }

            [Fact]
            public void ShouldHandleExtraDoubledash()
            {
                var args = new string[] { "scriptname.csx", "-restore", "--", "-port", "--", "8080" };

                var sr = ArgumentHandler.SplitScriptArgs(args);

                sr.CommandArguments.ShouldEqual(new[] { "scriptname.csx", "-restore" });
                sr.ScriptArguments.ShouldEqual(new[] { "-port", "--", "8080" });
            }

            [Fact]
            public void ShouldHandleTrailingDoubledash()
            {
                var args = new string[] { "scriptname.csx", "-restore", "--" };

                var sr = ArgumentHandler.SplitScriptArgs(args);

                sr.CommandArguments.ShouldEqual(new[] { "scriptname.csx", "-restore" });
                sr.ScriptArguments.ShouldEqual(new string[0]);
            }
        }
    }
}