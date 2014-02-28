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
        private const string BackspaceSequence = "\b \b";

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

            [Theory, WithoutReplBufferLine]
            public void ShouldReplaceConsoleLine([Frozen] Mock<IConsole> consoleMock, ReplBuffer replBuffer, string firstString, string newString)
            {
                var writeChars = 0;

                consoleMock.Setup(c => c.Write(It.IsAny<char>())).Callback((char c) => writeChars++);

                replBuffer.Line = firstString;

                replBuffer.Line = newString;

                consoleMock.Verify(c => c.Write(BackspaceSequence), Times.Exactly(firstString.Length));
                consoleMock.Verify(c => c.Write(newString), Times.Once());
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

                consoleMock.Verify(c => c.Write(BackspaceSequence), Times.Exactly(4));
            }

            [Theory, WithoutReplBufferLine("foo")]
            public void ShouldNotTruncateTooMuch(string testString, [Frozen] Mock<IConsole> consoleMock, ReplBuffer replBuffer)
            {
                replBuffer.Line = testString;

                replBuffer.Back(5);

                replBuffer.Line.ShouldBeEmpty();
                consoleMock.Verify(c => c.Write(BackspaceSequence), Times.Exactly(3));
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

                consoleMock.Verify(c => c.Write(BackspaceSequence), Times.Exactly(2));
            }

            [Theory, WithoutReplBufferLine("foo")]
            public void ShouldNotTruncateTooMuch(string testString, [Frozen] Mock<IConsole> consoleMock, ReplBuffer replBuffer)
            {
                replBuffer.Line = testString;

                replBuffer.ResetTo(-4);

                replBuffer.Line.ShouldBeEmpty();
                consoleMock.Verify(c => c.Write(BackspaceSequence), Times.Exactly(3));
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
            public void ShouldInsertStringToTheCursorAtPosition(string testString, string newString, [Frozen] Mock<IConsole> consoleMock, ReplBuffer replBuffer)
            {
                replBuffer.Line = testString;

                replBuffer.MoveLeft();

                replBuffer.Insert(newString);

                consoleMock.Verify(c => c.Write(newString + "r"), Times.Once());
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
                consoleMock.Setup(c => c.Position).Returns(() => cursorPos);
                consoleMock.SetupSet(c => c.Position = It.IsAny<int>()).Callback((int p) => cursorPos = p);

                replBuffer.Line = testString;

                replBuffer.MoveLeft();
                replBuffer.MoveLeft();

                consoleMock.Object.Position.ShouldEqual(testString.Length - 2);
            }

            [Theory, WithoutReplBufferLine("foo")]
            public void ShouldNotMoveCursorTooFarLeft(string testString, [Frozen] Mock<IConsole> consoleMock, ReplBuffer replBuffer)
            {
                var cursorPos = testString.Length;
                consoleMock.Setup(c => c.Position).Returns(() => cursorPos);
                consoleMock.SetupSet(c => c.Position = It.IsAny<int>()).Callback((int p) => cursorPos = p);

                replBuffer.Line = testString;

                replBuffer.MoveLeft();
                replBuffer.MoveLeft();
                replBuffer.MoveLeft();
                replBuffer.MoveLeft();

                consoleMock.Object.Position.ShouldEqual(0);
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
                consoleMock.Setup(c => c.Position).Returns(() => cursorPos);
                consoleMock.SetupSet(c => c.Position = It.IsAny<int>()).Callback((int p) => cursorPos = p);

                replBuffer.Line = testString;

                replBuffer.MoveLeft();
                replBuffer.MoveLeft();
                replBuffer.MoveRight();

                consoleMock.Object.Position.ShouldEqual(testString.Length - 1);
            }

            [Theory, WithoutReplBufferLine("foo")]
            public void ShouldNotMoveCursorTooFarRight(string testString, [Frozen] Mock<IConsole> consoleMock, ReplBuffer replBuffer)
            {
                var cursorPos = testString.Length;
                consoleMock.Setup(c => c.Position).Returns(() => cursorPos);
                consoleMock.SetupSet(c => c.Position = It.IsAny<int>()).Callback((int p) => cursorPos = p);

                replBuffer.Line = testString;

                replBuffer.MoveLeft();
                replBuffer.MoveRight();
                replBuffer.MoveRight();

                consoleMock.Object.Position.ShouldEqual(testString.Length);
            }
        }
    }
}
