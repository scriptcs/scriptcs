using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ScriptCs.Extensions
{
    internal static class MethodInfoExtensions
    {
        internal static IEnumerable<ParameterInfo> GetParametersWithoutExtensions(this MethodInfo method)
        {
            IEnumerable<ParameterInfo> methodParams = method.GetParameters();
            if (method.IsDefined(typeof(ExtensionAttribute), false))
            {
                methodParams = methodParams.Skip(1);
            }

            return methodParams;
        }
    }
}