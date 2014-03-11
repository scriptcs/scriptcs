namespace ScriptCs
{
    using System.Collections.Generic;
    using System.Linq;

    using Common.Logging;

    using ScriptCs.Contracts;

    public class RewritingFilePreProcessor : FilePreProcessor
    {
        private readonly IEnumerable<ICodeRewriter> _rewriters;

        public RewritingFilePreProcessor(IFileSystem fileSystem, ILog logger, IEnumerable<ILineProcessor> lineProcessors, IEnumerable<ICodeRewriter> rewriters)
            : base(fileSystem, logger, lineProcessors)
        {
            _rewriters = rewriters;
        }

        protected override string GenerateCode(FileParserContext context)
        {
            var code = base.GenerateCode(context);
            var rewrittenCode = _rewriters.Aggregate(code, (c, rewriter) => rewriter.Rewrite(c));

            return rewrittenCode;
        }
    }
}