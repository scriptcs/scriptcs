namespace ScriptCs.Contracts
{
    public interface IFilePathFinder
    {
        string[] FindPossibleAssemblyNames(string nameFragment);
        string[] FindPossibleFilePaths(string pathFragment);
    }
}