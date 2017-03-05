using System;
using System.IO;
using System.Linq;
using ScriptCs.Contracts;
using ScriptCs.PaketDirective;

namespace ScriptCs
{
    public class PaketLoader : IPaketLoader
    {
        private IConsole _console;
        private IFileSystem _fileSystem;
        private IFilePreProcessor _filePreProcessor;

        public PaketLoader(IConsole console, IFileSystem fileSystem, IFilePreProcessor filePreProcessor)
        {
            _filePreProcessor = filePreProcessor;
            _fileSystem = fileSystem;
            _console = console;
        }

        public void Load(FilePreProcessorResult preProcessorResult)
        {
            var paketRefs = preProcessorResult.CustomReferences.Where(r => r.StartsWith(Constants.PaketPrefix)).Select(r => r.Substring(Constants.PaketPrefix.Length)).ToArray();

            if (!paketRefs.Any())
                return;

            var info = Path.Combine(_fileSystem.CurrentDirectory, "Repl.csx");
            var scriptFileBeingProcessed = new FileInfo(info);

            Func<string, string> prefixWithMonoIfNeeded =
                commandLine => FrameworkUtils.IsMono ? "mono " + commandLine : commandLine;
            var frameworkName = FrameworkUtils.FrameworkName;

            //TODO: hacky but works for now.
            var targetFramework = "net" + frameworkName.Substring(frameworkName.IndexOf("=v") + 2).Replace(".", "");

            var prioritizedSearchPaths = new[] {new DirectoryInfo(_fileSystem.HostBin)};

            var result =
                PaketShim.ResolveLoadScript(scriptFileBeingProcessed, paketRefs, prefixWithMonoIfNeeded, targetFramework, prioritizedSearchPaths);

            if (result.IsSolved)
            {
                var solved = (ReferenceLoading.PaketHandler.ReferenceLoadingResult.Solved) result;
                var loadingScript = solved.loadingScript;
                var loadingScriptCodeResult = _filePreProcessor.ProcessFile(loadingScript);
                preProcessorResult.AssemblyReferences.AddRange(loadingScriptCodeResult.AssemblyReferences);

                // todo: decide what we should do with solved.additionalIncludeFolders
                // (contains list of folders we should search for csx files for subsequent #load statements)
                // see https://github.com/forki/visualfsharp/blob/paket/tests/fsharpqa/Source/InteractiveSession/Paket/PaketWithRemoteFile/UseGlobbing.fsx
                return;
            }

            _console.ForegroundColor = ConsoleColor.Red;
            // failure: we should test the different cases

            if (result.IsPackageManagerNotFound)
            {
                var packageManagerNotFound =
                    (ReferenceLoading.PaketHandler.ReferenceLoadingResult.PackageManagerNotFound) result;

                // we could print those properties:
                //packageManagerNotFound.implicitIncludeDir
                //packageManagerNotFound.userProfile
                _console.WriteLine("Package manager not found");
            }
            else if (result.IsPackageResolutionFailed)
            {
                var packageResolutionFailed =
                    (ReferenceLoading.PaketHandler.ReferenceLoadingResult.PackageResolutionFailed) result;
                _console.WriteLine(
                    string.Format(@"Package resolution failed: 
                        toolpath: {0}
                        workingdir: {1}
                        message: {2}",
                        packageResolutionFailed.toolPath,
                        packageResolutionFailed.workingDir,
                        packageResolutionFailed.msg)
                    );
            }
            else
            {
                _console.WriteLine("Unknown error:" + result.ToString());
            }
            _console.ResetColor();
        }
    }
}