using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ScriptedScriptPackLoader : IScriptedScriptPackLoader
    {
        private readonly IScriptedScriptPackFinder _finder;
        private readonly IFileSystem _fileSystem;
        private readonly ILog _logger;
        private readonly IScriptPackContextResolver _contextResolver;
        private readonly IScriptPackContextRegistry _contextRegistry;

        public ScriptedScriptPackLoader(IScriptedScriptPackFinder finder, IFileSystem fileSystem, ILog logger, IScriptPackContextResolver contextResolver, IScriptPackContextRegistry contextRegistry)
        {
            _finder = finder;
            _fileSystem = fileSystem;
            _logger = logger;
            _contextResolver = contextResolver;
            _contextRegistry = contextRegistry;
        }

        public ScriptedScriptPackLoadResult Load(IScriptExecutor executor)
        {
            var scriptResults = new List<Tuple<String, ScriptResult>>();
            var scriptPacks = new List<IScriptPack>();
            
            _logger.Debug("Finding scripted script packs");
            var scriptPaths = _finder.GetScriptedScriptPacks(_fileSystem.CurrentDirectory);

            executor.AddReferences(typeof(IScriptPackContext).Assembly);
            executor.ImportNamespaces("ScriptCs.Contracts");
            var saveDir = _fileSystem.CurrentDirectory;
            foreach (var path in scriptPaths)
            {
                _fileSystem.CurrentDirectory = Path.GetDirectoryName(path);
                var result = executor.Execute(path);
                scriptResults.Add(new Tuple<string, ScriptResult>(path, result));   
                var host = (IExtendedScriptHost) executor.ScriptHost;
                var settings = host.ScriptPackSettings;
                if (settings != null)
                {
                    _contextRegistry.Register(settings.GetContextType());
                    var scriptPack = new ScriptPackTemplate(settings) {ContextResolver = _contextResolver};
                    scriptPacks.Add(scriptPack);
                }
            }
            executor.RemoveReferences(typeof(IScriptPackContext).Assembly);
            executor.RemoveNamespaces("ScriptCs.Contracts");
            var loadResult = new ScriptedScriptPackLoadResult(scriptPacks,scriptResults);
            _fileSystem.CurrentDirectory = saveDir;
            return loadResult;
        } 
    }
}
