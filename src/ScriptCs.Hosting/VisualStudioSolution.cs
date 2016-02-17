using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptCs.Contracts;

namespace ScriptCs.Hosting
{
    public class VisualStudioSolution : IVisualStudioSolution
    {
        internal StringBuilder _header;
        internal StringBuilder _projects;
        internal StringBuilder _global;

        public VisualStudioSolution()
        {
            _header = new StringBuilder();
            _projects = new StringBuilder();
            _global = new StringBuilder();
            AddHeader();
        }

        public void AddHeader()
        {
            _header.AppendLine("Microsoft Visual Studio Solution File, Format Version 12.00");
            _header.AppendLine("# Visual Studio 2013");
            _header.AppendLine("VisualStudioVersion = 12.0.30501.0");
            _header.AppendLine("MinimumVisualStudioVersion = 10.0.40219.1");
        }

        public void AddScriptcsProject(string scriptcsPath, string workingPath, string args, bool attach, Guid projectGuid)
        {
            _projects.AppendFormat(@"Project(""{{911E67C6-3D85-4FCE-B560-20A9C3E3FF48}}"") = ""scriptcs"", ""{0}"", ""{{{1}}}""{2}", scriptcsPath, projectGuid, Environment.NewLine);
            _projects.AppendLine("\tProjectSection(DebuggerProjectSystem) = preProject");
            _projects.AppendLine("\t\tPortSupplier = 00000000-0000-0000-0000-000000000000");
            _projects.AppendFormat("\t\tExecutable = {0}{1}", scriptcsPath, Environment.NewLine);
            _projects.AppendLine("\t\tRemoteMachine = localhost");
            _projects.AppendFormat("\t\tStartingDirectory = {0}{1}", workingPath, Environment.NewLine);
            _projects.AppendFormat("\t\tArguments = {0}{1}", args, Environment.NewLine);
            _projects.AppendLine("\t\tEnvironment = Default");
            _projects.AppendLine("\t\tLaunchingEngine = 00000000-0000-0000-0000-000000000000");
            _projects.AppendLine("\t\tUseLegacyDebugEngines = No");
            _projects.AppendLine("\t\tLaunchSQLEngine = No");
            _projects.AppendFormat("\t\tAttachLaunchAction = {0}{1}", attach == true ? "Yes" : "No", Environment.NewLine);
            _projects.AppendLine("\tEndProjectSection");
            _projects.AppendLine("EndProject");
        }

        public void AddProject(string path, string name, Guid guid, string[] files)
        {
            _projects.AppendFormat(@"Project(""{{2150E333-8FDC-42A3-9474-1A3956D46DE8}}"") = ""{0}"", ""{0}"", ""{{{1}}}""{2}", name, guid, Environment.NewLine);
            _projects.AppendLine("\tProjectSection(SolutionItems) = preProject");
            foreach (var file in files)
            {
                if (path == null)
                {
                    _projects.AppendFormat("\t\t{0} = {0}{1}", file, Environment.NewLine);
                }
                else
                {
                    _projects.AppendFormat("\t\t{0}\\{1} = {0}\\{1}{2}", path, file, Environment.NewLine);
                }
            }
            _projects.AppendLine("\tEndProjectSection");
            _projects.AppendLine("EndProject");
        }

        public void AddGlobalHeader(Guid projectGuid)
        {
            _global.AppendLine  ("Global");
            _global.AppendLine  ("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution");
            _global.AppendLine  ("\t\tDebug|Any CPU = Debug|Any CPU");
            _global.AppendLine  ("\tEndGlobalSection");
            _global.AppendLine  ("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");
            _global.AppendFormat("\t\t{{{0}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU{1}", projectGuid, Environment.NewLine);
            _global.AppendLine  ("\tEndGlobalSection");
            _global.AppendLine  ("\tGlobalSection(SolutionProperties) = preSolution");
            _global.AppendLine  ("\t\tHideSolutionNode = FALSE");
            _global.AppendLine  ("\tEndGlobalSection");
        }

        public void AddGlobalNestedProjects(IList<ProjectItem> nestedItems)
        {
            if (nestedItems.Any())
            {
                _global.AppendLine("\tGlobalSection(NestedProjects) = preSolution");
                foreach (var item in nestedItems)
                {
                    _global.AppendFormat("\t\t{{{0}}} = {{{1}}}{2}", item.Project, item.Parent, Environment.NewLine);
                }
                _global.AppendLine("\tEndGlobalSection");
            }
        }

        public void AddGlobal(Guid projectGuid, IList<ProjectItem> nestedItems)
        {
            AddGlobalHeader(projectGuid);
            AddGlobalNestedProjects(nestedItems);
            _global.AppendLine("EndGlobal");
        }

        public override string ToString()
        {
            var solutionBuilder = new StringBuilder();
            solutionBuilder.Append(_header);
            solutionBuilder.Append(_projects);
            solutionBuilder.Append(_global);
            return solutionBuilder.ToString();
        }
    }
}
