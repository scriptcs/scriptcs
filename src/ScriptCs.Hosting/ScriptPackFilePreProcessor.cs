using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ScriptPackFilePreProcessor : FilePreProcessor
    {
        private readonly IFileSystem _fileSystem;
        private List<string> _processed = new List<string>();
        private int injectionLine = 0;

        public ScriptPackFilePreProcessor(IFileSystem fileSystem, ILog logger, 
            IEnumerable<ILineProcessor> lineProcessors) : base(fileSystem, logger, lineProcessors)
        {
            _fileSystem = fileSystem;
        }

        public override void ParseScript(List<string> scriptLines, FileParserContext context)
        {
            var className = Guid.NewGuid().ToString("N");
            base.ParseScript(scriptLines, context);
            var currentFile = context.LoadedScripts.LastOrDefault().ToUpper();
            if (currentFile.EndsWith("SCRIPTPACK.CSX") && !_processed.Contains(currentFile))  
            {
                _processed.Add(currentFile);
                var body = context.BodyLines;
                body.Insert(injectionLine, string.Format("public class {0} : ScriptPackTemplate {{{1}", className, Environment.NewLine));
                body.Add(string.Format("{0}}}{1}", Environment.NewLine, Environment.NewLine));
                injectionLine = body.Count;
            }
        }
    }
}
