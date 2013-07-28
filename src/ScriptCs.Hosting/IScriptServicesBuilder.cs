namespace ScriptCs
{
    public interface IScriptServicesBuilder : IScriptServiceConfiguration<IScriptServicesBuilder>
    {
        ScriptServices Build();
        IScriptServicesBuilder Debug(bool debug = true);
        IScriptServicesBuilder ScriptName(string name);
        IScriptServicesBuilder Repl(bool repl = true);
        IScriptServicesBuilder LogLevel(LogLevel level);
        void LoadModules(string extension, params string[] moduleNames);
    }
}