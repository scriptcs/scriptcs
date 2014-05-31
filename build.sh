#!/usr/bin/env bash
set -e
set -o pipefail
set -x

# install
mozroots --import --sync --quiet
mono ./.nuget/NuGet.exe restore ./ScriptCs.sln

# script
xbuild ./ScriptCs.sln /property:Configuration=Release /nologo /verbosity:normal
