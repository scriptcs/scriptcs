using System;
using System.Runtime.Versioning;
using ScriptCs.Package;
using Should;
using Xunit;

namespace ScriptCs.Tests
{
    public class PackageReferenceTests
    {
        public class Constructor
        {
            [Fact]
            public void WhenStringVersionIsEmptyVersionShouldBeEmpty()
            {
                var p = new PackageReference("packageId", new FrameworkName(".NETFramework,Version=v4.0"), "");
                p.Version.ShouldEqual(new Version());
            }

            [Fact]
            public void WhenStringVersionHasNormalValueVersionShouldBeEqualToThat()
            {
                var p = new PackageReference("packageId", new FrameworkName(".NETFramework,Version=v4.0"), "1.0.1");
                p.Version.ShouldEqual(new Version("1.0.1"));
            }

            [Fact]
            public void WhenStringVersionHasSpecialValueVersionShouldBeEqualToNormalAndSpecialVersionShouldBeSet()
            {
                var p = new PackageReference("packageId", new FrameworkName(".NETFramework,Version=v4.0"), "1.0.1-alpha");
                p.Version.ShouldEqual(new Version("1.0.1"));
                p.SpecialVersion.ShouldEqual("alpha");
            }
        }
    }
}