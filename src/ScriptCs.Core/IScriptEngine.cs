namespace ScriptCs
{
    public interface IScriptEngine
    {
        string BaseDirectory { get; set; }

        ScriptResult Execute(ScriptEnvironment environment, ScriptPackSession scriptPackSession);
    }
}