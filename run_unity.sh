#!/bin/bash

# Unity Project Launcher for Virtual Tokyo Matching
# Launches Unity Editor with the VTM project

PROJECT_PATH="/home/kafka/projects/VirtualTokyoMatching"
UNITY_EDITOR="/home/kafka/Unity/Hub/Editor/2022.3.22f1/Editor/Unity"

echo "======================================================================"
echo "Virtual Tokyo Matching - Unity Project Launcher"
echo "======================================================================"
echo "Project Path: $PROJECT_PATH"
echo "Unity Editor: $UNITY_EDITOR"
echo "======================================================================"

# Check if Unity Editor exists
if [[ ! -f "$UNITY_EDITOR" ]]; then
    echo "❌ Unity Editor not found at: $UNITY_EDITOR"
    echo "Please check your Unity installation."
    exit 1
fi

# Check if project exists
if [[ ! -d "$PROJECT_PATH" ]]; then
    echo "❌ Project not found at: $PROJECT_PATH"
    echo "Please run setup_unity_project.sh first."
    exit 1
fi

# Kill any existing Unity processes for this project
echo "🔄 Checking for existing Unity processes..."
EXISTING_UNITY=$(pgrep -f "Unity.*VirtualTokyoMatching" || true)
if [[ -n "$EXISTING_UNITY" ]]; then
    echo "⚠️  Found existing Unity process(es): $EXISTING_UNITY"
    echo "Terminating existing processes..."
    pkill -f "Unity.*VirtualTokyoMatching" || true
    sleep 2
fi

echo "🚀 Launching Unity Editor..."
echo "Opening project: $PROJECT_PATH"
echo ""
echo "Unity Editor will open in a new window."
echo "To close this launcher, press Ctrl+C"
echo ""

# Launch Unity Editor
"$UNITY_EDITOR" -projectPath "$PROJECT_PATH" &
UNITY_PID=$!

echo "✅ Unity Editor launched (PID: $UNITY_PID)"
echo ""
echo "💡 Tips:"
echo "   • Import VRChat SDK3 Worlds via Window → VRChat SDK → Show Control Panel"
echo "   • Find the VirtualTokyoMatching scene in Assets/VirtualTokyoMatching/Scenes/"
echo "   • Use VTM menu items to build world components"
echo ""
echo "Press Ctrl+C to exit this launcher (Unity will continue running)"

# Wait for Unity process or user interrupt
trap "echo ''; echo '🛑 Launcher stopped. Unity Editor will continue running.'; exit 0" INT

# Monitor Unity process
while kill -0 $UNITY_PID 2>/dev/null; do
    sleep 5
done

echo "Unity Editor has closed."