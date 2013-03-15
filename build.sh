#!/usr/bin/env bash

export EnableNuGetPackageRestore=true

xbuild ScriptCs.sln

if [ ! -d "packages/xunit.runners.1.9.1" ]; then
    # Install the xunit runners package
    mono --runtime=v4.0.30319 .nuget/NuGet.exe install xunit.runners -version 1.9.1 -o packages
fi

RUNNER_PATH="/Users/Dale/Code/OpenSource/ScriptCS/packages/xunit.runners.1.9.1/tools"


mono ${RUNNER_PATH}/xunit.console.clr4.x86.exe test/ScriptCs.Core.Tests/bin/Debug/ScriptCs.Core.Tests.dll