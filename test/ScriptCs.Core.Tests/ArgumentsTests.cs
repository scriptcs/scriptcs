using System;
using System.Collections.Generic;
using Xunit;

namespace ScriptCs.Tests
{
    //from http://jake.ginnivan.net/c-sharp-argument-parser/ - thanks Jake!
    public class ArgumentsTests
    {
        [Fact]
        public void ArgumentBooleanTest()
        {
            IEnumerable<string> args = new[]
            {
                "-testBool"
            };
            var target = new Arguments(args);
            Assert.True(target.IsTrue("testBool"));
        }

        [Fact]
        public void IsTrueDoesntExist()
        {
            IEnumerable<string> args = new string[] { };
            var target = new Arguments(args);
            Assert.False(target.IsTrue("doesntExist"));
        }

        [Fact]
        public void ArgumentDoubleDashesTest()
        {
            IEnumerable<string> args = new[]
            {
                "--testArg=Value"
            };
            var target = new Arguments(args);
            Assert.Equal("Value", target.Single("testArg"));
        }

        [Fact]
        public void ArgumentSingleTest()
        {
            IEnumerable<string> args = new[]
            {
                "-test:Value"
            };
            var target = new Arguments(args);
            Assert.Equal(1, target["test"].Count);
            Assert.Equal("Value", target.Single("test"));
        }

        [Fact]
        public void ArgumentWithSpaceSeparatorTest()
        {
            IEnumerable<string> args = Arguments.SplitCommandLine("-test Value");

            var target = new Arguments(args);
            Assert.Equal(1, target["test"].Count);
            Assert.Equal("Value", target.Single("test"));
        }

        [Fact]
        public void ArgumentWithSpaceSeparatorAndSpaceInValueTest()
        {
            IEnumerable<string> args = Arguments.SplitCommandLine("-test \"Value With Space\"");

            var target = new Arguments(args);
            Assert.Equal(1, target["test"].Count);
            Assert.Equal("Value With Space", target.Single("test"));
        }

        [Fact]
        public void AddWaitingAsFlagTest()
        {
            IEnumerable<string> args = Arguments.SplitCommandLine("-flag -test \"Value With Space\"");

            var target = new Arguments(args);
            Assert.Equal(2, target.Count);
            Assert.Equal(1, target["test"].Count);
            Assert.Equal("Value With Space", target.Single("test"));
            Assert.True(target.IsTrue("flag"));
        }

        [Fact]
        public void AddSingleTwiceTest()
        {
            IEnumerable<string> args = Arguments.SplitCommandLine("-flag");

            var target = new Arguments(args);

            Assert.Throws<ArgumentException>(() => target.AddSingle("flag", "true"));
        }

        [Fact]
        public void FlagsTest()
        {
            IEnumerable<string> args = Arguments.SplitCommandLine("-flag1 -flag2");

            var target = new Arguments(args);

            Assert.True(target.IsTrue("flag1"));
            Assert.True(target.IsTrue("flag2"));
        }

        [Fact]
        public void RemoveTest()
        {
            IEnumerable<string> args = Arguments.SplitCommandLine("-flag1 -flag2");

            var target = new Arguments(args);

            Assert.True(target.IsTrue("flag1"));
            Assert.True(target.IsTrue("flag2"));
            target.Remove("flag1");
            Assert.False(target.Exists("flag1"));
            Assert.True(target.IsTrue("flag2"));
        }

        [Fact]
        public void SingleReturnsNullIfNotDefinedTest()
        {

            var target = new Arguments(new string[] { });

            Assert.False(target.Exists("notDefined"));
            Assert.Null(target.Single("notDefined"));
        }

        [Fact]
        public void ExistsTest()
        {
            IEnumerable<string> args = Arguments.SplitCommandLine("-flag1");

            var target = new Arguments(args);

            Assert.True(target.Exists("flag1"));
        }

        [Fact]
        public void ArgumentListTest()
        {
            IEnumerable<string> args = new[]
            {
                "-test:Value",
                "-test:Value2"
            };
            var target = new Arguments(args);
            Assert.Equal(2, target["test"].Count);
            Assert.Equal("Value", target["test"][0]);
            Assert.Equal("Value2", target["test"][1]);
        }

        [Fact]
        public void ArgumentPathTest()
        {
            IEnumerable<string> args = new[]
            {
                "-test:Value",
                @"-test:C:\Folder\"
            };
            var target = new Arguments(args);
            Assert.Equal(2, target["test"].Count);
            Assert.Equal("Value", target["test"][0]);
            Assert.Equal(@"C:\Folder\", target["test"][1]);
        }

        [Fact]
        public void ArgumentQuotedPathTest()
        {
            IEnumerable<string> args = new[]
            {
                "-test:Value",
                "-test:\"C:\\Folder\\\""
            };
            var target = new Arguments(args);
            Assert.Equal(2, target["test"].Count);
            Assert.Equal("Value", target["test"][0]);
            Assert.Equal("C:\\Folder\\", target["test"][1]);
        }

        [Fact]
        public void ArgumentQuotedPathWithSpaceTest()
        {
            IEnumerable<string> args = new[]
            {
                "-test:Value",
                "-test:\"C:\\Folder Name\\\""
            };
            var target = new Arguments(args);
            Assert.Equal(2, target["test"].Count);
            Assert.Equal("Value", target["test"][0]);
            Assert.Equal("C:\\Folder Name\\", target["test"][1]);
        }

        [Fact]
        public void ArgumentQuotedPathWithSpaceAndFollowingArgTest()
        {
            IEnumerable<string> args = new[]
            {
                "-test:Value",
                "-test:\"C:\\Folder Name\\\"",
                "-testPath:\"C:\\Folder2\\\"",
                "-boolArg"
            };

            var target = new Arguments(args);
            Assert.Equal(2, target["test"].Count);
            Assert.Equal(@"C:\Folder2\", target.Single("testPath"));
            Assert.True(target.IsTrue("boolArg"));

            Assert.Equal("Value", target["test"][0]);
            Assert.Equal("C:\\Folder Name\\", target["test"][1]);
        }

        [Fact]
        public void ArgumentListRequestSingleThrowsExceptionTest()
        {
            IEnumerable<string> args = new[]
            {
                "-test:Value",
                "-test:Value2"
            };

            var target = new Arguments(args);
            //Should throw Argument exception because test is defined more than once
            Assert.Throws<ArgumentException>(() => target.Single("test"));
        }

        [Fact]
        public void ArgumentCommaListTest()
        {
            IEnumerable<string> args = new[]
            {
                "-testList:Value,Value2,Value3"
            };

            var target = new Arguments(args);
            Assert.Equal(3, target["testList"].Count);

            Assert.Equal("Value", target["testList"][0]);
            Assert.Equal("Value2", target["testList"][1]);
            Assert.Equal("Value3", target["testList"][2]);
        }

        [Fact]
        public void BlogExample()
        {
            const string commandLine = "-u -d -mdb=\"c:\\entries.mdb\" -xml=\"j:\\\"";

            var target = new Arguments(Arguments.SplitCommandLine(commandLine));

            Assert.Equal("c:\\entries.mdb", target.Single("mdb"));
            Assert.Equal("j:\\", target.Single("xml"));
        }
    }
}