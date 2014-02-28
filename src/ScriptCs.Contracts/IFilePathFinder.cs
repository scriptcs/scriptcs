namespace ScriptCs.Contracts
{
    public interface IFilePathFinder
    {
        string[] FindPossibleAssemblyNames(string nameFragment, IFileSystem fileSystem);
        string[] FindPossibleFilePaths(string pathFragment, IFileSystem fileSystem);
    }
}