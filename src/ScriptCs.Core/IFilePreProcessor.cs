using System.ComponentModel.Composition;

namespace Scriptcs
{
    [InheritedExport]
    public interface IFilePreProcessor
    {
        string ProcessFile(string path);
    }
}