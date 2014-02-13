using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ScriptedScriptPackLoader : IScriptedScriptPackLoader
    {
        private readonly IScriptedScriptPackFinder _finder;
        private readonly IScriptExecutor _executor;
        private readonly IFileSystem _fileSystem;
        private readonly ILog _logger;

        public ScriptedScriptPackLoader(IScriptedScriptPackFinder finder, IScriptExecutor executor, IFileSystem fileSystem, ILog logger)
        {
            _finder = finder;
            _executor = executor;
            _fileSystem = fileSystem;
            _logger = logger;
        }

        public IEnumerable<Tuple<String, ScriptResult>> Load()
        {
            _logger.Info("Finding scripted script packs");
            var scriptPaths = _finder.GetScriptedScriptPacks(_fileSystem.CurrentDirectory);
            return null;
        } 
    }
}
