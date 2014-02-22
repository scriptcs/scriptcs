using ScriptCs.Contracts;
using Should;
using Xunit;

namespace ScriptCs.Hosting.Tests
{
    public class ReplHistoryTests
    {
        public class TheCurrentLineProperty
        {
            private readonly IReplHistory _history;

            public TheCurrentLineProperty()
            {
                _history = new ReplHistory(5);
            }

            [Fact]
            public void ShouldShowTheCurrentLine()
            {
                const string testString = "test";

                _history.AddLine("foo");
                _history.AddLine("bar");
                _history.AddLine(testString);

                _history.CurrentLine.ShouldEqual(testString);
            }
        }

        public class TheAddLineMethod
        {
            private readonly IReplHistory _history;

            public TheAddLineMethod()
            {
                _history = new ReplHistory(3);
            }

            [Fact]
            public void ShouldRemoveOldestLineWhenExceedingLimit()
            {
                const string testString1 = "test1";
                const string testString2 = "test2";
                const string testString3 = "test3";
                const string testString4 = "test4";

                _history.AddLine(testString1);
                _history.AddLine(testString2);
                _history.AddLine(testString3);
                _history.AddLine(testString4);

                var line = _history.GetNextLine();

                line.ShouldEqual(testString2);
            }
        }

        public class TheGetPreviousLineMethod
        {
            private readonly IReplHistory _history;

            public TheGetPreviousLineMethod()
            {
                _history = new ReplHistory(5);
            }

            [Fact]
            public void ShouldMoveBackTheCurrentLine()
            {
                const string testString1 = "test1";
                const string testString2 = "test2";
                const string testString3 = "test3";
                const string testString4 = "test4";

                _history.AddLine(testString1);
                _history.AddLine(testString2);
                _history.AddLine(testString3);
                _history.AddLine(testString4);

                _history.GetPreviousLine();
                _history.GetPreviousLine();

                _history.CurrentLine.ShouldEqual(testString3);
            }

            [Fact]
            public void ShouldReturnThePreviousLine()
            {
                const string testString1 = "test1";
                const string testString2 = "test2";
                const string testString3 = "test3";
                const string testString4 = "test4";

                _history.AddLine(testString1);
                _history.AddLine(testString2);
                _history.AddLine(testString3);
                _history.AddLine(testString4);

                _history.GetPreviousLine();
                var line = _history.GetPreviousLine();

                line.ShouldEqual(testString3);
            }

            [Fact]
            public void ShouldCirculateBackToTheLastLine()
            {
                const string testString1 = "test1";
                const string testString2 = "test2";
                const string testString3 = "test3";

                _history.AddLine(testString1);
                _history.AddLine(testString2);
                _history.AddLine(testString3);

                _history.GetPreviousLine();
                _history.GetPreviousLine();
                _history.GetPreviousLine();
                var line = _history.GetPreviousLine();

                line.ShouldEqual(testString3);
            }
        }

        public class TheGetNextLineMethod
        {
            private readonly IReplHistory _history;

            public TheGetNextLineMethod()
            {
                _history = new ReplHistory(5);
            }

            [Fact]
            public void ShouldMoveTheCurrentLineForward()
            {
                const string testString1 = "test1";
                const string testString2 = "test2";
                const string testString3 = "test3";
                const string testString4 = "test4";

                _history.AddLine(testString1);
                _history.AddLine(testString2);
                _history.AddLine(testString3);
                _history.AddLine(testString4);

                _history.GetPreviousLine();
                _history.GetPreviousLine();
                _history.GetPreviousLine();
                _history.GetNextLine();

                _history.CurrentLine.ShouldEqual(testString3);
            }

            [Fact]
            public void ShouldReturnTheNextLine()
            {
                const string testString1 = "test1";
                const string testString2 = "test2";
                const string testString3 = "test3";
                const string testString4 = "test4";

                _history.AddLine(testString1);
                _history.AddLine(testString2);
                _history.AddLine(testString3);
                _history.AddLine(testString4);

                _history.GetPreviousLine();
                _history.GetPreviousLine();
                _history.GetPreviousLine();
                var line = _history.GetNextLine();

                line.ShouldEqual(testString3);
            }

            [Fact]
            public void ShouldCirculateForwardToTheFirstLine()
            {
                const string testString1 = "test1";
                const string testString2 = "test2";
                const string testString3 = "test3";

                _history.AddLine(testString1);
                _history.AddLine(testString2);
                _history.AddLine(testString3);

                var line = _history.GetNextLine();

                line.ShouldEqual(testString1);
            }
        }
    }
}
