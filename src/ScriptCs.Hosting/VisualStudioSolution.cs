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
        internal StringBuilder Header { get; set; }
        internal StringBuilder Projects { get; set; }
        internal StringBuilder Global { get; set; }

        public VisualStudioSolution()
        {
            Header = new StringBuilder();
            Projects = new StringBuilder();
            Global = new StringBuilder();
            AddHeader();
        }

        public void AddHeader()
        {
            Header.AppendLine("Microsoft Visual Studio Solution File, Format Version 12.00");
            Header.AppendLine("# Visual Studio 2013");
            Header.AppendLine("VisualStudioVersion = 12.0.30501.0");
            Header.AppendLine("MinimumVisualStudioVersion = 10.0.40219.1");
        }

        public void AddScriptcsProject(string scriptcsPath, string workingPath, string args, bool attach, Guid projectGuid)
        {
            Projects.AppendFormat(@"Project(""{{911E67C6-3D85-4FCE-B560-20A9C3E3FF48}}"") = ""scriptcs"", ""{0}"", ""{{{1}}}""{2}", scriptcsPath, projectGuid, Environment.NewLine);
            Projects.AppendLine("\tProjectSection(DebuggerProjectSystem) = preProject");
            Projects.AppendLine("\t\tPortSupplier = 00000000-0000-0000-0000-000000000000");
            Projects.AppendFormat("\t\tExecutable = {0}{1}", scriptcsPath, Environment.NewLine);
            Projects.AppendLine("\t\tRemoteMachine = localhost");
            Projects.AppendFormat("\t\tStartingDirectory = {0}{1}", workingPath, Environment.NewLine);
            Projects.AppendFormat("\t\tArguments = {0}{1}", args, Environment.NewLine);
            Projects.AppendLine("\t\tEnvironment = Default");
            Projects.AppendLine("\t\tLaunchingEngine = 00000000-0000-0000-0000-000000000000");
            Projects.AppendLine("\t\tUseLegacyDebugEngines = No");
            Projects.AppendLine("\t\tLaunchSQLEngine = No");
            Projects.AppendFormat("\t\tAttachLaunchAction = {0}{1}", attach == true ? "Yes" : "No", Environment.NewLine);
            Projects.AppendLine("\tEndProjectSection");
            Projects.AppendLine("EndProject");
        }

        public void AddProject(string path, string name, Guid guid, string[] files)
        {
            Projects.AppendFormat(@"Project(""{{2150E333-8FDC-42A3-9474-1A3956D46DE8}}"") = ""{0}"", ""{0}"", ""{{{1}}}""{2}", name, guid, Environment.NewLine);
            Projects.AppendLine("\tProjectSection(SolutionItems) = preProject");
            foreach (var file in files)
            {
                if (path == null)
                {
                    Projects.AppendFormat("\t\t{0} = {0}{1}", file, Environment.NewLine);
                }
                else
                {
                    Projects.AppendFormat("\t\t{0}\\{1} = {0}\\{1}{2}", path, file, Environment.NewLine);
                }
            }
            Projects.AppendLine("\tEndProjectSection");
            Projects.AppendLine("EndProject");
        }

        public void AddGlobalHeader(Guid projectGuid)
        {
            Global.AppendLine("Global");
            Global.AppendLine("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution");
            Global.AppendLine("\t\tDebug|Any CPU = Debug|Any CPU");
            Global.AppendLine("\tEndGlobalSection");
            Global.AppendLine("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");
            Global.AppendFormat("\t\t{{{0}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU{1}", projectGuid, Environment.NewLine);
            Global.AppendLine("\tEndGlobalSection");
            Global.AppendLine("\tGlobalSection(SolutionProperties) = preSolution");
            Global.AppendLine("\t\tHideSolutionNode = FALSE");
            Global.AppendLine("\tEndGlobalSection");
        }

        public void AddGlobalNestedProjects(IList<ProjectItem> nestedItems)
        {
            if (nestedItems.Any())
            {
                Global.AppendLine("\tGlobalSection(NestedProjects) = preSolution");
                foreach (var item in nestedItems)
                {
                    Global.AppendFormat("\t\t{{{0}}} = {{{1}}}{2}", item.Project, item.Parent, Environment.NewLine);
                }
                Global.AppendLine("\tEndGlobalSection");
            }
        }

        public void AddGlobal(Guid projectGuid, IList<ProjectItem> nestedItems)
        {
            AddGlobalHeader(projectGuid);
            AddGlobalNestedProjects(nestedItems);
            Global.AppendLine("EndGlobal");
        }

        public override string ToString()
        {
            var solutionBuilder = new StringBuilder();
            solutionBuilder.Append(Header);
            solutionBuilder.Append(Projects);
            solutionBuilder.Append(Global);
            return solutionBuilder.ToString();
        }
    }
}
