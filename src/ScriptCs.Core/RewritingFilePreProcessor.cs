namespace ScriptCs
{
    using System;
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

        protected override FilePreProcessorResult Process(Action<FileParserContext> parseAction)
        {
            var result = base.Process(parseAction);

            var rewrittenResult = _rewriters.Aggregate(result, (r, rewriter) => rewriter.Rewrite(r));

            return rewrittenResult;
        }
    }
}