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
                string[] args = new string[0];
                string[] scriptArgs;

                ScriptCsArgs.SplitScriptArgs(ref args, out scriptArgs);

                args.ShouldEqual(new string[0]);
                scriptArgs.ShouldEqual(new string[0]);
            }

            [Fact]
            public void ShouldHandleMissingDoubledash()
            {
                string[] args = new[] { "scriptname.csx", "-restore" };
                string[] scriptArgs;

                ScriptCsArgs.SplitScriptArgs(ref args, out scriptArgs);

                args.ShouldEqual(new[] { "scriptname.csx", "-restore" });
                scriptArgs.ShouldEqual(new string[0]);
            }

            [Fact]
            public void ShouldHandleArgsAndScriptArgs()
            {
                string[] args = new[] { "scriptname.csx", "-restore", "--", "-port", "8080" };
                string[] scriptArgs;

                ScriptCsArgs.SplitScriptArgs(ref args, out scriptArgs);

                args.ShouldEqual(new[] { "scriptname.csx", "-restore" });
                scriptArgs.ShouldEqual(new[] { "-port", "8080" });
            }

            [Fact]
            public void ShouldHandleJustScriptArgs()
            {
                string[] args = new[] { "--", "-port", "8080" };
                string[] scriptArgs;

                ScriptCsArgs.SplitScriptArgs(ref args, out scriptArgs);

                args.ShouldEqual(new string[0]);
                scriptArgs.ShouldEqual(new[] { "-port", "8080" });
            }

            [Fact]
            public void ShouldHandleJustDoubledash()
            {
                string[] args = new[] { "--" };
                string[] scriptArgs;

                ScriptCsArgs.SplitScriptArgs(ref args, out scriptArgs);

                args.ShouldEqual(new string[0]);
                scriptArgs.ShouldEqual(new string[0]);
            }

            [Fact]
            public void ShouldHandleExtraDoubledash()
            {
                string[] args = new[] { "scriptname.csx", "-restore", "--", "-port", "--", "8080" };
                string[] scriptArgs;

                ScriptCsArgs.SplitScriptArgs(ref args, out scriptArgs);

                args.ShouldEqual(new[] { "scriptname.csx", "-restore" });
                scriptArgs.ShouldEqual(new[] { "-port", "--", "8080" });
            }

            [Fact]
            public void ShouldHandleTrailingDoubledash()
            {
                string[] args = new[] { "scriptname.csx", "-restore", "--" };
                string[] scriptArgs;

                ScriptCsArgs.SplitScriptArgs(ref args, out scriptArgs);

                args.ShouldEqual(new[] { "scriptname.csx", "-restore" });
                scriptArgs.ShouldEqual(new string[0]);
            }
        }
    }
}