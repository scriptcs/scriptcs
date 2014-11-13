using System;
using System.Runtime.Versioning;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Xunit;
using Should;
using Xunit;
using Xunit.Extensions;

namespace ScriptCs.Tests
{
    public class PackageReferenceTests
    {
        public class Constructor
        {
            private readonly IFixture _fixture =  new Fixture();

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

            ///<summary>
            /// Should provide correct version and special version, given 0.5.0-beta-2
            ///</summary>
            [Theory]
            [AutoData]
            public void MustHandleAllSortsOfDifferentSpecialVersions(string specialVersion)
            {
                // Arrange
                var version = "0.5.0";

                // Act
                var packageReference = new PackageReference(_fixture.Create<string>(),
                    new FrameworkName(".NETFramework,Version=v4.0"), version + "-" + specialVersion);

                // Assert
                packageReference.Version.ShouldEqual(new Version("0.5.0"));
                packageReference.SpecialVersion.ShouldEqual(specialVersion);
            }

        }
    }
}