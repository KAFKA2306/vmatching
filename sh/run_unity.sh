#!/usr/bin/env bash
PROJECT_PATH="$HOME/projects/VirtualTokyoMatching"
EDITOR="$HOME/Unity/Hub/Editor/2022.3.22f1/Editor/Unity"

echo "🔄 Checking for existing Unity processes..."
mapfile -t PIDS < <(pgrep -f "$EDITOR")
if (( ${#PIDS[@]} )); then
  echo "⚠️  Found existing Unity process(es): ${PIDS[*]}"
  echo "Terminating existing processes..."
  pkill -9 -f "$EDITOR"
  sleep 2
fi

echo "🚀 Launching Unity Editor..."
"$EDITOR" -projectPath "$PROJECT_PATH" -logFile "$PROJECT_PATH/unity_launch.log" &
echo "✅ Unity Editor launched (PID: $!)"
