using System;
using System.Reflection;

namespace ScriptCs
{
    internal static class ProfileOptimizationShim
    {
        private static readonly Type profileOptimization = Type.GetType("System.Runtime.ProfileOptimization");

        public static void SetProfileRoot(string directoryPath)
        {
            if (profileOptimization != null)
            {
                profileOptimization
                    .GetMethod("SetProfileRoot", BindingFlags.Public | BindingFlags.Static)
                    .Invoke(null, new object[] { directoryPath });
            }
        }

        public static void StartProfile(string profile)
        {
            if (profileOptimization != null)
            {
                profileOptimization
                    .GetMethod("StartProfile", BindingFlags.Public | BindingFlags.Static)
                    .Invoke(null, new object[] { profile });
            }
        }
    }
}
