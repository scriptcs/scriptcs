using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCs.Contracts
{
    [Export(typeof (IScriptCsRecipe))]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    [MetadataAttribute]
    public class ScriptCsRecipe : ExportAttribute, IScriptCsRecipeMetadata
    {
        public ScriptCsRecipe(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }

}
