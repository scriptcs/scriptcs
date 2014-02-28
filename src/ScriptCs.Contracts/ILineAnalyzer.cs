namespace ScriptCs.Contracts
{
    public interface ILineAnalyzer
    {
        string CurrentText { get; }
        LineState CurrentState { get; }
        int TextPosition { get; }

        void Analyze(string line);
        void Reset();
    }

    public enum LineState { FilePath, AssemblyName, Identifier, Member, Unknown }
}
