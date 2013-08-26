using System;
using System.Collections.Generic;
using NuGet;

namespace ScriptCs.Hosting.Package
{
    internal class NugetMachineWideSettings : IMachineWideSettings
    {
        private readonly Lazy<IEnumerable<Settings>> _settings;

        public NugetMachineWideSettings()
        {
            var baseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            _settings = new Lazy<IEnumerable<NuGet.Settings>>(() => NuGet.Settings.LoadMachineWideSettings(new PhysicalFileSystem(baseDirectory)));
        }

        public IEnumerable<Settings> Settings
        {
            get
            {
                return _settings.Value;
            }
        }
    }
}