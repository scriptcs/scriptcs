namespace ScriptCs
{
    public interface ISubmission<T>
    {
        ICompilation Compilation { get; }
    }
}