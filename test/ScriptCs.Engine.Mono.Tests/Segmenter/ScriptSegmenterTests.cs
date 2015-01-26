using System.Linq;
using Should;
using Xunit;

namespace ScriptCs.Engine.Mono.Tests.Segmenter
{
    using ScriptCs.Engine.Mono.Segmenter;

    public class ScriptSegmenterTests
    {
        public class SegmentCode
        {
            [Fact]
            public void ShouldSegmentCodeAndReturnInCorrectOrder()
            {
                const string Code = "void Bar() {} Bar(); class A {}";

                var segmenter = new ScriptSegmenter();

                var result = segmenter.Segment(Code);

                result.Count().ShouldEqual(4);

                result[0].Type.ShouldEqual(SegmentType.Class);
                result[0].Code.ShouldEqual("class A {}");
                result[1].Type.ShouldEqual(SegmentType.Prototype);
                result[1].Code.ShouldEqual("Action Bar;");
                result[2].Type.ShouldEqual(SegmentType.Method);
                result[2].Code.ShouldEqual("Bar = delegate () {};");
                result[3].Type.ShouldEqual(SegmentType.Evaluation);
                result[3].Code.ShouldEqual("Bar();");
            }
        }
    }
}