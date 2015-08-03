namespace ScriptCs
{
    using ScriptCs.Command;

    internal static class Application
    {
        public static int Run(Config config, string[] scriptArgs)
        {
            var scriptServicesBuilder = ScriptServicesBuilderFactory.Create(config, scriptArgs);
            var factory = new CommandFactory(scriptServicesBuilder);
            var command = factory.CreateCommand(config, scriptArgs);
            return (int)command.Execute();
        }
    }
}
