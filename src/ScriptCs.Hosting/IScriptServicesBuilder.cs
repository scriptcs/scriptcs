using ScriptCs.Contracts;

namespace ScriptCs
{
    public interface IScriptServicesBuilder : IServiceOverrides<IScriptServicesBuilder>
    {
        ScriptServices Build();

        IScriptServicesBuilder InMemory(bool inMemory = true);

        IScriptServicesBuilder ScriptName(string name);

        IScriptServicesBuilder Repl(bool repl = true);

        IScriptServicesBuilder LogLevel(LogLevel level);

        IScriptServicesBuilder LoadModules(string extension, params string[] moduleNames);
    }
}