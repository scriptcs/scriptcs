using System.ComponentModel.Composition;

namespace ScriptCs
{
    [InheritedExport]
    public interface IFilePreProcessor
    {
        string ProcessFile(string path);
    }
}