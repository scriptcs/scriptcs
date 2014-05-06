using System;
using System.ComponentModel.Composition;

namespace ScriptCs.Contracts
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ModuleAttribute : ExportAttribute, IModuleMetadata
    {
        public ModuleAttribute(string name) : base(typeof(IModule))
        {
            Name = name;
            Autoload = false;
        }

        public string Name { get; private set; }

        public string Extensions { get; set; }
        
        public bool Autoload { get; set; }
    }
}