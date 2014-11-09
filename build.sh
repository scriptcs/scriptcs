#!/usr/bin/env bash
set -e
set -o pipefail
set -x

# install
mozroots --import --sync --quiet
mono ./.paket/paket.bootstrapper.exe 
mono ./.paket/paket.exe restore

# script
mkdir -p artifacts
xbuild ./ScriptCs.sln /property:Configuration=Release /nologo /verbosity:normal
mono ./packages/xunit.runners/tools/xunit.console.clr4.exe test/ScriptCs.Tests.Acceptance/bin/Release/ScriptCs.Tests.Acceptance.dll /xml artifacts/ScriptCs.Tests.Acceptance.dll.TestResult.xml /html artifacts/ScriptCs.Tests.Acceptance.dll.TestResult.html

