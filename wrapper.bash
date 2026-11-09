#!/usr/bin/bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)" # 当前目录

# # 使用系统库
export LD_PRELOAD="/usr/lib/libz.so /usr/lib/libfreetype.so"

# .NET Hook
export DOTNET_STARTUP_HOOKS="$SCRIPT_DIR/.bin/RimWorld.StartupHook/RimWorld.StartupHook.dll"

# .NET GC
export DOTNET_gcServer=1
export DOTNET_gcConcurrent=1
export DOTNET_GCDynamicAdaptationMode=1

# .NET JIT
export DOTNET_TieredPGO=1
export DOTNET_TieredCompilation=1

NEW_ARGS=()
while (($#)); do
    case "$1" in
        */SteamLinuxRuntime_4/_v2-entry-point)
            shift 3
            ;;
        *)
            NEW_ARGS+=("$1")
            shift
            ;;
    esac
done

exec gamemoderun "${NEW_ARGS[@]}"
