using System.Security.Cryptography;
using System.Text;

namespace ScriptCs.Tests.Acceptance.Support
{
    using System.Reflection;
    using ScriptCs;

    public static class MethodBaseExtensions
    {
        public static string GetFullName(this MethodBase method)
        {
            Guard.AgainstNullArgument(nameof(method), method);

            return method.DeclaringType == null
                ? method.Name
                : string.Concat(GenerateSimpleHash(method.DeclaringType.FullName), ".", method.Name);
        }

        /// <summary>
        /// Windows cannot handle long path, so create a short path
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string GenerateSimpleHash(string name)
        {
            // use a hash instead of random characters
            // so we have some consistency in debugging folders
            using (var md5Hash = MD5.Create())
            {
                var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(name));
                var hash = new StringBuilder(10);

                for (var i = 0; i < 5; i++)
                {
                    hash.Append(data[i].ToString("x2"));
                }

                return hash.ToString();
            }
        }
    }
}
