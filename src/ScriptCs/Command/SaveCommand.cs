using System;
using ScriptCs.Contracts;

namespace ScriptCs.Command
{
    internal class SaveCommand : ISaveCommand
    {
        private readonly IPackageAssemblyResolver _packageAssemblyResolver;

        public SaveCommand(IPackageAssemblyResolver packageAssemblyResolver)
        {
            _packageAssemblyResolver = packageAssemblyResolver;
        }

        public CommandResult Execute()
        {
            Console.WriteLine("Initiated saving packages into packages.config...");
            try
            {
                _packageAssemblyResolver.SavePackages();
            }
            catch (Exception e)
            {
                Console.WriteLine("Save failed: {0}.", e.Message);
                return CommandResult.Error;
            }
            return CommandResult.Success;
        }
    }
}