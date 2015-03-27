using Should;
using Xunit;

namespace ScriptCs.Tests
{
    public class ScriptCsArgsTests
    {
        public class TheParseMethod
        {
            [Fact]
            public void ShouldSetVersionIfPackageVersionNumberFollowsPackageToInstallName()
            {
                string[] args = { "-install", "glimpse.scriptcs", "1.0.1", "-pre" };

                var result = ScriptCsArgs.Parse(args);

                result.Install.ShouldEqual("glimpse.scriptcs");
                result.PackageVersion.ShouldEqual("1.0.1");
                result.AllowPreRelease.ShouldBeTrue();
            }

            [Fact]
            public void ShouldSetVersionIfPackageVersionNumberSpecifiedExplicitly()
            {
                string[] args = { "-install", "glimpse.scriptcs", "-packageversion", "1.0.1", "-pre" };

                var result = ScriptCsArgs.Parse(args);

                result.Install.ShouldEqual("glimpse.scriptcs");
                result.PackageVersion.ShouldEqual("1.0.1");
                result.AllowPreRelease.ShouldBeTrue();
            }
        }
    }
}
