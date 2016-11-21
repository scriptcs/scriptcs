using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ScriptHostFactory : IScriptHostFactory
    {
        private readonly IConsole _console;
        private readonly Printers _printers;

        public ScriptHostFactory(IConsole console, Printers printers)
        {
            _console = console;
            _printers = printers;
        }

        public IScriptHost CreateScriptHost(IScriptPackManager scriptPackManager, string[] scriptArgs)
        {
            return new ScriptHost(scriptPackManager, new ScriptEnvironment(scriptArgs, _console, _printers));
        }
    }
}