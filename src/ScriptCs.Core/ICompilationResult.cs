namespace ScriptCs
{
    public interface ICompilationResult
    {
        bool Success { get; }
        string ErrorMessage { get; }
    }
}