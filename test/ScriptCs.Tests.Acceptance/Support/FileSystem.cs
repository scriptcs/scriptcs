namespace ScriptCs.Tests.Acceptance.Support
{
    using System;
    using System.Globalization;
    using System.IO;
    using Should;

    // NOTE (adamralph): difficult to believe the retry stuff is required, but it is. System.IO and the filesystem race.
    public static class FileSystem
    {
        public static void EnsureDirectoryCreated(string path)
        {
            if (Directory.Exists(path))
            {
                return;
            }

            var timeout = 0.05d;

            var createTimeout = DateTime.Now.AddSeconds(timeout);
            while (true)
            {
                try
                {
                    Directory.CreateDirectory(path);

                    var existsTimeout = DateTime.Now.AddSeconds(timeout);
                    while (true)
                    {
                        if (Directory.Exists(path))
                        {
                            break;
                        }

                        if (DateTime.Now < existsTimeout)
                        {
                            continue;
                        }

                        throw new IOException(
                            string.Format(CultureInfo.InvariantCulture, "Failed to create directory '{0}'", path));
                    }
                }
                catch (Exception)
                {
                    if (DateTime.Now < createTimeout)
                    {
                        continue;
                    }

                    throw;
                }

                break;
            }
        }

        public static void EnsureDirectoryDeleted(string path)
        {
            if (!Directory.Exists(path))
            {
                return;
            }

            var timeout = 0.05d;

            var deleteTimeout = DateTime.Now.AddSeconds(timeout);
            while (true)
            {
                try
                {
                    Directory.Delete(path, true);

                    var goneTimeout = DateTime.Now.AddSeconds(timeout);
                    while (true)
                    {
                        if (!Directory.Exists(path))
                        {
                            break;
                        }

                        if (DateTime.Now < goneTimeout)
                        {
                            continue;
                        }

                        throw new IOException(
                            string.Format(CultureInfo.InvariantCulture, "Failed to delete directory '{0}'", path));
                    }
                }
                catch (Exception)
                {
                    if (DateTime.Now < deleteTimeout)
                    {
                        continue;
                    }

                    throw;
                }

                break;
            }
        }

        public static void EnsureFileDeleted(string fileName)
        {
            File.Delete(fileName);
            File.Exists(fileName).ShouldBeFalse(fileName + " should be deleted");
        }
    }
}
