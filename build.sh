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
cp src/ScriptCs/bin/Release/net461/* artifacts/Release/bin/
mono ./tools/xunit.runner.console.2.3.0/xunit.console.exe test/ScriptCs.Tests.Acceptance/bin/Release/net461/ScriptCs.Tests.Acceptance.dll

