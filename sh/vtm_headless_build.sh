#!/bin/bash
# vtm_headless_build.sh - Virtual Tokyo Matching Headless World Builder
# Creates complete VRChat world structure via Unity CLI without manual interaction

set -e  # Exit on any error

# Configuration
UNITY_PATH="$HOME/Unity/Hub/Editor/2022.3.22f1/Editor/Unity"
PROJECT_PATH="$HOME/projects/VirtualTokyoMatching"
LOG_FILE="./unity_build.log"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "======================================================================"
echo "VirtualTokyoMatching - Headless World Builder"
echo "======================================================================"
echo "Unity Editor: $UNITY_PATH"
echo "Project Path: $PROJECT_PATH"
echo "Log File: $LOG_FILE"
echo "======================================================================"

# Verify Unity installation
if [ ! -f "$UNITY_PATH" ]; then
    echo "ERROR: Unity Editor not found at: $UNITY_PATH"
    echo "Please check your Unity installation or update the UNITY_PATH variable"
    exit 1
fi

# Verify project directory
if [ ! -d "$PROJECT_PATH" ]; then
    echo "ERROR: Unity project not found at: $PROJECT_PATH"
    echo "Please run setup_unity_project.sh first to create the project"
    exit 1
fi

echo "✅ Prerequisites verified"

# Function to run Unity command and check result
run_unity_command() {
    local method_name="$1"
    local description="$2"
    local log_file="$3"
    
    echo "🔄 $description..."
    
    "$UNITY_PATH" \
        -quit \
        -batchmode \
        -nographics \
        -logFile "$log_file" \
        -projectPath "$PROJECT_PATH" \
        -executeMethod "$method_name"
    
    local exit_code=$?
    
    if [ $exit_code -eq 0 ]; then
        echo "✅ $description completed successfully"
        return 0
    else
        echo "❌ $description failed with exit code: $exit_code"
        echo "Check log file for details: $log_file"
        return $exit_code
    fi
}

# Main execution
echo ""
echo "Starting headless world creation process..."
echo ""

# Step 1: Create complete world structure
run_unity_command \
    "VirtualTokyoMatching.Editor.VTMSceneBuilder.CreateCompleteWorld" \
    "Creating complete world structure" \
    "$LOG_FILE"

if [ $? -ne 0 ]; then
    echo ""
    echo "❌ World creation failed. Check the log file for details:"
    echo "   tail -20 $LOG_FILE"
    exit 1
fi

echo ""
echo "🎉 VirtualTokyoMatching world creation completed successfully!"
echo ""
echo "📋 Generated Content:"
echo "   • Scene: Assets/VirtualTokyoMatching/Scenes/VirtualTokyoMatching.unity"
echo "   • Environment: Lobby + 3 Session Rooms"
echo "   • Spawn Points: 10 total (8 lobby + 2 per room)"
echo "   • UI Systems: 4 canvas systems with Japanese localization"
echo "   • Lighting: Directional sun + ambient + room lighting"
echo "   • Systems: VTM Controller + 30 networked profiles"
echo ""
echo "📝 Next Steps:"
echo "   1. Open Unity Editor: $UNITY_PATH -projectPath $PROJECT_PATH"
echo "   2. Import VRChat SDK3 Worlds package if not already imported"
echo "   3. Add VRC_SceneDescriptor component to VRCWorld GameObject"
echo "   4. Configure spawn points in VRC_SceneDescriptor"
echo "   5. Add UdonSharp components to VTMController"
echo "   6. Build and upload to VRChat"
echo ""
echo "📊 Check the log file for detailed information:"
echo "   cat $LOG_FILE | grep 'VTM Builder'"
echo ""
echo "======================================================================"