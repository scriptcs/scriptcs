using ScriptCs.Contracts;
using Should;
using Xunit;

namespace ScriptCs.Tests
{
    public class DirectiveLineProcessorTests
    {
        public class TheProcessLineMethod
        {
            [Fact]
            public void ShouldReturnTrueButNotContinueProcessingDirectiveIfAfterCodeByDefault()
            {
                var directiveLineProcessor = new TestableDirectiveLineProcessor();
                var returnValue = directiveLineProcessor.ProcessLine(null, null, "#Test x", false);
                returnValue.ShouldBeTrue();
                directiveLineProcessor.InheritedProcessLineCalled.ShouldBeFalse();
            }

            [Fact]
            public void ShouldReturnTrueAndContinueProcessingDirectiveIfAfterCodeAndBehaviourAfterCodeIsAllow()
            {
                var directiveLineProcessor = new TestableDirectiveLineProcessor(BehaviorAfterCode.Allow);
                var returnValue = directiveLineProcessor.ProcessLine(null, null, "#Test x", false);
                returnValue.ShouldBeTrue();
                directiveLineProcessor.InheritedProcessLineCalled.ShouldBeTrue();
            }
        }

        public class TheMatchesMethod
        {
            [Fact]
            public void ShouldReturnTrueWhenLineMatchesDirectiveStringWithAnArgument()
            {
                var directiveLineProcessor = new TestableDirectiveLineProcessor();
                directiveLineProcessor.Matches("#Test argument").ShouldBeTrue();
            }

            [Fact]
            public void ShouldReturnTrueWhenLineMatchesDirectiveStringWithoutAnArgument()
            {
                var directiveLineProcessor = new TestableDirectiveLineProcessor();
                directiveLineProcessor.Matches("#Test").ShouldBeTrue();
            }

            [Fact]
            public void ShouldReturnFalseWhenLineDoesNotMatchDirectiveStringWithAnArgument()
            {
                var directiveLineProcessor = new TestableDirectiveLineProcessor();
                directiveLineProcessor.Matches("#NotATest argument").ShouldBeFalse();
            }

            [Fact]
            public void ShouldReturnFalseWhenLineDoesNotMatchDirectiveStringWithoutAnArgument()
            {
                var directiveLineProcessor = new TestableDirectiveLineProcessor();
                directiveLineProcessor.Matches("#NotATest").ShouldBeFalse();
            }
        }

        public class TheGetDirectiveArgumentMethod
        {
            [Fact]
            public void ShouldParseTheArgumentFromTheDirectiveLine()
            {
                var directiveLineProcessor = new TestableDirectiveLineProcessor(BehaviorAfterCode.Allow);
                directiveLineProcessor.ProcessLine(null, null, "#Test argument", false);
                directiveLineProcessor.ArgumentParsedCorrectly.ShouldBeTrue();
            }
        }

        public class TestableDirectiveLineProcessor : DirectiveLineProcessor
        {
            private BehaviorAfterCode? _behaviourAfterCode;
            public TestableDirectiveLineProcessor()
            { }
            public TestableDirectiveLineProcessor(BehaviorAfterCode behaviourAfterCode)
            {
                _behaviourAfterCode = behaviourAfterCode;
            }
            protected override BehaviorAfterCode BehaviorAfterCode
            {
                get
                {
                    return _behaviourAfterCode ?? base.BehaviorAfterCode;
                }
            }
            protected override string DirectiveName
            {
                get { return "Test"; }
            }
            public bool ArgumentParsedCorrectly { get; private set; }
            public bool InheritedProcessLineCalled { get; private set; }
            protected override bool ProcessLine(IFileParser parser, FileParserContext context, string line)
            {
                ArgumentParsedCorrectly = GetDirectiveArgument(line) == "argument";
                InheritedProcessLineCalled = true;
                return true;
            }
        }
    }
}
