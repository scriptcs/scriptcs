#r "packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.DotNetCli
open Fake.Testing
open System.IO

Target "Clean" (fun _ ->
    !! "artifacts" ++ "src/*/bin" ++ "test/*/bin" ++ "src/*/obj" ++ "test/*/obj"
        |> DeleteDirs
)

Target "Build" (fun _ ->
    DotNetCli.Restore id

    "ScriptCs.sln"
    |> MSBuildHelper.build (fun p ->
        { p with
             RestorePackagesFlag = true
             Verbosity = Some Minimal
             Targets = [ "Build" ] 
             Properties =
                [
                    "Optimize", "True"
                    "Configuration", "Release"
                ]
        } ) 
)

Target "Test" (fun _ ->
#if MONO
    !! "test/**/bin/**/*Tests.Acceptance.dll"
    |> xUnit2 (fun c -> 
         {c with 
             MaxThreads = CollectionConcurrencyMode.MaxThreads 1
         })
#else
    !! "test/**/*Tests*.csproj"
    |> Seq.iter (fun p -> 
         DotNetCli.Test (fun c -> 
            {c with 
                WorkingDir = Path.GetDirectoryName p
                AdditionalArgs = ["--no-build"]
            })
    )
#endif
)

Target "Pack" (fun _ ->
    "ScriptCs.sln"
    |> MSBuildHelper.build (fun p ->
        { p with
             RestorePackagesFlag = true
             Verbosity = Some Minimal
             Targets = [ "Pack" ] 
             Properties =
                [
                    "Optimize", "True"
                    "Configuration", "Release"
                    "PackageOutputPath", "../../artifacts"
                ]
        } ) 
)

"Clean"
      ==> "Build"
      ==> "Test"
      ==> "Pack"

RunTargetOrDefault "Pack"