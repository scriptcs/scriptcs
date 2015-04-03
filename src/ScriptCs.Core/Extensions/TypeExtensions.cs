using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ScriptCs.Extensions
{
    internal static class TypeExtensions
    {
        public static IEnumerable<MethodInfo> GetExtensionMethods(this Type type, Assembly assembly)
        {
            Guard.AgainstNullArgument("type", type);
            Guard.AgainstNullArgument("assembly", assembly);

            return assembly.GetExportedTypes()
                .Where(x => !x.IsGenericType && !x.IsNested && x.IsSealed)
                .SelectMany(x => x.GetMethods(BindingFlags.Static | BindingFlags.Public))
                .Where(x => x.IsDefined(typeof(ExtensionAttribute), false))
                .Where(x => x.GetParameters()[0].ParameterType == type);
        }
    }
}
