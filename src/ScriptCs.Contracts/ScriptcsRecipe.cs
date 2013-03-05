using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scriptcs.Contracts
{
    [Export(typeof (IScriptcsRecipe))]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    [MetadataAttribute]
    public class ScriptcsRecipe : ExportAttribute, IScriptcsRecipeMetadata
    {
        public ScriptcsRecipe(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }

}
