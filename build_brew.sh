#!/usr/bin/env bash
set -e
set -o pipefail
set -x

# install
mozroots --import --sync --quiet
mono ./.nuget/NuGet.exe restore ./ScriptCs.sln

# script
mkdir -p artifacts/Release/bin
xbuild ./ScriptCs.sln /property:Configuration=Release /nologo /verbosity:normal
cp src/ScriptCs/bin/Release/* artifacts/Release/bin/
