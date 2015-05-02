// parameters
var versionSuffix = Environment.GetEnvironmentVariable("VERSION_SUFFIX");
versionSuffix = string.IsNullOrWhiteSpace(versionSuffix) ? "-beta" : versionSuffix;
var msBuildFileVerbosity = (Verbosity)Enum.Parse(typeof(Verbosity), Environment.GetEnvironmentVariable("MSBUILD_FILE_VERBOSITY") ?? "detailed", true);
var nugetVerbosity = Environment.GetEnvironmentVariable("NUGET_VERBOSITY") ?? "quiet";

// solution specific variables
var version = File.ReadAllText("src/CommonAssemblyInfo.cs").Split(new[] { "AssemblyInformationalVersion(\"" }, 2, StringSplitOptions.None).ElementAt(1).Split(new[] { '"' }).First();
var nugetCommand = "scriptcs_packages/NuGet.CommandLine.2.8.3/tools/NuGet.exe";
var solution = "src/ScriptCs.Engines.sln";
var output = "artifacts/output";
var logs = "artifacts/logs";
var packs = new[]
{
    "src/ScriptCs.Engine.Common/ScriptCs.Engine.Common",
    "src/ScriptCs.CSharp/ScriptCs.CSharp",
    "src/ScriptCs.VisualBasic/ScriptCs.VisualBasic",
};

// solution agnostic tasks
var bau = Require<Bau>();

bau
.Task("default").DependsOn("pack")

.Task("logs").Do(() => CreateDirectory(logs))

.MSBuild("clean").DependsOn("logs").Do(msb => Configure(msb, "Clean"))

.Task("clobber").DependsOn("clean").Do(() => DeleteDirectory(output))

.Exec("restore").Do(exec => exec.Run(nugetCommand).With("restore", solution))

.MSBuild("build").DependsOn("clean", "restore", "logs").Do(msb => Configure(msb, "Build"))

.Task("output").Do(() => CreateDirectory(output))

.Task("pack").DependsOn("build", "clobber", "output").Do(() =>
    {
        foreach (var pack in packs)
        {
            File.Copy(pack + ".nuspec", pack + ".nuspec.original", true);
        }

        try
        {
            foreach (var pack in packs)
            {
                File.WriteAllText(pack + ".nuspec", File.ReadAllText(pack + ".nuspec").Replace("0.0.0", version + versionSuffix));

                var project = pack + ".csproj";
                bau.CurrentTask.LogInfo("Packing '" + project + "'...");
                
                new Exec { Name = "pack " + project }
                    .Run(nugetCommand)
                    .With(
                        "pack", project,
                        "-OutputDirectory", output,
                        "-Properties", "Configuration=Release",
                        "-IncludeReferencedProjects",
                        "-Verbosity " + nugetVerbosity)
                    .Execute();
            }
        }
        finally
        {
            foreach (var pack in packs)
            {
                File.Copy(pack + ".nuspec.original", pack + ".nuspec", true);
                File.Delete(pack + ".nuspec.original");
            }
        }
    })

.Run();

void Configure(MSBuild msb, string target)
{
    msb.MSBuildVersion = "net45";
    msb.Solution = solution;
    msb.Targets = new[] { target, };
    msb.Properties = new { Configuration = "Release" };
    msb.MaxCpuCount = -1;
    msb.NodeReuse = false;
    msb.Verbosity = Verbosity.Minimal;
    msb.NoLogo = true;
    msb.FileLoggers.Add(
        new FileLogger
        {
            FileLoggerParameters = new FileLoggerParameters
            {
                PerformanceSummary = true,
                Summary = true,
                Verbosity = msBuildFileVerbosity,
                LogFile = logs + "/" + target + ".log",
            }
        });
}

void CreateDirectory(string name)
{
    if (!Directory.Exists(name))
    {
        Directory.CreateDirectory(name);
        System.Threading.Thread.Sleep(100); // HACK (adamralph): wait for the directory to be created
    }
}

void DeleteDirectory(string name)
{
    if (Directory.Exists(name))
    {
        Directory.Delete(name, true);
    }
}
