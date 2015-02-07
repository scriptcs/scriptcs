using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCs.Tests
{
    using System.IO;
    using Contracts;
    using Xunit;
    using Should;

    public class AssemblyUtilityTests
    {
        public class TheIsManagedAssemblyMethod
        {
            private readonly IAssemblyUtility _assemblyUtility = new AssemblyUtility();

            [Fact]
            public void ShouldReturnTrueWhenThePathIsNotRootedAndDoesNotHaveADllExtension()
            {
                _assemblyUtility.IsManagedAssembly("System.Data").ShouldBeTrue();
            }

            [Fact]
            public void ShouldReturnTrueWhenThePathPointsToAManagedAssembly()
            {
                _assemblyUtility.IsManagedAssembly(typeof(String).Assembly.Location).ShouldBeTrue();
            }
        }
    }
}
