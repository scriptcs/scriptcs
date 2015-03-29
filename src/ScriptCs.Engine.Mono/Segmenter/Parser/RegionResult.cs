namespace ScriptCs.Engine.Mono.Segmenter.Parser
{
    public class RegionResult
    {
        public int Offset { get; set; }

        public int Length { get; set; }

        public bool IsCompleteBlock { get; set; }

        public RegionResult Combine(RegionResult region)
        {
            Guard.AgainstNullArgument("region", region);

            return new RegionResult
            {
                Length = Length + region.Length + (region.Offset - (Offset + Length)),
                Offset = Offset,
                IsCompleteBlock = IsCompleteBlock && region.IsCompleteBlock
            };
        }
    }
}