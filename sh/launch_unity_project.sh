#!/bin/bash

# Virtual Tokyo Matching - Enhanced Unity Project Launcher
# Handles Unity Hub integration, VCC compatibility, and VRChat SDK setup

set -e

echo "======================================================================"
echo "Virtual Tokyo Matching - Enhanced Unity Project Launcher"
echo "======================================================================"

# Configuration
PROJECT_PATH="/home/kafka/projects/VirtualTokyoMatching"
PROJECT_PATH_ALT="/home/kafka/projects/virtualtokyomatching"
UNITY_EDITOR="/home/kafka/Unity/Hub/Editor/2022.3.22f1/Editor/Unity"
VCC_PATH="/home/kafka/.local/share/VRChatCreatorCompanion"
UNITY_VERSION="2022.3.22f1"

echo "🔍 Project Analysis:"
echo "   Primary Path: $PROJECT_PATH"
echo "   Alt Path: $PROJECT_PATH_ALT"
echo "   Unity Editor: $UNITY_EDITOR"
echo "   Unity Version: $UNITY_VERSION"
echo ""

# Determine correct project path
if [[ -d "$PROJECT_PATH" ]]; then
    ACTIVE_PROJECT_PATH="$PROJECT_PATH"
    echo "✅ Using primary project path: $PROJECT_PATH"
elif [[ -d "$PROJECT_PATH_ALT" ]]; then
    ACTIVE_PROJECT_PATH="$PROJECT_PATH_ALT"
    echo "✅ Using alternate project path: $PROJECT_PATH_ALT"
else
    echo "❌ Neither project path exists. Please ensure the project is properly set up."
    exit 1
fi

# Validate Unity Editor installation
if [[ ! -f "$UNITY_EDITOR" ]]; then
    echo "❌ Unity Editor not found at: $UNITY_EDITOR"
    echo "Please verify Unity installation."
    exit 1
fi

# Check for required build support modules
echo "🔧 Checking build support modules..."
PLAYBACK_ENGINES_DIR="/home/kafka/Unity/Hub/Editor/2022.3.22f1/Editor/Data/PlaybackEngines"

if [[ -d "$PLAYBACK_ENGINES_DIR/WindowsStandaloneSupport" ]]; then
    echo "✅ Windows Build Support: Available"
else
    echo "⚠️  Windows Build Support: Missing"
fi

if [[ -d "$PLAYBACK_ENGINES_DIR/AndroidPlayer" ]]; then
    echo "✅ Android Build Support: Available"
else
    echo "⚠️  Android Build Support: Missing"
fi

if [[ -d "$PLAYBACK_ENGINES_DIR/LinuxStandaloneSupport" ]]; then
    echo "✅ Linux Build Support: Available"
else
    echo "⚠️  Linux Build Support: Missing"
fi

echo ""

# Check VCC integration
echo "🔍 VRChat Creator Companion (VCC) Status:"
if [[ -d "$VCC_PATH" ]]; then
    echo "✅ VCC directory found: $VCC_PATH"
    
    # Look for VCC project listings
    VCC_PROJECTS_FILE="$VCC_PATH/projects.json"
    if [[ -f "$VCC_PROJECTS_FILE" ]]; then
        echo "✅ VCC projects registry found"
        if grep -q "VirtualTokyoMatching" "$VCC_PROJECTS_FILE" 2>/dev/null; then
            echo "✅ Project registered in VCC"
        else
            echo "⚠️  Project not registered in VCC"
        fi
    else
        echo "⚠️  VCC projects registry not found"
    fi
else
    echo "⚠️  VCC not detected at standard location"
fi

echo ""

# Check project Unity version compatibility
UNITY_VERSION_FILE="$ACTIVE_PROJECT_PATH/ProjectSettings/ProjectVersion.txt"
if [[ -f "$UNITY_VERSION_FILE" ]]; then
    PROJECT_UNITY_VERSION=$(grep "m_EditorVersion:" "$UNITY_VERSION_FILE" | cut -d' ' -f2)
    echo "📋 Project Unity Version: $PROJECT_UNITY_VERSION"
    
    if [[ "$PROJECT_UNITY_VERSION" == "$UNITY_VERSION" ]]; then
        echo "✅ Unity version matches"
    else
        echo "⚠️  Unity version mismatch - proceeding anyway"
    fi
else
    echo "⚠️  Could not determine project Unity version"
fi

echo ""

# Pre-launch setup
echo "⚙️  Pre-launch Setup:"

# Ensure proper project structure
if [[ ! -d "$ACTIVE_PROJECT_PATH/Assets" ]]; then
    echo "❌ Assets folder not found - this doesn't appear to be a valid Unity project"
    exit 1
fi

if [[ ! -d "$ACTIVE_PROJECT_PATH/ProjectSettings" ]]; then
    echo "❌ ProjectSettings folder not found - this doesn't appear to be a valid Unity project"
    exit 1
fi

# Check for VRChat SDK
VRC_SDK_FOUND=false
if [[ -d "$ACTIVE_PROJECT_PATH/Packages/com.vrchat.worlds" ]]; then
    echo "✅ VRChat SDK3 Worlds package found"
    VRC_SDK_FOUND=true
elif find "$ACTIVE_PROJECT_PATH/Assets" -name "*VRC*" -type d 2>/dev/null | head -1 | grep -q VRC; then
    echo "✅ VRChat SDK assets found in project"
    VRC_SDK_FOUND=true
else
    echo "⚠️  VRChat SDK not detected - will need to be imported"
fi

# Set build target for VRChat compatibility
echo "🎯 Configuring build settings for VRChat..."

# Kill any existing Unity processes for this project
echo "🔄 Checking for existing Unity processes..."
EXISTING_UNITY=$(pgrep -f "Unity.*$(basename "$ACTIVE_PROJECT_PATH")" || true)
if [[ -n "$EXISTING_UNITY" ]]; then
    echo "⚠️  Found existing Unity process(es): $EXISTING_UNITY"
    echo "Terminating existing processes..."
    pkill -f "Unity.*$(basename "$ACTIVE_PROJECT_PATH")" || true
    sleep 3
fi

# Create Unity launch wrapper script for VRChat optimization
LAUNCH_WRAPPER="$ACTIVE_PROJECT_PATH/unity_launch_wrapper.sh"
cat > "$LAUNCH_WRAPPER" << 'EOF'
#!/bin/bash
echo "🎮 Unity Editor launched with VRChat optimizations"
echo "📋 VRChat Development Checklist:"
echo "   1. File → Build Settings → PC, Mac & Linux Standalone (Windows 64-bit)"
echo "   2. Window → VRChat SDK → Show Control Panel"
echo "   3. Import VRChat SDK3 Worlds if not already present"
echo "   4. Set up build targets: Windows 64-bit (PC) and/or Android (Quest)"
echo "   5. Configure world settings in VRChat SDK Control Panel"
echo ""
echo "🚀 VTM-specific tasks:"
echo "   • Open scene: Assets/VirtualTokyoMatching/Scenes/VirtualTokyoMatching.unity"
echo "   • Use VTM menu: VTM/Create Complete World to build scene"
echo "   • Test with multiple clients for multiplayer verification"
echo ""
exec "$@"
EOF

chmod +x "$LAUNCH_WRAPPER"

echo ""
echo "🚀 Launching Unity Editor..."
echo "Project: $ACTIVE_PROJECT_PATH"
echo ""

# Launch Unity with optimized parameters for VRChat development
UNITY_CMD=(
    "$LAUNCH_WRAPPER"
    "$UNITY_EDITOR"
    -projectPath "$ACTIVE_PROJECT_PATH"
    -logFile "$ACTIVE_PROJECT_PATH/unity_launch.log"
)

# Add VRChat-specific arguments
if [[ "$VRC_SDK_FOUND" == true ]]; then
    UNITY_CMD+=(-buildTarget StandaloneWindows64)
fi

echo "💻 Command: ${UNITY_CMD[*]}"
echo ""

# Execute Unity launch
"${UNITY_CMD[@]}" &
UNITY_PID=$!

echo "✅ Unity Editor launched successfully (PID: $UNITY_PID)"
echo ""
echo "📚 Development Resources:"
echo "   • VRChat Creator Documentation: https://creators.vrchat.com/"
echo "   • SDK3 Worlds Documentation: https://creators.vrchat.com/worlds/"
echo "   • UdonSharp Documentation: https://udonsharp.docs.vrchat.com/"
echo ""
echo "🔧 Troubleshooting Tips:"
echo "   • If VRChat SDK platform dropdown is disabled:"
echo "     → File → Build Settings → Switch to Windows 64-bit platform"
echo "     → Restart Unity Editor if issues persist"
echo "   • For Quest builds: Switch to Android platform in Build Settings"
echo "   • Performance targets: PC ≥72FPS, Quest ≥60FPS"
echo ""
echo "📍 Log file: $ACTIVE_PROJECT_PATH/unity_launch.log"
echo "Press Ctrl+C to exit this launcher (Unity will continue running)"

# Monitor Unity process
trap "echo ''; echo '🛑 Launcher stopped. Unity Editor continues running.'; exit 0" INT

while kill -0 $UNITY_PID 2>/dev/null; do
    sleep 10
done

echo ""
echo "Unity Editor has closed."
echo "Check log file for details: $ACTIVE_PROJECT_PATH/unity_launch.log"