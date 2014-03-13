namespace ScriptCs.Contracts
{
	using System.ComponentModel.Composition;

	public interface ICodeRewriter
    {
        string Rewrite(string code);
    }
}