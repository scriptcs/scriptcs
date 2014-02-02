using ScriptCs.Contracts;

namespace ScriptCs
{
    public interface IScriptServicesBuilder : IServiceOverrides<IScriptServicesBuilder>
    {
        ScriptServices Build();

        IScriptServicesBuilder Cache(bool cache = true);

        IScriptServicesBuilder ScriptName(string name);

        IScriptServicesBuilder Repl(bool repl = true);

        IScriptServicesBuilder Debug(bool debug = true);

        IScriptServicesBuilder LogLevel(LogLevel level);

        IScriptServicesBuilder LoadModules(string extension, params string[] moduleNames);
    }
}