#!/usr/bin/env bash
set -e
set -o pipefail
set -x

# install
mozroots --import --sync --quiet
mono ./.nuget/NuGet.exe restore ./ScriptCs.sln

# script
mkdir artifacts --parents
xbuild ./ScriptCs.sln /property:Configuration=Release /nologo /verbosity:normal
mono ./packages/xunit.runners.1.9.2/tools/xunit.console.clr4.exe test/ScriptCs.Tests.Acceptance/bin/Release/ScriptCs.Tests.Acceptance.dll /xml artifacts/ScriptCs.Tests.Acceptance.dll.TestResult.xml /html artifacts/ScriptCs.Tests.Acceptance.dll.TestResult.html

