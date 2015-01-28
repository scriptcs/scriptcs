namespace ScriptCs.Engine.Mono.Segmenter
{
    public class SegmentResult
    {
        public SegmentType Type { get; set; }

        public int BeginLine { get; set; }

        public string Code { get; set; }
    }
}