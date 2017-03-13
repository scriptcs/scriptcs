#!/usr/bin/env bash
set -e
set -o pipefail
set -x

# install
mozroots --import --sync --quiet

# script
mkdir -p artifacts/Release/bin
xbuild ./build/Build.proj /nologo /v:M /t:Build "$@" /fl "/flp:LogFile=artifacts/msbuild.log;Verbosity=diagnostic;DetailedSummary"

# Not using Build.proj to copy artifacts or run tests due to bug: wildcard includes have incorrect and duplicate files
# https://bugzilla.xamarin.com/show_bug.cgi?id=31628
cp -r src/*/bin/Release artifacts/Release/bin/
mono ./packages/xunit.runners.1.9.2/tools/xunit.console.clr4.exe test/ScriptCs.Tests.Acceptance/bin/Release/ScriptCs.Tests.Acceptance.dll /xml artifacts/ScriptCs.Tests.Acceptance.dll.TestResult.xml /html artifacts/ScriptCs.Tests.Acceptance.dll.TestResult.html
