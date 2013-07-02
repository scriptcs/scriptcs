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
                // Arrange
                var args = new string[0];
                
                // Act
                string[] scriptArgs;
                ScriptCsArgs.SplitScriptArgs(ref args, out scriptArgs);

                // Assert
                args.ShouldEqual(new string[0]);
                scriptArgs.ShouldEqual(new string[0]);
            }

            [Fact]
            public void ShouldHandleMissingDoubledash()
            {
                // Arrange
                var args = new[] { "scriptname.csx", "-restore" };

                // Act
                string[] scriptArgs;
                ScriptCsArgs.SplitScriptArgs(ref args, out scriptArgs);

                // Assert
                args.ShouldEqual(new[] { "scriptname.csx", "-restore" });
                scriptArgs.ShouldEqual(new string[0]);
            }

            [Fact]
            public void ShouldHandleArgsAndScriptArgs()
            {
                // Arrange
                var args = new[] { "scriptname.csx", "-restore", "--", "-port", "8080" };

                // Act
                string[] scriptArgs;
                ScriptCsArgs.SplitScriptArgs(ref args, out scriptArgs);

                // Assert
                args.ShouldEqual(new[] { "scriptname.csx", "-restore" });
                scriptArgs.ShouldEqual(new[] { "-port", "8080" });
            }

            [Fact]
            public void ShouldHandleJustScriptArgs()
            {
                // Arrange
                var args = new[] { "--", "-port", "8080" };

                // Act
                string[] scriptArgs;
                ScriptCsArgs.SplitScriptArgs(ref args, out scriptArgs);

                // Assert
                args.ShouldEqual(new string[0]);
                scriptArgs.ShouldEqual(new[] { "-port", "8080" });
            }

            [Fact]
            public void ShouldHandleJustDoubledash()
            {
                // Arrange
                var args = new[] { "--" };

                // Act
                string[] scriptArgs;
                ScriptCsArgs.SplitScriptArgs(ref args, out scriptArgs);

                // Assert
                args.ShouldEqual(new string[0]);
                scriptArgs.ShouldEqual(new string[0]);
            }

            [Fact]
            public void ShouldHandleExtraDoubledash()
            {
                // Arrange
                var args = new[] { "scriptname.csx", "-restore", "--", "-port", "--", "8080" };

                // Act
                string[] scriptArgs;
                ScriptCsArgs.SplitScriptArgs(ref args, out scriptArgs);

                // Assert
                args.ShouldEqual(new[] { "scriptname.csx", "-restore" });
                scriptArgs.ShouldEqual(new[] { "-port", "--", "8080" });
            }

            [Fact]
            public void ShouldHandleTrailingDoubledash()
            {
                // Arrange
                var args = new[] { "scriptname.csx", "-restore", "--" };

                // Act
                string[] scriptArgs;
                ScriptCsArgs.SplitScriptArgs(ref args, out scriptArgs);

                // Assert
                args.ShouldEqual(new[] { "scriptname.csx", "-restore" });
                scriptArgs.ShouldEqual(new string[0]);
            }
        }
    }
}