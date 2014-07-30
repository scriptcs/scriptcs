using System;

namespace ScriptCs.Hosting
{
    public class TypeResolver : ITypeResolver
    {
        public Type ResolveType(string type)
        {
            return Type.GetType(type);
        }
    }
}