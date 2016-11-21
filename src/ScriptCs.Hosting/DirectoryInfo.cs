using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCs.Hosting
{
    public class DirectoryInfo
    {
        public DirectoryInfo()
        {
            Guid = Guid.NewGuid();
            Files = new List<string>();
            Directories = new Dictionary<string, DirectoryInfo>();
        }

        public Guid Guid { get; private set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string FullPath { get; set; }
        public List<string> Files { get; private set; }
        public Dictionary<string, DirectoryInfo> Directories { get; private set; }
    }
}
