using Moq;
using Ploeh.AutoFixture.Xunit;
using ScriptCs.Contracts;
using ScriptCs.Tests;
using Should;
using Xunit;
using Xunit.Extensions;

namespace ScriptCs.Hosting.Tests
{
    public class ReplBufferTests
    {
        private const string BackspaceSequence = "\b \b";

        public class TheStartLineMethod
        {
            [Theory, InputLineAutoData]
            public void ShouldClearTheBuffer(ReplBuffer replBuffer, string testString)
            {
                replBuffer.Line = testString;

                replBuffer.StartLine();

                replBuffer.Line.ShouldBeEmpty();
            }
        }

        public class SetTheLineProperty
        {
            [Theory, InputLineAutoData]
            public void ShouldReplacePreviousLine(ReplBuffer replBuffer, string firstString, string newString)
            {
                replBuffer.Line = firstString;

                replBuffer.Line = newString;

                replBuffer.Line.ShouldEqual(newString);
            }

            [Theory, InputLineAutoData]
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
            [Theory, InputLineAutoData]
            public void ShouldReturnTheLineLength(ReplBuffer replBuffer, string testString)
            {
                replBuffer.Line = testString;

                replBuffer.Position.ShouldEqual(testString.Length);
            }
        }

        public class TheBackMethod
        {
            [Theory, InputLineAutoData("foobar")]
            public void ShouldTruncateTheLine(string testString, ReplBuffer replBuffer)
            {
                replBuffer.Line = testString;

                replBuffer.Back(4);

                replBuffer.Line.ShouldEqual("fo");
            }

            [Theory, InputLineAutoData("foobar")]
            public void ShouldTruncateTheConsoleLine(string testString, [Frozen] Mock<IConsole> consoleMock, ReplBuffer replBuffer)
            {
                replBuffer.Line = testString;

                replBuffer.Back(4);

                consoleMock.Verify(c => c.Write(BackspaceSequence), Times.Exactly(4));
            }

            [Theory, InputLineAutoData("foo")]
            public void ShouldNotTruncateTooMuch(string testString, [Frozen] Mock<IConsole> consoleMock, ReplBuffer replBuffer)
            {
                replBuffer.Line = testString;

                replBuffer.Back(5);

                replBuffer.Line.ShouldBeEmpty();
                consoleMock.Verify(c => c.Write(BackspaceSequence), Times.Exactly(3));
            }

            [Theory, InputLineAutoData]
            public void ShouldIgnoreNegativeSteps(string testString, [Frozen] Mock<IConsole> consoleMock, ReplBuffer replBuffer)
            {
                replBuffer.Line = testString;

                replBuffer.Back(-3);

                replBuffer.Line.ShouldEqual(testString);
                consoleMock.Verify(c => c.Write(It.IsAny<string>()), Times.Once()); // Called once at set up but should not be called during test
            }
        }

        public class TheResetToMethod
        {
            [Theory, InputLineAutoData("foobar")]
            public void ShouldTruncateTheLine(string testString, ReplBuffer replBuffer)
            {
                replBuffer.Line = testString;

                replBuffer.ResetTo(4);

                replBuffer.Line.ShouldEqual("foob");
            }

            [Theory, InputLineAutoData("foobar")]
            public void ShouldTruncateTheConsoleLine(string testString, [Frozen] Mock<IConsole> consoleMock, ReplBuffer replBuffer)
            {
                replBuffer.Line = testString;

                replBuffer.ResetTo(4);

                consoleMock.Verify(c => c.Write(BackspaceSequence), Times.Exactly(2));
            }

            [Theory, InputLineAutoData("foo")]
            public void ShouldNotTruncateTooMuch(string testString, [Frozen] Mock<IConsole> consoleMock, ReplBuffer replBuffer)
            {
                replBuffer.Line = testString;

                replBuffer.ResetTo(-4);

                replBuffer.Line.ShouldBeEmpty();
                consoleMock.Verify(c => c.Write(BackspaceSequence), Times.Exactly(3));
            }

            [Theory, InputLineAutoData("foo")]
            public void ShouldIgnoreOutOfBoundsPositions(string testString, [Frozen] Mock<IConsole> consoleMock, ReplBuffer replBuffer)
            {
                replBuffer.Line = testString;

                replBuffer.ResetTo(8);

                replBuffer.Line.ShouldEqual(testString);
                consoleMock.Verify(c => c.Write(It.IsAny<string>()), Times.Once());
                    // Called once at set up but should not be called during test
            }
        }

        public class TheAppendMethod
        {
            [Theory, InputLineAutoData]
            public void ShouldAddCharacterToTheLine(string testString, char testChar, ReplBuffer replBuffer)
            {
                replBuffer.Line = testString;

                replBuffer.Append(testChar);

                replBuffer.Line.ShouldEqual(testString + testChar);
            }

            [Theory, InputLineAutoData]
            public void ShouldAddCharacterToTheConsoleLine(char testChar, [Frozen] Mock<IConsole> consoleMock, ReplBuffer replBuffer)
            {
                replBuffer.Append(testChar);

                consoleMock.Verify(c => c.Write(testChar), Times.Once());
            }

            [Theory, InputLineAutoData]
            public void ShouldAddStringToTheLine(string firstString, string secondString, ReplBuffer replBuffer)
            {
                replBuffer.Line = firstString;

                replBuffer.Append(secondString);

                replBuffer.Line.ShouldEqual(firstString + secondString);
            }

            [Theory, InputLineAutoData]
            public void ShouldAddStringToTheConsoleLine(string testString, [Frozen] Mock<IConsole> consoleMock, ReplBuffer replBuffer)
            {
                replBuffer.Append(testString);

                consoleMock.Verify(c => c.Write(testString), Times.Once());
            }
        }
    }
}
