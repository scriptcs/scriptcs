namespace ScriptCs.PaketDirective
open System
open System.IO
module PaketShim =
    let ResolveLoadScript(scriptFile: FileInfo, packageManagerTextLines: string array, alterToolPath: Func<string,string>) =
        ReferenceLoading.PaketHandler.Internals.ResolvePackages alterToolPath.Invoke (scriptFile.Directory.FullName, scriptFile.Name, Array.toList packageManagerTextLines)
