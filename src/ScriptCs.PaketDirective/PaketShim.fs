namespace ScriptCs.PaketDirective
open System
open System.IO
module PaketShim =
    open ReferenceLoading.PaketHandler

    let ResolveLoadScript
          ( scriptFile:              FileInfo
          , packageManagerTextLines: string array
          , alterToolPath:           Func<string,string>
          , targetFramework:         string
          , prioritizedSearchPaths:  DirectoryInfo seq
          ) =
        ReferenceLoading.PaketHandler.Internals.ResolvePackages
            targetFramework
            (MakePackageManagerCommand "csx")
            (fun workDir -> ReferenceLoading.PaketHandler.GetPaketLoadScriptLocation workDir (Some targetFramework) "main.group.csx")
            alterToolPath.Invoke
            prioritizedSearchPaths
            scriptFile.Directory.FullName
            scriptFile.Name
            (Array.toList packageManagerTextLines)
