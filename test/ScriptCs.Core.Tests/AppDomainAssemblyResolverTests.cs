using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ScriptCs.Tests
{
    public class AppDomainAssemblyResolverTests
    {
        public class TheConstructor {
            [Fact]
            public void ShouldSubscribeToTheResolveEvent()
            {
            } 
        }

        public class TheInitializeMethod
        {
            [Fact]
            public void ShouldAddHostAssemblyPaths()
            {
            }

            [Fact]
            public void ShouldAddModuleAssemblyPaths()
            {
            }

            [Fact]
            public void ShouldAddScriptPackAssemblyPaths()
            {
            }
        }

        public class TheAddAssemblyPathsMethod
        {
            [Fact]
            public void ShouldRetrieveTheMappedAssemblyInfo()
            {
            }

            [Fact]
            public void ShouldRegisterTheAssemblyIfTheAssemblyDoesNotExist()
            {
            }

            [Fact]
            public void ShouldOverrideIfTheAssemblyVersionIsGreaterThanTheMappedAssemblyAndItWasNotLoaded()
            {
            }

            [Fact]
            public void ShouldLogWhenTheAssemblyIsMapped()
            {
            }

            [Fact]
            public void ShouldWarnIfTheAssemblyVersionIsGreaterThanTheMappedAssemblyAndItWasLoaded()
            {
            }

            [Fact]
            public void ShouldNotOverrideIfTheAssemblyVersionIsGreaterThanTheMappedAssemblyAndItWasLoaded()
            {
            }
           
            [Fact]
            public void ShouldNotOverrideTheAssemblyVersionIsLessThenOrEqualToTheMappedAssemblyAndItWasLoaded()
            {
            }
        }

        public class TheAssemblyResolveMethod
        {
            [Fact]
            public void ShouldRetrievedTheMappedAssemblyInfo()
            {
            }

            [Fact]
            public void ShouldLoadTheAssemblyIfTheMappedAssemblyInfoExistsAndItHasNotBeenLoaded()
            {
            }

            [Fact]
            public void ShouldLogTheAssemblyThatIsBeingResolved()
            {
            }

            [Fact]
            public void ShouldReturnTheAssemblyIfItWasLoaded()
            {
            }
        }
    }
}
