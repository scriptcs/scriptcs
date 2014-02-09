namespace ScriptCs.Contracts
{
    public interface IScriptPackSettingsReferences
    {
        IScriptPackSettingsImports References(params string[] references);
    }
}