#!/usr/bin/bash

set -euo pipefail

STEAM_DIR="$HOME/.local/share/Steam"                                                  # Steam 数据目录
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"                                           # 当前目录
RUNTIME_DIR="${XDG_RUNTIME_DIR:-/run/user/$(id -u)}"                                  # XDG 用户运行时目录
STEAM_WORKSHOP="$STEAM_DIR/steamapps/workshop/content/294100"                         # Steam 创意工坊目录
RIMWORLD_DATA_DIR="$HOME/.config/unity3d/Ludeon Studios/RimWorld by Ludeon Studios"   # 游戏数据目录

# .NET Hook
export DOTNET_STARTUP_HOOKS="$SCRIPT_DIR/.bin/RimWorld.StartupHook/RimWorld.StartupHook.dll"

# .NET GC
export DOTNET_gcServer=1
export DOTNET_gcConcurrent=1
export DOTNET_GCDynamicAdaptationMode=1

# .NET JIT
export DOTNET_TieredPGO=1
export DOTNET_TieredCompilation=1

# bwrap 隔离参数
BWRAP_ARGS=(
    # etc
    --tmpfs /etc
    --ro-bind /etc/ssl /etc/ssl
	--ro-bind-try /etc/alsa /etc/alsa
    --ro-bind /etc/localtime /etc/localtime
    --ro-bind /etc/resolv.conf /etc/resolv.conf
    --ro-bind /etc/ld.so.cache /etc/ld.so.cache
    --ro-bind /etc/nsswitch.conf /etc/nsswitch.conf
    --ro-bind /etc/ca-certificates /etc/ca-certificates

    # usr
    --ro-bind /usr /usr
    --symlink usr/bin /bin
    --symlink usr/lib /lib
	--symlink usr/lib /lib64

    # proc, sys
    --proc /proc
    --ro-bind /sys /sys

    # 单独处理 tmp 和 run
    --tmpfs /tmp
    --tmpfs /run
    --dir "$RUNTIME_DIR"
    --ro-bind-try "$RUNTIME_DIR/pipewire-0" "$RUNTIME_DIR/pipewire-0"
    --ro-bind-try "$RUNTIME_DIR/discord-ipc-0" "$RUNTIME_DIR/discord-ipc-0"
    --ro-bind "$RUNTIME_DIR/${WAYLAND_DISPLAY:-wayland-0}" "$RUNTIME_DIR/${WAYLAND_DISPLAY:-wayland-0}"

    # dev
    --dev /dev
    --dir /dev/dri
	--dev-bind-try /dev/udmabuf /dev/udmabuf
    --dev-bind /dev/dri/renderD128 /dev/dri/renderD128

    # 隔离 home
    --tmpfs "$HOME"

    # 游戏存档
    --bind "$RIMWORLD_DATA_DIR" "$RIMWORLD_DATA_DIR"

    # 包装器
    --ro-bind "$SCRIPT_DIR/.bin" "$SCRIPT_DIR/.bin"

    # Steam
	--dir "$HOME/.steam"
    --ro-bind-try "$STEAM_WORKSHOP" "$STEAM_WORKSHOP"
    --ro-bind-try "$HOME/.steam/sdk64" "$HOME/.steam/sdk64"
    --ro-bind-try "$STEAM_DIR/linux64" "$STEAM_DIR/linux64"
    --ro-bind-try "$HOME/.steam/steam.pid" "$HOME/.steam/steam.pid"

    # 游戏本体
    --ro-bind "$STEAM_DIR/steamapps/common/RimWorld" "$STEAM_DIR/steamapps/common/RimWorld"

    # ! 我不明白，为什么载具框架要改自己 Mod 下的文件，好像是和更新日志有关：<https://github.com/SmashPhil/Vehicle-Framework/issues/389>
    --bind-try "$STEAM_WORKSHOP/3014915404/Updates/UpdateLog.xml" "$STEAM_WORKSHOP/3014915404/Updates/UpdateLog.xml"

    # ! 给 Mod 兜底
    --overlay-src "$STEAM_DIR/steamapps/workshop/content/294100/2288125657"
    --tmp-overlay "$STEAM_DIR/steamapps/workshop/content/294100/2288125657"

    --overlay-src "$STEAM_DIR/steamapps/workshop/content/294100/1660622094"
    --tmp-overlay "$STEAM_DIR/steamapps/workshop/content/294100/1660622094"

    # 其他
	--new-session
    --unshare-ipc
    --unshare-uts
	--unshare-user
	--cap-drop ALL
	--unshare-cgroup
	--disable-userns

    # 环境变量
    --setenv XDG_RUNTIME_DIR "$RUNTIME_DIR"
    # # 使用系统库
    --setenv LD_PRELOAD "/usr/lib/libz.so /usr/lib/libfreetype.so"

    # 工作目录
    --chdir "$STEAM_DIR/steamapps/common/RimWorld"

    --die-with-parent
)

NEW_ARGS=()
while (($#)); do
    case "$1" in
        # 去除 Steam 的隔离
        */SteamLinuxRuntime_4/_v2-entry-point)
            shift 3
            ;;
        */RimWorld/RimWorld)
            # 插入我自己的隔离
            NEW_ARGS+=(bwrap "${BWRAP_ARGS[@]}" "$1")
            shift
            ;;
        *)
            NEW_ARGS+=("$1")
            shift
            ;;
    esac
done

exec "${NEW_ARGS[@]}"
