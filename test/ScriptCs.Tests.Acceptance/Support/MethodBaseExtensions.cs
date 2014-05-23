namespace ScriptCs.Tests.Acceptance.Support
{
    using System.Reflection;
    using ScriptCs;

    public static class MethodBaseExtensions
    {
        public static string GetFullName(this MethodBase method)
        {
            Guard.AgainstNullArgument("method", method);

            return method.DeclaringType == null
                ? method.Name
                : string.Concat(method.DeclaringType.FullName, ".", method.Name);
        }
    }
}
