namespace ScriptCs
{
    using System.Collections.Generic;
    using System.Linq;

    using ScriptCs.Contracts;

    public class DependenciesPreProcessor : IDependenciesPreProcessor
    {
        private readonly IReferenceLineProcessor _referenceLineProcessor;

        private readonly ILoadLineProcessor _loadLineProcessor;

        private readonly IFileSystem _fileSystem;

        public DependenciesPreProcessor(
            IReferenceLineProcessor referenceLineProcessor,
            ILoadLineProcessor loadLineProcessor,
            IFileSystem fileSystem)
        {
            _referenceLineProcessor = referenceLineProcessor;
            _loadLineProcessor = loadLineProcessor;
            _fileSystem = fileSystem;
        }

        public IEnumerable<string> GetDependencies(string path)
        {
            var oldCurrentDirectory = _fileSystem.CurrentDirectory;
            _fileSystem.CurrentDirectory = _fileSystem.GetWorkingDirectory(path);

            foreach (var line in _fileSystem.ReadAllLines(path).Where(l => l.StartsWith("#r") || l.StartsWith("#load")))
            {
                if (line.StartsWith("#r"))
                {
                    var argument = _referenceLineProcessor.GetDirectiveArgument(line);
                    var referencePath = _referenceLineProcessor.GetArgumentFullPath(argument);
                    if (_fileSystem.FileExists(referencePath))
                    {
                        yield return referencePath;
                    }
                }
                else
                {
                    var argument = _loadLineProcessor.GetDirectiveArgument(line);
                    var fullPath = _loadLineProcessor.GetArgumentFullPath(argument);

                    yield return fullPath;

                    foreach (var p in this.GetDependencies(fullPath))
                    {
                        yield return p;
                    }
                }
            }

            _fileSystem.CurrentDirectory = oldCurrentDirectory;
        }
    }
}