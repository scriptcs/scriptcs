﻿using System;
using System.Runtime.Versioning;
using Should;
using Xunit.Extensions;

namespace ScriptCs.Tests
{
    public class PackageReferenceTests
    {
        public class Constructor
        {
            ///<summary>
            /// Should provide correct version and special version.
            ///</summary>
            [Theory]
            [InlineData("", "", "")]
            [InlineData("1.0.1", "1.0.1", "")]
            [InlineData("1.0.1-alpha", "1.0.1", "alpha")]
            [InlineData("1.0.1-beta-2", "1.0.1", "beta-2")]
            public void MustHandleAllSortsOfDifferentVersionsAndSpecialVersions(string version, string expectedVersion, string expectedSpecialVersion)
            {
                // Arrange
                // Act
                var packageReference = new PackageReference("packageId",
                    new FrameworkName(".NETFramework,Version=v4.0"), version);

                // Assert
                packageReference.Version.ShouldEqual(string.IsNullOrWhiteSpace(expectedVersion) ? new Version() : new Version(expectedVersion));
                
                if (packageReference.SpecialVersion != null)
                    packageReference.SpecialVersion.ShouldEqual(expectedSpecialVersion);
            }
        }
    }
}
