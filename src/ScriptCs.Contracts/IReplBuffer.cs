namespace ScriptCs.Contracts
{
    public interface IReplBuffer
    {
        string Line { get; set; }
        int Position { get; }
        void StartLine();
        void Back(int count = 1);
        void ResetTo(int newPosition);
        void Append(char ch);
        void Append(string str);
    }
}