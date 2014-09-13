using System;

namespace ScriptCs
{
    public static class Constants
    {
        [Obsolete("Use IFileSystem instead.")]
        public const string PackagesFile = "packages.config";
        
        [Obsolete("Use IFileSystem instead.")]
        public const string NugetFile = "nuget.config";
        
        [Obsolete("Use IFileSystem instead.")]
        public const string PackagesFolder = "packages";
        
        [Obsolete("Use IFileSystem instead.")]
        public const string BinFolder = "bin";

        [Obsolete("Use IFileSystem instead.")]
        public const string DllCacheFolder = ".cache";

        public const string ConfigFilename = "scriptcs.opts";

        public const string DefaultRepositoryUrl = "https://nuget.org/api/v2/";
    }
}
