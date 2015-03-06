using System;
using System.Reflection;
using PowerArgs;
using ScriptCs.Argument;
using ScriptCs.Command;
using ScriptCs.Hosting;

namespace ScriptCs
{
    internal static class Program
    {
        [LoaderOptimizationAttribute(LoaderOptimization.MultiDomain)]
        private static int Main(string[] args)
        {
            SetProfile();
            
            ArgumentParseResult arguments;
            var console = new ScriptConsole();
            try
            {
                var parser = new ArgumentHandler(new ArgumentParser(), new ConfigFileParser(console), new FileSystem());
                arguments = parser.Parse(args);
            }
            catch(Exception ex)
            {
                console.WriteLine(ex.Message);
                var options = new ArgUsageOptions { ShowPosition = false, ShowType = false };
                var usage = ArgUsage.GetUsage<ScriptCsArgs>(options: options);
                console.WriteLine(usage);
                return 1;
            }

            var scriptServicesBuilder = ScriptServicesBuilderFactory.Create(arguments.CommandArguments, arguments.ScriptArguments);
            var factory = new CommandFactory(scriptServicesBuilder);
            var command = factory.CreateCommand(arguments.CommandArguments, arguments.ScriptArguments);
            return (int)command.Execute();
        }

        private static void SetProfile()
        {
            var profileOptimizationType = Type.GetType("System.Runtime.ProfileOptimization");
            if (profileOptimizationType != null)
            {
                var setProfileRoot = profileOptimizationType.GetMethod("SetProfileRoot", BindingFlags.Public | BindingFlags.Static);
                setProfileRoot.Invoke(null, new object[] { typeof(Program).Assembly.Location });

                var startProfile = profileOptimizationType.GetMethod("StartProfile", BindingFlags.Public | BindingFlags.Static);
                startProfile.Invoke(null, new object[] { typeof(Program).Assembly.GetName().Name + ".profile" });
            }
        }
    }
}
