#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
"${DOTNET_BIN:-$PWD/.tools/dotnet/dotnet}" run --project tests/Tests.csproj -c Release
python3 -m unittest discover -s tests -p 'test_*.py'
