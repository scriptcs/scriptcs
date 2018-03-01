#r "packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.DotNetCli

Target "Clean" (fun _ ->
    !! "artifacts" ++ "src/*/bin" ++ "test/*/bin"
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
    DotNetCli.Test
       (fun p -> 
          { p with 
              Configuration = "Release" })
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
      ==> "Pack"

RunTargetOrDefault "Pack"