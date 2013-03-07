namespace ScriptCs.Contracts
{
    public interface ISession
    {
        object Execute(string code);
    }
}
