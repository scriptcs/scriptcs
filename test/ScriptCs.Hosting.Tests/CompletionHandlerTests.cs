using Ploeh.AutoFixture.Xunit;
using ScriptCs.Contracts;
using ScriptCs.Tests;
using Moq;
using Xunit.Extensions;

namespace ScriptCs.Hosting.Tests
{
    public class CompletionHandlerTests
    {
        public class TheUpdateBufferWithCompletionMethod
        {
            [Theory, ScriptCsAutoData]
            public void ShouldSetBufferToFirstCompletionOnNewCompletionTask(string fragment, int position, [Frozen] Mock<ILineAnalyzer> lineAnalyzerMock, [Frozen] Mock<IReplBuffer> bufferMock, CompletionHandler completionHandler)
            {
                string firstCompletion = fragment + "1";

                lineAnalyzerMock.Setup(la => la.CurrentText).Returns(fragment);
                lineAnalyzerMock.Setup(la => la.TextPosition).Returns(position);

                completionHandler.UpdateBufferWithCompletion(str => new []{ str + "1", str + "2" });

                bufferMock.Verify(b => b.ResetTo(position), Times.Once());
                bufferMock.Verify(b => b.Insert(firstCompletion), Times.Once());
            }

            [Theory, ScriptCsAutoData]
            public void ShouldSetBufferToSecondCompletionOnSecondCall(string fragment, int position, [Frozen] Mock<ILineAnalyzer> lineAnalyzerMock, [Frozen] Mock<IReplBuffer> bufferMock, CompletionHandler completionHandler)
            {
                string secondCompletion = fragment + "2";

                lineAnalyzerMock.Setup(la => la.CurrentText).Returns(fragment);
                lineAnalyzerMock.Setup(la => la.TextPosition).Returns(position);

                completionHandler.UpdateBufferWithCompletion(str => new[] { str + "1", str + "2" });
                completionHandler.UpdateBufferWithCompletion(str => new[] { "" });

                bufferMock.Verify(b => b.ResetTo(position), Times.Exactly(2));
                bufferMock.Verify(b => b.Insert(secondCompletion), Times.Once());
            }

            [Theory, ScriptCsAutoData]
            public void ShouldSetBufferToFirstCompletionWhenCompletionListHasBeenTraversed(string fragment, int position, [Frozen] Mock<ILineAnalyzer> lineAnalyzerMock, [Frozen] Mock<IReplBuffer> bufferMock, CompletionHandler completionHandler)
            {
                string firstCompletion = fragment + "1";

                lineAnalyzerMock.Setup(la => la.CurrentText).Returns(fragment);
                lineAnalyzerMock.Setup(la => la.TextPosition).Returns(position);

                completionHandler.UpdateBufferWithCompletion(str => new[] { str + "1", str + "2" });
                completionHandler.UpdateBufferWithCompletion(str => new[] { "" });
                completionHandler.UpdateBufferWithCompletion(str => new[] { "" });

                bufferMock.Verify(b => b.ResetTo(position), Times.Exactly(3));
                bufferMock.Verify(b => b.Insert(firstCompletion), Times.Exactly(2));
            }
        }

        public class TheUpdateBufferWithPreviousMethod
        {
            [Theory, ScriptCsAutoData]
            public void ShouldSetBufferToPreviousCompletion(string fragment, int position, [Frozen] Mock<ILineAnalyzer> lineAnalyzerMock, [Frozen] Mock<IReplBuffer> bufferMock, CompletionHandler completionHandler)
            {
                string thirdCompletion = fragment + "3";

                lineAnalyzerMock.Setup(la => la.CurrentText).Returns(fragment);
                lineAnalyzerMock.Setup(la => la.TextPosition).Returns(position);

                completionHandler.UpdateBufferWithCompletion(str => new[] { str + "1", str + "2", str + "3", str + "4" });
                completionHandler.UpdateBufferWithCompletion(str => new[] { "" });
                completionHandler.UpdateBufferWithCompletion(str => new[] { "" });
                completionHandler.UpdateBufferWithCompletion(str => new[] { "" });

                completionHandler.UpdateBufferWithPrevious();

                bufferMock.Verify(b => b.ResetTo(position), Times.Exactly(5));
                bufferMock.Verify(b => b.Insert(thirdCompletion), Times.Exactly(2));
            }

            [Theory, ScriptCsAutoData]
            public void ShouldSetBufferToLastCompletionWhenAtFirst(string fragment, int position, [Frozen] Mock<ILineAnalyzer> lineAnalyzerMock, [Frozen] Mock<IReplBuffer> bufferMock, CompletionHandler completionHandler)
            {
                string fourthCompletion = fragment + "4";

                lineAnalyzerMock.Setup(la => la.CurrentText).Returns(fragment);
                lineAnalyzerMock.Setup(la => la.TextPosition).Returns(position);

                completionHandler.UpdateBufferWithCompletion(str => new[] { str + "1", str + "2", str + "3", str + "4" });

                completionHandler.UpdateBufferWithPrevious();

                bufferMock.Verify(b => b.ResetTo(position), Times.Exactly(2));
                bufferMock.Verify(b => b.Insert(fourthCompletion), Times.Exactly(1));
            }

            [Theory, ScriptCsAutoData]
            public void ShouldDoNothingWhenNotCompleting([Frozen] Mock<IReplBuffer> bufferMock, CompletionHandler completionHandler)
            {
                completionHandler.UpdateBufferWithPrevious();

                bufferMock.Verify(b => b.Insert(It.IsAny<string>()), Times.Never());
            }
        }

        public class TheAbortMethod
        {
            [Theory, ScriptCsAutoData]
            public void ShouldResetBuffer(string fragment, int position, [Frozen] Mock<ILineAnalyzer> lineAnalyzerMock, [Frozen] Mock<IReplBuffer> bufferMock, CompletionHandler completionHandler)
            {
                lineAnalyzerMock.Setup(la => la.CurrentText).Returns(fragment);
                lineAnalyzerMock.Setup(la => la.TextPosition).Returns(position);

                completionHandler.UpdateBufferWithCompletion(str => new[] { str + "1", str + "2", str + "3", str + "4" });

                completionHandler.Abort();

                bufferMock.Verify(b => b.ResetTo(position), Times.Exactly(2));
                bufferMock.Verify(b => b.Insert(fragment), Times.Exactly(1));
            }

            [Theory, ScriptCsAutoData]
            public void ShouldDoNothingIfNotCompleting([Frozen] Mock<IReplBuffer> bufferMock, CompletionHandler completionHandler)
            {
                completionHandler.Abort();

                bufferMock.Verify(b => b.Insert(It.IsAny<string>()), Times.Never());
            }
        }

        public class TheResetMethod
        {
            [Theory, ScriptCsAutoData]
            public void ShouldResetCompletion(string fragment, int position,
                [Frozen] Mock<ILineAnalyzer> lineAnalyzerMock, [Frozen] Mock<IReplBuffer> bufferMock, CompletionHandler completionHandler)
            {
                string firstCompletion = fragment + "3";

                lineAnalyzerMock.Setup(la => la.CurrentText).Returns(fragment);
                lineAnalyzerMock.Setup(la => la.TextPosition).Returns(position);

                completionHandler.UpdateBufferWithCompletion(str => new[] {str + "1", str + "2"});
                completionHandler.Reset();
                completionHandler.UpdateBufferWithCompletion(str => new[] {str + "3", str + "4" });

                bufferMock.Verify(b => b.ResetTo(position), Times.Exactly(2));
                bufferMock.Verify(b => b.Insert(firstCompletion), Times.Once());
            }
        }
    }
}
