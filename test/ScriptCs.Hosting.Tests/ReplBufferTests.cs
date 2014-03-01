using Moq;
using Ploeh.AutoFixture.Xunit;
using ScriptCs.Contracts;
using ScriptCs.Tests;
using Should;
using Xunit.Extensions;

namespace ScriptCs.Hosting.Tests
{
    public class ReplBufferTests
    {
        public class TheStartLineMethod
        {
            [Theory, WithoutReplBufferLine]
            public void ShouldClearTheBuffer(ReplBuffer replBuffer, string testString)
            {
                replBuffer.Line = testString;

                replBuffer.StartLine();

                replBuffer.Line.ShouldBeEmpty();
            }

            [Theory, WithoutReplBufferLine]
            public void ShouldResetThePosition(ReplBuffer replBuffer, string testString)
            {
                replBuffer.Line = testString;

                replBuffer.StartLine();

                replBuffer.Position.ShouldEqual(0);
            }
        }

        public class SetTheLineProperty
        {
            [Theory, WithoutReplBufferLine]
            public void ShouldReplacePreviousLine(ReplBuffer replBuffer, string firstString, string newString)
            {
                replBuffer.Line = firstString;

                replBuffer.Line = newString;

                replBuffer.Line.ShouldEqual(newString);
            }

            [Theory, WithoutReplBufferLine("foobar", "dude")]
            public void ShouldReplaceConsoleLine(string str1, string str2, [Frozen] Mock<IConsole> consoleMock, ReplBuffer replBuffer)
            {
                replBuffer.Line = str1;

                replBuffer.Line = str2;

                consoleMock.VerifySet(c => c.HorizontalPosition = 0, Times.Exactly(2));
                consoleMock.Verify(c => c.Write("      "), Times.Once());
                consoleMock.Verify(c => c.Write(str2), Times.Once());
            }
        }

        public class ThePositionProperty
        {
            [Theory, WithoutReplBufferLine]
            public void ShouldReturnTheLineLengthIfCursorNotMoved(ReplBuffer replBuffer, string testString)
            {
                replBuffer.Line = testString;

                replBuffer.Position.ShouldEqual(testString.Length);
            }

            [Theory, WithoutReplBufferLine]
            public void ShouldReturnCurrentPositionIfCurserHasMoved(ReplBuffer replBuffer, string testString)
            {
                replBuffer.Line = testString;

                replBuffer.MoveLeft();

                replBuffer.Position.ShouldEqual(testString.Length - 1);
            }
        }

        public class TheBackMethod
        {
            [Theory, WithoutReplBufferLine("foobar")]
            public void ShouldTruncateTheLine(string testString, ReplBuffer replBuffer)
            {
                replBuffer.Line = testString;

                replBuffer.Back(4);

                replBuffer.Line.ShouldEqual("fo");
            }

            [Theory, WithoutReplBufferLine("foobar")]
            public void ShouldUpdatePosition(string testString, ReplBuffer replBuffer)
            {
                replBuffer.Line = testString;

                replBuffer.Back(4);

                replBuffer.Position.ShouldEqual(2);
            }

            [Theory, WithoutReplBufferLine("foobar")]
            public void ShouldTruncateTheConsoleLine(string testString, [Frozen] Mock<IConsole> consoleMock, ReplBuffer replBuffer)
            {
                replBuffer.Line = testString;

                replBuffer.Back(4);

                consoleMock.VerifySet(c => c.HorizontalPosition = 2, Times.Exactly(2));
                consoleMock.Verify(c => c.Write("    "), Times.Once());
            }

            [Theory, WithoutReplBufferLine("foo")]
            public void ShouldNotTruncateTooMuch(string testString, [Frozen] Mock<IConsole> consoleMock, ReplBuffer replBuffer)
            {
                replBuffer.Line = testString;

                replBuffer.Back(5);

                replBuffer.Line.ShouldBeEmpty();
                consoleMock.Verify(c => c.Write("   "), Times.Once());
            }

            [Theory, WithoutReplBufferLine]
            public void ShouldIgnoreNegativeSteps(string testString, [Frozen] Mock<IConsole> consoleMock, ReplBuffer replBuffer)
            {
                replBuffer.Line = testString;

                replBuffer.Back(-3);

                replBuffer.Line.ShouldEqual(testString);
                consoleMock.Verify(c => c.Write(It.IsAny<string>()), Times.Once()); // Called once at set up but should not be called during test
            }
        }

        public class TheDeleteMethod
        {
            [Theory, WithoutReplBufferLine("foobar")]
            public void ShouldDeleteOneCharacterInTheBuffer(string testString, ReplBuffer replBuffer)
            {
                replBuffer.Line = testString;

                replBuffer.MoveLeft();
                replBuffer.MoveLeft();
                replBuffer.Delete();

                replBuffer.Line.ShouldEqual("foobr");
            }

            [Theory, WithoutReplBufferLine("foobar")]
            public void ShouldDeleteOneCharacterInTheConsole(string testString, [Frozen] Mock<IConsole> consoleMock, ReplBuffer replBuffer)
            {
                replBuffer.Line = testString;

                replBuffer.MoveLeft();
                replBuffer.MoveLeft();
                replBuffer.Delete();

                consoleMock.Verify(c => c.Write("r "));
            }

            [Theory, WithoutReplBufferLine("foobar")]
            public void ShouldDoNothingIfAtEndOfTheLine(string testString, ReplBuffer replBuffer)
            {
                replBuffer.Line = testString;

                replBuffer.Delete();

                replBuffer.Line.ShouldEqual("foobar");
            }

            [Theory, WithoutReplBufferLine]
            public void ShouldDoNothingIfEmptyLine(ReplBuffer replBuffer)
            {
                replBuffer.Delete();

                replBuffer.Line.ShouldEqual("");
            }
        }

        public class TheResetToMethod
        {
            [Theory, WithoutReplBufferLine("foobar")]
            public void ShouldTruncateTheLine(string testString, ReplBuffer replBuffer)
            {
                replBuffer.Line = testString;

                replBuffer.ResetTo(4);

                replBuffer.Line.ShouldEqual("foob");
            }

            [Theory, WithoutReplBufferLine("foobar")]
            public void ShouldTruncateTheConsoleLine(string testString, [Frozen] Mock<IConsole> consoleMock, ReplBuffer replBuffer)
            {
                replBuffer.Line = testString;

                replBuffer.ResetTo(4);

                consoleMock.VerifySet(c => c.HorizontalPosition = 4, Times.Exactly(2));
                consoleMock.Verify(c => c.Write("  "), Times.Once());
            }

            [Theory, WithoutReplBufferLine("foo")]
            public void ShouldNotTruncateTooMuch(string testString, [Frozen] Mock<IConsole> consoleMock, ReplBuffer replBuffer)
            {
                replBuffer.Line = testString;

                replBuffer.ResetTo(-4);

                replBuffer.Line.ShouldBeEmpty();
                consoleMock.VerifySet(c => c.HorizontalPosition = 0, Times.Exactly(2));
                consoleMock.Verify(c => c.Write("   "), Times.Once());
            }

            [Theory, WithoutReplBufferLine("foo")]
            public void ShouldIgnoreOutOfBoundsPositions(string testString, [Frozen] Mock<IConsole> consoleMock, ReplBuffer replBuffer)
            {
                replBuffer.Line = testString;

                replBuffer.ResetTo(8);

                replBuffer.Line.ShouldEqual(testString);
                consoleMock.Verify(c => c.Write(It.IsAny<string>()), Times.Once());
                    // Called once at set up but should not be called during test
            }
        }

        public class TheInsertMethod
        {
            [Theory, WithoutReplBufferLine]
            public void ShouldAddCharacterToTheLine(string testString, char testChar, ReplBuffer replBuffer)
            {
                replBuffer.Line = testString;

                replBuffer.Insert(testChar);

                replBuffer.Line.ShouldEqual(testString + testChar);
            }

            [Theory, WithoutReplBufferLine("foobar")]
            public void ShouldInsertCharacterToTheLineAtPosition(string testString, char testChar, ReplBuffer replBuffer)
            {
                replBuffer.Line = testString;

                replBuffer.MoveLeft();

                replBuffer.Insert(testChar);

                replBuffer.Line.ShouldEqual("fooba" + testChar + "r");
            }

            [Theory, WithoutReplBufferLine]
            public void ShouldAddCharacterToTheConsoleLine(char testChar, [Frozen] Mock<IConsole> consoleMock, ReplBuffer replBuffer)
            {
                replBuffer.Insert(testChar);

                consoleMock.Verify(c => c.Write("" + testChar), Times.Once());
            }

            [Theory, WithoutReplBufferLine("foobar")]
            public void ShouldInsertCharacterToTheConsoleAtPosition(string testString, char testChar, [Frozen] Mock<IConsole> consoleMock, ReplBuffer replBuffer)
            {
                replBuffer.Line = testString;

                replBuffer.MoveLeft();

                replBuffer.Insert(testChar);

                consoleMock.Verify(c => c.Write(testChar + "r"), Times.Once());
                consoleMock.VerifySet(c => c.HorizontalPosition = 6, Times.Exactly(2)); // Once at setup
            }

            [Theory, WithoutReplBufferLine]
            public void ShouldAddStringToTheLine(string firstString, string secondString, ReplBuffer replBuffer)
            {
                replBuffer.Line = firstString;

                replBuffer.Insert(secondString);

                replBuffer.Line.ShouldEqual(firstString + secondString);
            }

            [Theory, WithoutReplBufferLine("foobar")]
            public void ShouldInsertStringToTheLineAtPosition(string testString, string newString, ReplBuffer replBuffer)
            {
                replBuffer.Line = testString;

                replBuffer.MoveLeft();

                replBuffer.Insert(newString);

                replBuffer.Line.ShouldEqual("fooba" + newString + "r");
            }

            [Theory, WithoutReplBufferLine]
            public void ShouldAddStringToTheConsoleLine(string testString, [Frozen] Mock<IConsole> consoleMock, ReplBuffer replBuffer)
            {
                replBuffer.Insert(testString);

                consoleMock.Verify(c => c.Write(testString), Times.Once());
            }

            [Theory, WithoutReplBufferLine("foobar")]
            public void ShouldInsertStringToTheConsoleAtPosition(string testString, string newString, [Frozen] Mock<IConsole> consoleMock, ReplBuffer replBuffer)
            {
                replBuffer.Line = testString;

                replBuffer.MoveLeft();

                replBuffer.Insert(newString);

                consoleMock.Verify(c => c.Write(newString + "r"), Times.Once());
                consoleMock.VerifySet(c => c.HorizontalPosition = 5 + newString.Length, Times.Once());
            }
        }

        public class MoveLeft 
        {
            [Theory, WithoutReplBufferLine]
            public void ShouldMovePositionToTheLeft(string testString, ReplBuffer replBuffer)
            {
                replBuffer.Line = testString;

                replBuffer.MoveLeft();
                replBuffer.MoveLeft();

                replBuffer.Position.ShouldEqual(testString.Length - 2);
            }

            [Theory, WithoutReplBufferLine("foo")]
            public void ShouldNotMovePositionTooFarLeft(string testString, ReplBuffer replBuffer)
            {
                replBuffer.Line = testString;

                replBuffer.MoveLeft();
                replBuffer.MoveLeft();
                replBuffer.MoveLeft();
                replBuffer.MoveLeft();

                replBuffer.Position.ShouldEqual(0);
            }

            [Theory, WithoutReplBufferLine]
            public void ShouldMoveCursorToTheLeft(string testString, [Frozen] Mock<IConsole> consoleMock, ReplBuffer replBuffer)
            {
                var cursorPos = testString.Length;
                consoleMock.Setup(c => c.HorizontalPosition).Returns(() => cursorPos);
                consoleMock.SetupSet(c => c.HorizontalPosition = It.IsAny<int>()).Callback((int p) => cursorPos = p);

                replBuffer.Line = testString;

                replBuffer.MoveLeft();
                replBuffer.MoveLeft();

                consoleMock.Object.HorizontalPosition.ShouldEqual(testString.Length - 2);
            }

            [Theory, WithoutReplBufferLine("foo")]
            public void ShouldNotMoveCursorTooFarLeft(string testString, [Frozen] Mock<IConsole> consoleMock, ReplBuffer replBuffer)
            {
                var cursorPos = testString.Length;
                consoleMock.Setup(c => c.HorizontalPosition).Returns(() => cursorPos);
                consoleMock.SetupSet(c => c.HorizontalPosition = It.IsAny<int>()).Callback((int p) => cursorPos = p);

                replBuffer.Line = testString;

                replBuffer.MoveLeft();
                replBuffer.MoveLeft();
                replBuffer.MoveLeft();
                replBuffer.MoveLeft();

                consoleMock.Object.HorizontalPosition.ShouldEqual(0);
            }
        }

        public class MoveRight
        {
            [Theory, WithoutReplBufferLine]
            public void ShouldMovePositionToTheRight(string testString, ReplBuffer replBuffer)
            {
                replBuffer.Line = testString;

                replBuffer.MoveLeft();
                replBuffer.MoveLeft();
                replBuffer.MoveRight();

                replBuffer.Position.ShouldEqual(testString.Length - 1);
            }

            [Theory, WithoutReplBufferLine]
            public void ShouldNotMovePositionTooFarRight(string testString, ReplBuffer replBuffer)
            {
                replBuffer.Line = testString;

                replBuffer.MoveLeft();
                replBuffer.MoveRight();
                replBuffer.MoveRight();

                replBuffer.Position.ShouldEqual(testString.Length);
            }

            [Theory, WithoutReplBufferLine]
            public void ShouldMoveCursorToTheRight(string testString, [Frozen] Mock<IConsole> consoleMock, ReplBuffer replBuffer)
            {
                var cursorPos = testString.Length;
                consoleMock.Setup(c => c.HorizontalPosition).Returns(() => cursorPos);
                consoleMock.SetupSet(c => c.HorizontalPosition = It.IsAny<int>()).Callback((int p) => cursorPos = p);

                replBuffer.Line = testString;

                replBuffer.MoveLeft();
                replBuffer.MoveLeft();
                replBuffer.MoveRight();

                consoleMock.Object.HorizontalPosition.ShouldEqual(testString.Length - 1);
            }

            [Theory, WithoutReplBufferLine("foo")]
            public void ShouldNotMoveCursorTooFarRight(string testString, [Frozen] Mock<IConsole> consoleMock, ReplBuffer replBuffer)
            {
                var cursorPos = testString.Length;
                consoleMock.Setup(c => c.HorizontalPosition).Returns(() => cursorPos);
                consoleMock.SetupSet(c => c.HorizontalPosition = It.IsAny<int>()).Callback((int p) => cursorPos = p);

                replBuffer.Line = testString;

                replBuffer.MoveLeft();
                replBuffer.MoveRight();
                replBuffer.MoveRight();

                consoleMock.Object.HorizontalPosition.ShouldEqual(testString.Length);
            }
        }
    }
}
