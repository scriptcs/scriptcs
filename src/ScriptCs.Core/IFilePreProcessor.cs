using System.ComponentModel.Composition;

namespace ScriptCs
{
    public interface IFilePreProcessor
    {
        string ProcessFile(string path);
    }
}