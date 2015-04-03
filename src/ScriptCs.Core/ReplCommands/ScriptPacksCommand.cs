using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            var packContexts = repl.ScriptPackSession.Contexts;

            if (packContexts.IsNullOrEmpty())
            {
                PrintInColor("There are no script packs available in this REPL session");
                return null;
            }

            var importedNamespaces = repl.ScriptPackSession.Namespaces.IsNullOrEmpty() ? repl.Namespaces.ToArray() : repl.Namespaces.Union(repl.ScriptPackSession.Namespaces).ToArray();

            foreach (var packContext in packContexts)
            {
                var contextType = packContext.GetType();
                PrintInColor(contextType.ToString());

                var methods = contextType.GetAllMethods();
                var properties = contextType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

                PrintMethods(methods, importedNamespaces);
                PrintProperties(properties, importedNamespaces);

                _console.WriteLine();
            }

            return null;
        }

        private void PrintInColor(string text)
        {
            var originalColor = _console.ForegroundColor;
            _console.ForegroundColor = ConsoleColor.Yellow;
            _console.WriteLine(text);
            _console.ForegroundColor = originalColor;
        }

        private void PrintMethods(IEnumerable<MethodInfo> methods, string[] importedNamespaces)
        {
            if (!methods.IsNullOrEmpty())
            {
                _console.WriteLine("** Methods **");
                foreach (var method in methods)
                {
                    var methodParams = method.GetParametersWithoutExtensions()
                        .Select(p => string.Format("{0} {1}", GetPrintableType(p.ParameterType, importedNamespaces), p.Name));
                    var methodSignature = string.Format(" - {0} {1}({2})", GetPrintableType(method.ReturnType, importedNamespaces), method.Name,
                        string.Join(", ", methodParams));

                    _console.WriteLine(methodSignature);
                }
                _console.WriteLine();
            }
        }

        private void PrintProperties(IEnumerable<PropertyInfo> properties, string[] importedNamespaces)
        {
            if (!properties.IsNullOrEmpty())
            {
                _console.WriteLine("** Properties **");
                foreach (var property in properties)
                {
                    var propertyBuilder = new StringBuilder(string.Format(" - {0} {1}", GetPrintableType(property.PropertyType, importedNamespaces), property.Name));
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

        private string GetPrintableType(Type type, string[] importedNamespaces)
        {
            if (type.Name == "Void")
            {
                return "void";
            }

            if (type.Name == "Object")
            {
                return "object";
            }

            if (type.IsGenericType)
            {
                return BuildGeneric(type, importedNamespaces);
            }

            var nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null)
            {
                return string.Format("{0}?", GetPrintableType(nullableType, importedNamespaces));
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
                    return string.IsNullOrEmpty(type.FullName) || importedNamespaces.Contains(type.Namespace) ? type.Name : type.FullName;
            }
        }

        private string BuildGeneric(Type type, string[] importedNamespaces)
        {
            var baseName = type.Name.Substring(0, type.Name.IndexOf("`", StringComparison.Ordinal));
            var genericDefinition = new StringBuilder(string.Format("{0}<", baseName));
            var firstArgument = true;
            foreach (var t in type.GetGenericArguments())
            {
                if (!firstArgument)
                {
                    genericDefinition.Append(", ");
                }
                genericDefinition.Append(GetPrintableType(t, importedNamespaces));
                firstArgument = false;
            }
            genericDefinition.Append(">");
            return genericDefinition.ToString();
        }
    }
}