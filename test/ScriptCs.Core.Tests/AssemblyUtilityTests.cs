using System;
using ScriptCs.Contracts;
using Should;
using Xunit;

namespace ScriptCs.Tests
{
    public class AssemblyUtilityTests
    {
        public class TheIsManagedAssemblyMethod
        {
            private readonly IAssemblyUtility _assemblyUtility = new AssemblyUtility();

            [Fact]
            public void ShouldReturnFalseWhenThePathDoesNotPointToAManagedAssembly()
            {
                _assemblyUtility.IsManagedAssembly("ScriptCs.Core.Tests.dll.config").ShouldBeFalse();
            }

            [Fact]
            public void ShouldReturnTrueWhenThePathPointsToAManagedAssembly()
            {
                _assemblyUtility.IsManagedAssembly(typeof(String).Assembly.Location).ShouldBeTrue();
            }
        }
    }
}
