using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptCs.Contracts;

namespace ScriptCs.Hosting
{
    public class VisualStudioSolutionWriter : IVisualStudioSolutionWriter
    {
        internal DirectoryInfo _root; 
        private Guid _nullGuid = new Guid();

        public string WriteSolution(IFileSystem fs, string script, IVisualStudioSolution solution, IList<ProjectItem> nestedItems = null)
        {
            if (nestedItems == null)
            {
                nestedItems = new List<ProjectItem>();
            }

            var launcher = Path.Combine(fs.TempPath, "launcher-" + Guid.NewGuid().ToString() + ".sln");

            if (fs.FileExists(launcher))
            {
                fs.FileDelete(launcher);
            }

            var currentDir = fs.CurrentDirectory;
            var scriptcsPath = Path.Combine(fs.HostBin, "scriptcs.exe");
            var scriptcsArgs = string.Format("{0} -debug -loglevel info", script);
            _root = new DirectoryInfo { Name = "Solution Items", FullPath = currentDir};
            var projectGuid = Guid.NewGuid();

            solution.AddScriptcsProject(scriptcsPath, currentDir, scriptcsArgs, false, projectGuid);
            GetDirectoryInfo(fs, currentDir, _root);
            AddDirectoryProject(solution, fs, _root, _nullGuid, nestedItems);
            solution.AddGlobal(projectGuid, nestedItems);
            fs.WriteToFile(launcher, solution.ToString());
            return launcher;
        }

        private void AddDirectoryProject(IVisualStudioSolution solution, IFileSystem fs, DirectoryInfo currentDirectory, Guid parent, IList<ProjectItem> nestedItems)
        {
            solution.AddProject(currentDirectory.FullPath, currentDirectory.Name, currentDirectory.Guid, currentDirectory.Files.ToArray());
            foreach (DirectoryInfo dir in currentDirectory.Directories.Values)
            {
                AddDirectoryProject(solution, fs, dir, currentDirectory.Guid, nestedItems);
            }
            if (parent != _nullGuid)
            {
                nestedItems.Add(new ProjectItem(currentDirectory.Guid, parent));
            }
        }

        private void GetDirectoryInfo(IFileSystem fs, string currentDir, DirectoryInfo root)
        {
            IEnumerable<string> files;
            var packagesFolder = Path.Combine(currentDir, fs.PackagesFolder);
            files = fs.EnumerateFilesAndDirectories(currentDir, "*.csx").Where(
                f => f.StartsWith(packagesFolder).Equals(false));

            var skip = currentDir.Length;

            foreach (var file in files)
            {
                var pruned = file.Substring(skip + 1);
                var segments = pruned.Split(Path.DirectorySeparatorChar);
                if (segments.Length == 1)
                {
                    root.Files.Add(segments[0]);
                }
                else
                {
                    var currentDirectory = root;
                    var path = "";
                    for (int i = 0; i < segments.Length - 1; i++)
                    {
                        var segment = segments[i];
                        path = path + segment + @"\";
                        if (!currentDirectory.Directories.ContainsKey(segment))
                        {
                            var newDirectory = new DirectoryInfo { Name = segment, Path=path, FullPath = Path.GetDirectoryName(file)};
                            currentDirectory.Directories[segment] = newDirectory;
                            currentDirectory = newDirectory;
                        }
                        else
                        {
                            currentDirectory = currentDirectory.Directories[segment];
                        }
                    }
                    currentDirectory.Files.Add(segments[segments.Length - 1]);
                }
            }
        }
    }
}
