using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using ScriptCs.Contracts;
using ScriptCs.Extensions;

namespace ScriptCs.ReplCommands
{
    public class ScriptPacksCommand : IReplCommand
    {
        private readonly IConsole _console;

        public ScriptPacksCommand(IConsole console)
        {
            _console = console;
        }

        public string Description
        {
            get { return "Displays information about script packs available in the REPL session"; }
        }

        public string CommandName
        {
            get { return "scriptpacks"; }
        }

        public object Execute(IRepl repl, object[] args)
        {
            var packContexts = (repl.ScriptPackSession.Contexts ?? Enumerable.Empty<IScriptPackContext>()).ToArray();

            if (!packContexts.Any())
            {
                PrintInYellow("There are no script packs available in this REPL session");
                return null;
            }

            var importedNamespaces = repl.Namespaces.Union(repl.ScriptPackSession.Namespaces).ToArray();

            foreach (var packContext in packContexts)
            {
                var contextType = packContext.GetType();
                PrintInYellow(contextType.ToString());

                var methods = contextType
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(m => !m.IsSpecialName)
                    .Union(contextType.GetExtensionMethods(contextType.Assembly))
                    .ToArray();

                var properties = contextType
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

                PrintMethods(methods, importedNamespaces);
                PrintProperties(properties, importedNamespaces);

                _console.WriteLine();
            }

            return null;
        }

        private void PrintInYellow(string text)
        {
            var originalColor = _console.ForegroundColor;
            _console.ForegroundColor = ConsoleColor.Yellow;
            _console.WriteLine(text);
            _console.ForegroundColor = originalColor;
        }

        private void PrintMethods(MethodInfo[] methods, string[] importedNamespaces)
        {
            if (methods.Any())
            {
                _console.WriteLine("** Methods **");
                foreach (var method in methods)
                {
                    var methodParams = method.GetParameters()
                        .Skip((method.IsDefined(typeof(ExtensionAttribute), false) ? 1 : 0))
                        .Select(p => string.Format(
                            "{0} {1}", GetDisplayName(p.ParameterType, importedNamespaces), p.Name));

                    var methodSignature = string.Format(
                        " - {0} {1}({2})",
                        GetDisplayName(method.ReturnType, importedNamespaces),
                        method.Name,
                        string.Join(", ", methodParams));

                    _console.WriteLine(methodSignature);
                }

                _console.WriteLine();
            }
        }

        private void PrintProperties(PropertyInfo[] properties, string[] importedNamespaces)
        {
            if (properties.Any())
            {
                _console.WriteLine("** Properties **");
                foreach (var property in properties)
                {
                    var propertyBuilder = new StringBuilder(
                        string.Format(" - {0} {1}", GetDisplayName(property.PropertyType, importedNamespaces), property.Name));

                    propertyBuilder.Append(" {");

                    if (property.GetGetMethod() != null)
                    {
                        propertyBuilder.Append(" get;");
                    }

                    if (property.GetSetMethod() != null)
                    {
                        propertyBuilder.Append(" set;");
                    }

                    propertyBuilder.Append(" }");

                    _console.WriteLine(propertyBuilder.ToString());
                }
            }
        }

        private static string GetDisplayName(Type type, string[] importedNamespaces)
        {
            switch (type.Name)
            {
                case "Void":
                    return "void";
                case "Object":
                    return "object";
            }

            if (type.IsGenericType)
            {
                var genericArguments = type.GenericTypeArguments
                    .Select(typeArgument => GetDisplayName(typeArgument, importedNamespaces))
                    .ToArray();

                return string.Concat(
                    type.Name.Substring(0, type.Name.IndexOf("`", StringComparison.Ordinal)),
                    "<",
                    string.Join(", ", genericArguments),
                    ">");
            }

            var nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null)
            {
                return string.Concat(GetDisplayName(nullableType, importedNamespaces), "?");
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return "bool";
                case TypeCode.Byte:
                    return "byte";
                case TypeCode.Char:
                    return "char";
                case TypeCode.Decimal:
                    return "decimal";
                case TypeCode.Double:
                    return "double";
                case TypeCode.Int16:
                    return "short";
                case TypeCode.Int32:
                    return "int";
                case TypeCode.Int64:
                    return "long";
                case TypeCode.SByte:
                    return "sbyte";
                case TypeCode.Single:
                    return "Single";
                case TypeCode.String:
                    return "string";
                case TypeCode.UInt16:
                    return "UInt16";
                case TypeCode.UInt32:
                    return "UInt32";
                case TypeCode.UInt64:
                    return "UInt64";
                default:
                    return string.IsNullOrEmpty(type.FullName) || importedNamespaces.Contains(type.Namespace)
                        ? type.Name
                        : type.FullName;
            }
        }
    }
}
