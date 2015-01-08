namespace ScriptCs.Tests.Acceptance.Support
{
    using System.IO;

    public sealed class ScenarioDirectory
    {
        private static readonly string rootDirectory = "scenarios";

        private readonly string _name;

        public static ScenarioDirectory Create(string scenario)
        {
            var name = Path.Combine(rootDirectory, scenario);
            FileSystem.EnsureDirectoryDeleted(name);
            FileSystem.EnsureDirectoryCreated(name);
            return new ScenarioDirectory(name);
        }

        private ScenarioDirectory(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }

        public ScenarioDirectory WriteLine(string fileName, string text)
        {
            fileName = Map(fileName);
            FileSystem.EnsureDirectoryCreated(Path.GetDirectoryName(fileName));
            using (var writer = new StreamWriter(fileName, true))
            {
                writer.WriteLine(text);
                writer.Flush();
            }

            return this;
        }

        public void DeleteFile(string fileName)
        {
            FileSystem.EnsureFileDeleted(Map(fileName));
        }

        public string Map(string path)
        {
            return Path.Combine(_name, path);
        }
    }
}
