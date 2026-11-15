#!/usr/bin/bash

set -euo pipefail

STEAM_DIR="$HOME/.local/share/Steam"                                                  # Steam 数据目录
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"                                           # 当前目录
RUNTIME_DIR="${XDG_RUNTIME_DIR:-/run/user/$(id -u)}"                                  # XDG 用户运行时目录
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
    --ro-bind /etc /etc

    # usr
    --ro-bind /usr /usr
    --symlink usr/lib /lib
    --symlink usr/lib /lib64
    --symlink usr/bin /bin
    --symlink usr/bin /sbin

    # proc, sys
    --proc /proc
    --ro-bind /sys /sys

    # 单独处理 tmp 和 run
    --tmpfs /tmp
    --tmpfs /run
    --dir "$RUNTIME_DIR"

    # dev
    --dev /dev
    --dev-bind /dev/shm /dev/shm
    --dev-bind /dev/dri /dev/dri
    --dev-bind-try /dev/udmabuf /dev/udmabuf

    # Wayland
    --ro-bind "$RUNTIME_DIR/${WAYLAND_DISPLAY:-wayland-0}" "$RUNTIME_DIR/${WAYLAND_DISPLAY:-wayland-0}"

    # 音频
    --ro-bind "$RUNTIME_DIR/pulse" "$RUNTIME_DIR/pulse"

    # Discord RPC
    --ro-bind-try "$RUNTIME_DIR/discord-ipc-0" "$RUNTIME_DIR/discord-ipc-0"

    # D-Bus (gamemoderun 与 Fcitx5/IBus 输入法通信)
    --ro-bind-try "$RUNTIME_DIR/bus" "$RUNTIME_DIR/bus"
    --ro-bind-try /run/dbus /run/dbus

    # 隔离 home
    --tmpfs "$HOME"
    --ro-bind-try "$HOME/.pulse-cookie" "$HOME/.pulse-cookie"
    --ro-bind-try "$HOME/.local/share/fonts" "$HOME/.local/share/fonts"
    --ro-bind-try "$HOME/.config/fontconfig" "$HOME/.config/fontconfig"

    # 游戏本体
    --ro-bind "$STEAM_DIR/steamapps/common/RimWorld" "$STEAM_DIR/steamapps/common/RimWorld"

    # 游戏存档
    --bind "$RIMWORLD_DATA_DIR" "$RIMWORLD_DATA_DIR"

    # 包装器
    --ro-bind "$SCRIPT_DIR" "$SCRIPT_DIR"

    # Steam 最小化只读挂载
    --ro-bind-try "$HOME/.steam" "$HOME/.steam"
    --ro-bind-try "$HOME/.steampath" "$HOME/.steampath"
    --ro-bind-try "$HOME/.steampid" "$HOME/.steampid"
    --ro-bind-try "$STEAM_DIR/linux64" "$STEAM_DIR/linux64"
    --ro-bind-try "$STEAM_DIR/ubuntu12_32" "$STEAM_DIR/ubuntu12_32"
    --ro-bind-try "$STEAM_DIR/steamapps/workshop/content/294100" "$STEAM_DIR/steamapps/workshop/content/294100"

    # ! 我不明白，为什么载具框架要改自己 Mod 下的文件，好像是和更新日志有关
    --overlay-src "$STEAM_DIR/steamapps/workshop/content/294100/3014915404"
    --tmp-overlay "$STEAM_DIR/steamapps/workshop/content/294100/3014915404"

    # ! 我不明白，为什么 Vanilla Achievements Expanded Mod 要修改自己目录下的文件
    --overlay-src "$STEAM_DIR/steamapps/workshop/content/294100"
    --tmp-overlay "$STEAM_DIR/steamapps/workshop/content/294100"

    # 环境变量
    --setenv XDG_RUNTIME_DIR "$RUNTIME_DIR"
	# # 使用系统库
    --setenv LD_PRELOAD "/usr/lib/libz.so /usr/lib/libfreetype.so"

	# 工作目录
    --chdir "$SCRIPT_DIR"
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
            NEW_ARGS+=(bwrap "${BWRAP_ARGS[@]}" gamemoderun "$1")
            shift
            ;;
        *)
            NEW_ARGS+=("$1")
            shift
            ;;
    esac
done

exec "${NEW_ARGS[@]}"
