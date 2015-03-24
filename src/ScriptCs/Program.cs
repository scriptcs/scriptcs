using System;
using System.IO;
using System.Linq;
using ScriptCs.Command;

namespace ScriptCs
{
    internal static class Program
    {
        [LoaderOptimizationAttribute(LoaderOptimization.MultiDomain)]
        private static int Main(string[] args)
        {
            ProfileOptimizationShim.SetProfileRoot(Path.GetDirectoryName(typeof(Program).Assembly.Location));
            ProfileOptimizationShim.StartProfile(typeof(Program).Assembly.GetName().Name + ".profile");

            var nonScriptArgs = args.TakeWhile(arg => arg != "--").ToArray();
            var scriptArgs = args.Skip(nonScriptArgs.Length + 1).ToArray();

            ScriptCsArgs commandArgs;
            try
            {
                commandArgs = ScriptCsArgs.Parse(nonScriptArgs);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ScriptCsArgs.GetUsage());
                return 1;
            }

            if (commandArgs.Help)
            {
                Console.WriteLine(ScriptCsArgs.GetUsage());
                return 0;
            }

            if (commandArgs.Version)
            {
                VersionWriter.Write();
                return 0;
            }

            if (commandArgs.Config != null && !File.Exists(commandArgs.Config))
            {
                Console.WriteLine("The specified config file does not exist.");
                return 1;
            }

            var config = new Config()
                .Apply(ConfigMask.ReadGlobalOrDefault())
                .Apply(commandArgs.Config == null ? ConfigMask.ReadLocalOrDefault() : ConfigMask.Read(commandArgs.Config))
                .Apply(ConfigMask.Create(commandArgs));

            var scriptServicesBuilder = ScriptServicesBuilderFactory.Create(config, scriptArgs);
            var factory = new CommandFactory(scriptServicesBuilder);
            var command = factory.CreateCommand(config, scriptArgs);
            return (int)command.Execute();
        }
    }
}
