#!/usr/bin/env bash
set -e
set -o pipefail
set -x

# install
mozroots --import --sync --quiet
dotnet restore

# script
mkdir -p artifacts/Release/bin
msbuild ./ScriptCs.sln /property:Configuration=Release /nologo /verbosity:normal
cp src/ScriptCs/bin/Release/* artifacts/Release/bin/
mono ./packages/xunit.runner.console.2.3.1/tools/net461/xunit.console.exe test/ScriptCs.Tests.Acceptance/bin/Release/ScriptCs.Tests.Acceptance.dll

