using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Should;
using Xunit;
using System.IO;
using ScriptCs.Contracts;

namespace ScriptCs.Hosting.Tests
{
    public class VisualStudioSolutionTests
    {
        public class TheConstructor
        {
            private VisualStudioSolution _solution = new VisualStudioSolution();

            [Fact]
            public void ShouldInitializeVariables()
            {
                _solution._header.ShouldNotBeNull();
                _solution._projects.ShouldNotBeNull();
                _solution._global.ShouldNotBeNull();
            }

            [Fact]
            public void ShouldAppendTheHeader()
            {
                var headerBuilder = new StringBuilder();
                headerBuilder.AppendLine("Microsoft Visual Studio Solution File, Format Version 12.00");
                headerBuilder.AppendLine("# Visual Studio 2013");
                headerBuilder.AppendLine("VisualStudioVersion = 12.0.30501.0");
                headerBuilder.AppendLine("MinimumVisualStudioVersion = 10.0.40219.1");
                _solution._header.ToString().ShouldEqual(headerBuilder.ToString());
            }
        }

        public class TheAddGlobalHeaderMethod
        {
            private Guid _projectGuid = Guid.NewGuid();
            private VisualStudioSolution _builder = new VisualStudioSolution();

            [Fact]
            public void ShouldAppendTheGlobalHeader()
            {
                _builder.AddGlobalHeader(_projectGuid);
                var globalBuilder = new StringBuilder();
                globalBuilder.AppendLine("Global");
                globalBuilder.AppendLine("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution");
                globalBuilder.AppendLine("\t\tDebug|Any CPU = Debug|Any CPU");
                globalBuilder.AppendLine("\tEndGlobalSection");
                globalBuilder.AppendLine("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");
                globalBuilder.AppendFormat("\t\t{{{0}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU{1}", _projectGuid, Environment.NewLine);
                globalBuilder.AppendLine("\tEndGlobalSection");
                globalBuilder.AppendLine("\tGlobalSection(SolutionProperties) = preSolution");
                globalBuilder.AppendLine("\t\tHideSolutionNode = FALSE");
                globalBuilder.AppendLine("\tEndGlobalSection");
                globalBuilder.ToString().ShouldEqual(_builder._global.ToString());
            }
        }

        public class TheAddGlobalNestedProjectsMethod
        {
            [Fact]
            public void ShouldAppenedGlobalSectionEntriesForEachProject()
            {
                var builder = new VisualStudioSolution();

                var nestedItems = new List<ProjectItem>();
                var a = Guid.NewGuid();
                var b = Guid.NewGuid();
                var c = Guid.NewGuid();

                nestedItems.Add(new ProjectItem(a, b));
                nestedItems.Add(new ProjectItem(b, c));
                builder.AddGlobalNestedProjects(nestedItems);
                var nestedBuilder = new StringBuilder();
                nestedBuilder.AppendLine("\tGlobalSection(NestedProjects) = preSolution");
                nestedBuilder.AppendFormat("\t\t{{{0}}} = {{{1}}}{2}", nestedItems[0].Project, nestedItems[0].Parent,
                    Environment.NewLine);
                nestedBuilder.AppendFormat("\t\t{{{0}}} = {{{1}}}{2}", nestedItems[1].Project, nestedItems[1].Parent,
                    Environment.NewLine);
                nestedBuilder.AppendLine("\tEndGlobalSection");
                builder._global.ToString().Contains(nestedBuilder.ToString());
            }
        }

        public class TheAddScriptcsProjectMethod
        {
            private VisualStudioSolution _builder = new VisualStudioSolution();
            private const string _scriptcsPath = "scriptcs.exe";
            private const string _workingPath = "working";
            private const string _args = "test.csx";
            private const bool _attach = true;
            private Guid _projectGuid = Guid.NewGuid();
            private string _projects;

            public TheAddScriptcsProjectMethod()
            {
                _builder.AddScriptcsProject(_scriptcsPath, _workingPath, _args, _attach, _projectGuid);
                _projects = _builder._projects.ToString();
            }

            [Fact]
            public void ShouldAppendTheScriptcsProjectSection()
            {
                var projectBuilder = new StringBuilder();

                projectBuilder.AppendFormat(@"Project(""{{911E67C6-3D85-4FCE-B560-20A9C3E3FF48}}"") = ""scriptcs"", ""{0}"", ""{{{1}}}""{2}", _scriptcsPath, _projectGuid, Environment.NewLine);
                projectBuilder.AppendLine("\tProjectSection(DebuggerProjectSystem) = preProject");
                projectBuilder.AppendLine("\t\tPortSupplier = 00000000-0000-0000-0000-000000000000");
                projectBuilder.AppendFormat("\t\tExecutable = {0}{1}", _scriptcsPath, Environment.NewLine);
                projectBuilder.AppendLine("\t\tRemoteMachine = localhost");
                projectBuilder.AppendFormat("\t\tStartingDirectory = {0}{1}", _workingPath, Environment.NewLine);
                projectBuilder.AppendFormat("\t\tArguments = {0}{1}", _args, Environment.NewLine);
                projectBuilder.AppendLine("\t\tEnvironment = Default");
                projectBuilder.AppendLine("\t\tLaunchingEngine = 00000000-0000-0000-0000-000000000000");
                projectBuilder.AppendLine("\t\tUseLegacyDebugEngines = No");
                projectBuilder.AppendLine("\t\tLaunchSQLEngine = No");
                projectBuilder.AppendFormat("\t\tAttachLaunchAction = {0}{1}", _attach ? "Yes" : "No", Environment.NewLine);
                projectBuilder.AppendLine("\tEndProjectSection");
                projectBuilder.AppendLine("EndProject");
                _projects.ShouldEqual(projectBuilder.ToString());
            }

            [Fact]
            public void ShouldSetTheProjectElementScriptcsPath()
            {
                _projects.Contains(string.Format(@"Project(""{{911E67C6-3D85-4FCE-B560-20A9C3E3FF48}}"") = ""scriptcs"", ""{0}""", _scriptcsPath));
            }

            [Fact]
            public void ShouldSetTheExecutable()
            {
                _projects.Contains(string.Format("Executable = {0}", _scriptcsPath));
            }

            [Fact]
            public void ShouldSetTheStartingDirectory()
            {
                _projects.Contains(string.Format("StartingDirectory = {0}", _workingPath));
            }

            [Fact]
            public void ShouldSetTheArguements()
            {
                _projects.Contains(string.Format("Arguments = {0}", _args));
            }

            [Fact]
            public void ShouldSetTheAttachAction()
            {
                _projects.Contains(string.Format("AttachLaunchAction = {0}", _attach ? "Yes" : "No"));
            }
        }

        public class TheToStringMethod
        {
            [Fact]
            public void BuildsTheSolution()
            {
                var builder = new VisualStudioSolution();
                builder._header = new StringBuilder("A");
                builder._projects = new StringBuilder("B");
                builder._global = new StringBuilder("C");
                var solution = builder.ToString();
                solution.ShouldEqual("ABC");
            }
        }
    }
}
