using System.Linq;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ScriptHostFactory : IScriptHostFactory
    {
        private readonly IConsole _console;
        private readonly Printers _printers;
        private IScriptInfo _scriptInfo;

        public ScriptHostFactory(IConsole console, Printers printers, IScriptInfo scriptInfo)
        {
            _console = console;
            _printers = printers;
            _scriptInfo = scriptInfo;
        }

        public IScriptHost CreateScriptHost(IScriptPackManager scriptPackManager, string[] scriptArgs)
        {
            return new ScriptHost(scriptPackManager, new ScriptEnvironment(scriptArgs, _console, _printers, _scriptInfo.ScriptPath, _scriptInfo.LoadedScripts.ToArray()));
        }
    }
}