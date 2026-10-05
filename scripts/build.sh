#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
dotnet_bin="${DOTNET_BIN:-$PWD/.tools/dotnet/dotnet}"
"$dotnet_bin" build src/RepoEmoteWheel.csproj -c Release --nologo "$@"
mkdir -p dist
cp src/bin/Release/netstandard2.1/RepoEmoteWheel.dll dist/
