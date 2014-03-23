using ScriptCs.Contracts;

namespace ScriptCs.Hosting
{
    public interface IScriptServicesBuilder : IServiceOverrides<IScriptServicesBuilder>
    {
        ScriptServices Build(IModuleConfiguration config);

        IScriptServicesBuilder Cache(bool cache = true);

        IScriptServicesBuilder ScriptName(string name);

        IScriptServicesBuilder Repl(bool repl = true);

        IScriptServicesBuilder Debug(bool debug = true);

        IScriptServicesBuilder LogLevel(LogLevel level);

        IModuleConfiguration LoadModules(string extension, params string[] moduleNames);
    }
}