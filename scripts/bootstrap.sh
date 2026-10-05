#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
mkdir -p .tools
if [[ ! -x .tools/dotnet/dotnet ]]; then
  curl -fsSL https://dot.net/v1/dotnet-install.sh -o .tools/dotnet-install.sh
  bash .tools/dotnet-install.sh --version 8.0.425 --install-dir "$PWD/.tools/dotnet"
fi
if [[ ! -f .tools/bepinex/BepInEx/core/BepInEx.dll ]]; then
  curl -fL https://github.com/BepInEx/BepInEx/releases/download/v5.4.23.2/BepInEx_win_x64_5.4.23.2.zip -o .tools/bepinex.zip
  python3 -c 'import zipfile; zipfile.ZipFile(".tools/bepinex.zip").extractall(".tools/bepinex")'
fi
