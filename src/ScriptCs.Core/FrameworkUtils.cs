using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCs
{
    public static class FrameworkUtils
    {
        private static string _frameworkName;
        public static string FrameworkName
        {
            get
            {
                if (_frameworkName == null)
                {
                    // in order to handle the weird behavior of old nuget packages
                    // with NET Standard 2.0, we'll use the entry assemblie if possible
                    var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

                    //Thanks to Dave Glick for this code contribution
                    var frameworkName = assembly.GetCustomAttributes(true)
                       .OfType<System.Runtime.Versioning.TargetFrameworkAttribute>()
                       .Select(x => x.FrameworkName)
                       .FirstOrDefault();
                    _frameworkName = frameworkName;
                }
                return _frameworkName;
            }
        }
    }
}
