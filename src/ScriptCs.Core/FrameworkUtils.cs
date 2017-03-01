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
                    //Thanks to Dave Glick for this code contribution
                    var frameworkName = Assembly.GetExecutingAssembly().GetCustomAttributes(true)
                       .OfType<System.Runtime.Versioning.TargetFrameworkAttribute>()
                       .Select(x => x.FrameworkName)
                       .FirstOrDefault();
                    _frameworkName = frameworkName;
                }
                return _frameworkName;
            }
        }

        private static object _isMono;
        public static bool IsMono
        {
            get
            {
                if (_isMono == null)
                {
                    _isMono = Type.GetType("Mono.Runtime") != null;
                }
                return (bool) _isMono;
            }
        }
    }
}
