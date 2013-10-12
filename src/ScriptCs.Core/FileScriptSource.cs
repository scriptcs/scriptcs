using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ScriptCs.Contracts;

namespace ScriptCs
{
    public class FileScriptSource : IScriptSource
    {
        private IFileSystem _fileSystem;

        public FileScriptSource(string path, IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            Path = path;
        }

        public Task<List<string>> GetLines()
        {
            return Task.FromResult(_fileSystem.ReadFileLines(Path).ToList());
        }

        public string Path { get; private set; }
    }
}
