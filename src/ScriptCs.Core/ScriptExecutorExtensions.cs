using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ScriptCs.Contracts;

namespace ScriptCs
{
    public static class ScriptExecutorExtensions
    {
        public static void ImportNamespaces(this IScriptExecutor executor, params Type[] typesFromReferencedAssembly)
        {
            Guard.AgainstNullArgument("executor", executor);
            Guard.AgainstNullArgument("typesFromReferencedAssembly", typesFromReferencedAssembly);

            var namespaces = typesFromReferencedAssembly.Select(t => t.Namespace);
            executor.ImportNamespaces(namespaces.ToArray());
        }

        public static void ImportNamespace<T>(this IScriptExecutor executor)
        {
            Guard.AgainstNullArgument("executor", executor);

            executor.ImportNamespaces(typeof(T));
        }

        public static void AddReferences(this IScriptExecutor executor, params Type[] typesFromReferencedAssembly)
        {
            Guard.AgainstNullArgument("executor", executor);
            Guard.AgainstNullArgument("typeFromReferencedAssembly", typesFromReferencedAssembly);

            var paths = typesFromReferencedAssembly.Select(t => t.Assembly.Location);

            executor.AddReferences(paths.ToArray());
        }

        public static void AddReference<T>(this IScriptExecutor executor)
        {
            Guard.AgainstNullArgument("executor", executor);

            executor.AddReferences(typeof(T));
        }

        public static void AddReferenceAndImportNamespaces(this IScriptExecutor executor, Type[] types)
        {
            Guard.AgainstNullArgument("executor", executor);

            executor.AddReferences(types);
            executor.ImportNamespaces(types);
        }
    }
}
