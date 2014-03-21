namespace ScriptCs.Contracts
{
    public interface IReplHistory
    {
        string CurrentLine { get; }
        void AddLine(string line);
        string GetPreviousLine();
        string GetNextLine();
    }
}