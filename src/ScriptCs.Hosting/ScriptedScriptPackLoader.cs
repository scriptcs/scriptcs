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

        public class ScriptedScriptPackLoadResult
        {
            public IEnumerable<IScriptPack> ScriptPacks { get; private set; } 
            public IEnumerable<Tuple<string, ScriptResult>> Results { get; private set; } 
        }

        public IEnumerable<Tuple<String, ScriptResult>> Load()
        {
            var scriptResults = new List<Tuple<String, ScriptResult>>();
            var scriptPacks = new List<IScriptPack>();
            
            _logger.Info("Finding scripted script packs");
            var scriptPaths = _finder.GetScriptedScriptPacks(_fileSystem.CurrentDirectory);

            _executor.AddReferences(typeof(IScriptPackContext).Assembly);
            _executor.ImportNamespaces("ScriptCs.Contracts");
            var saveDir = _fileSystem.CurrentDirectory;
            foreach (var path in scriptPaths)
            {
                _fileSystem.CurrentDirectory = Path.GetDirectoryName(path);
                var result = _executor.Execute(path);
                var host = (IExtendedScriptHost) _executor.ScriptHost;
                var settings = host.ScriptPackSettings;
                if (settings != null)
                {
                    var scriptPack = new ScriptPackTemplate(settings);
                    //need to inject a locator for retrieving the context from the container
                    //scriptPack.ContextResolver = 
                    scriptPacks.Add(scriptPack);
                }
                else
                {
                    
                }
               
                scriptResults.Add(new Tuple<string, ScriptResult>(path, result));   
            }
            return null;
        } 
    }
}
