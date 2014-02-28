using ScriptCs.Contracts;
using Should;
using Xunit;

namespace ScriptCs.Hosting.Tests
{
    public class LineAnalyzerTests
    {
        public class TheAnalyzeMethod
        {
            private readonly ILineAnalyzer _analyzer;

            public TheAnalyzeMethod()
            {
                _analyzer = new LineAnalyzer();
            }

            [Fact]
            public void TheCurrentStateShouldShowTheTypeOfMatch()
            {
                _analyzer.Analyze(":cd home");

                _analyzer.CurrentState.ShouldEqual(LineState.FilePath);
            }

            [Fact]
            public void TheCurrentTextShouldBeTheArgument()
            {
                _analyzer.Analyze(":cd home");

                _analyzer.CurrentText.ShouldEqual("home");
            }

            [Fact]
            public void TheTextPositionShouldShowStartOfTheArgument()
            {
                _analyzer.Analyze(":cd home");

                _analyzer.TextPosition.ShouldEqual(4);
            }

            [Fact]
            public void TheCurrentStateShouldBeUnknownWhenThereIsNoMatch()
            {
                _analyzer.Analyze(":foo home");

                _analyzer.CurrentState.ShouldEqual(LineState.Unknown);
            }
        }

        public class TheResetMethod
        {
            private readonly ILineAnalyzer _analyzer;

            public TheResetMethod()
            {
                _analyzer = new LineAnalyzer();
            }

            [Fact]
            public void ShouldSetCurrentStateToUnknown()
            {
                _analyzer.Analyze(":cd home");
                _analyzer.Reset();

                _analyzer.CurrentState.ShouldEqual(LineState.Unknown);
            }
        }
    }
}
