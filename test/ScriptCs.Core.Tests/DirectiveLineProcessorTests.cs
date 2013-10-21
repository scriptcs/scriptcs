using Moq;
using ScriptCs.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Should;
using Moq.Protected;
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
            public bool InheritedProcessLineCalled { get; private set; }
            protected override bool ProcessLine(IFileParser parser, FileParserContext context, string line)
            {
                InheritedProcessLineCalled = true;
                return true;
            }
        }
    }
}
