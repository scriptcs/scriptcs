#!/usr/bin/env bash
set -e
set -o pipefail
set -x

# install
mozroots --import --sync --quiet
mono ./.nuget/NuGet.exe restore ./.nuget/packages.config -PackagesDirectory ./packages
mono ./.nuget/NuGet.exe restore ./ScriptCs.sln

# script
mkdir -p artifacts/Release/bin
msbuild ./ScriptCs.sln /property:Configuration=Release /nologo /verbosity:normal
cp src/ScriptCs/bin/Release/* artifacts/Release/bin/
mono ./packages/xunit.runner.console.2.3.1/tools/net452/xunit.console.exe test/ScriptCs.Tests.Acceptance/bin/Release/ScriptCs.Tests.Acceptance.dll