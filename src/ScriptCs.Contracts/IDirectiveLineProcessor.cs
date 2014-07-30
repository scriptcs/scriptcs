namespace ScriptCs.Contracts
{
    public interface IDirectiveLineProcessor : ILineProcessor
    {
        bool Matches(string line);
    }
}
