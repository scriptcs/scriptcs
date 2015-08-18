#!/usr/bin/env bash
set -e
set -o pipefail
set -x

# install
mozroots --import --sync --quiet
mono ./.nuget/NuGet.exe restore ./ScriptCs.sln

# script
mkdir -p artifacts/Release/bin
xbuild ./build/Build.proj /property:Configuration=Release /nologo /v:m /target:UpdateProjectVersions
xbuild ./ScriptCs.sln /property:Configuration=Release /nologo /v:m /fl "/flp:LogFile=artifacts/msbuild.log;Verbosity=diagnostic;DetailedSummary"
xbuild ./build/Build.proj /property:Configuration=Release /nologo /v:m /target:RestoreCommonVersionInfo

cp src/*/bin/Release/* artifacts/Release/bin/ 2>/dev/null
mono ./packages/xunit.runners.1.9.2/tools/xunit.console.clr4.exe test/ScriptCs.Tests.Acceptance/bin/Release/ScriptCs.Tests.Acceptance.dll /xml artifacts/ScriptCs.Tests.Acceptance.dll.TestResult.xml /html artifacts/ScriptCs.Tests.Acceptance.dll.TestResult.html
