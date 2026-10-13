#!/usr/bin/env bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

DOTNET_CMD="/home/cj/.dotnet/dotnet"
if ! command -v "$DOTNET_CMD" &> /dev/null; then
    DOTNET_CMD="dotnet"
fi

export DOTNET_ROOT="/home/cj/.dotnet"
export PATH="/home/cj/.dotnet:$PATH"

echo "Building MoreLives mod..."
"$DOTNET_CMD" build -c Release

echo "Build complete! Output DLL copied to Atomicrops/BepInEx/plugins."
