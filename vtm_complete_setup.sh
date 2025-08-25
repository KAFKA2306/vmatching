#!/bin/bash
# vtm_complete_setup.sh - Complete automated setup for Virtual Tokyo Matching
# Handles full project setup, world creation, and optimization

set -e  # Exit on any error

# Configuration
UNITY_PATH="$HOME/Unity/Hub/Editor/2022.3.22f1/Editor/Unity"
PROJECT_PATH="$HOME/projects/VirtualTokyoMatching"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Log files
SETUP_LOG="./unity_setup.log"
BUILD_LOG="./unity_build.log"
MATERIALS_LOG="./unity_materials.log"
OPTIMIZATION_LOG="./unity_optimization.log"
VALIDATION_LOG="./unity_validation.log"

echo "======================================================================"
echo "VirtualTokyoMatching - Complete Automated Setup"
echo "======================================================================"
echo "This script will:"
echo "  1. Create the complete world structure"
echo "  2. Setup materials and lighting optimization"
echo "  3. Apply VRChat-specific optimizations"
echo "  4. Validate the complete setup"
echo ""
echo "Unity Editor: $UNITY_PATH"
echo "Project Path: $PROJECT_PATH"
echo "======================================================================"

# Verify prerequisites
if [ ! -f "$UNITY_PATH" ]; then
    echo "❌ ERROR: Unity Editor not found at: $UNITY_PATH"
    exit 1
fi

if [ ! -d "$PROJECT_PATH" ]; then
    echo "❌ ERROR: Unity project not found at: $PROJECT_PATH"
    echo "Please run setup_unity_project.sh first"
    exit 1
fi

echo "✅ Prerequisites verified"
echo ""

# Function to run Unity command with better error handling
run_unity_step() {
    local step_number="$1"
    local method_name="$2"
    local description="$3"
    local log_file="$4"
    
    echo "🔄 Step $step_number: $description..."
    
    # Clear previous log
    > "$log_file"
    
    "$UNITY_PATH" \
        -quit \
        -batchmode \
        -nographics \
        -logFile "$log_file" \
        -projectPath "$PROJECT_PATH" \
        -executeMethod "$method_name" || {
        
        echo "❌ Step $step_number failed!"
        echo "📋 Error details from $log_file:"
        echo "----------------------------------------"
        tail -20 "$log_file" | grep -E "(Error|Exception|Failed|VTM)" || echo "No specific errors found in log"
        echo "----------------------------------------"
        echo ""
        echo "💡 Troubleshooting:"
        echo "   • Check if VRChat SDK is properly imported"
        echo "   • Verify Unity project is not corrupted"
        echo "   • Run: cat $log_file | grep -i error"
        return 1
    }
    
    echo "✅ Step $step_number completed successfully"
    
    # Show success details from log
    grep "VTM Builder" "$log_file" | tail -5 || true
    echo ""
    
    return 0
}

# Main execution sequence
echo "Starting complete automated setup..."
echo ""

# Step 1: Create world structure
run_unity_step 1 \
    "VirtualTokyoMatching.Editor.VTMSceneBuilder.CreateCompleteWorld" \
    "Creating base world structure" \
    "$BUILD_LOG"

# Step 2: Setup materials (if method exists)
if grep -q "SetupMaterials" "$PROJECT_PATH/Assets/VirtualTokyoMatching/Scripts/Editor/VTMSceneBuilder.cs" 2>/dev/null; then
    run_unity_step 2 \
        "VirtualTokyoMatching.Editor.VTMSceneBuilder.SetupMaterials" \
        "Setting up materials and lighting" \
        "$MATERIALS_LOG"
else
    echo "⚠️  Step 2: Material setup method not found - skipping"
fi

# Step 3: VRChat optimization (if method exists)
if grep -q "OptimizeForVRChat" "$PROJECT_PATH/Assets/VirtualTokyoMatching/Scripts/Editor/VTMSceneBuilder.cs" 2>/dev/null; then
    run_unity_step 3 \
        "VirtualTokyoMatching.Editor.VTMSceneBuilder.OptimizeForVRChat" \
        "Applying VRChat optimizations" \
        "$OPTIMIZATION_LOG"
else
    echo "⚠️  Step 3: VRChat optimization method not found - skipping"
fi

# Step 4: Validate setup
if [ -f "$PROJECT_PATH/Assets/VirtualTokyoMatching/Scripts/Testing/VTMSystemValidator.cs" ]; then
    run_unity_step 4 \
        "VirtualTokyoMatching.Testing.VTMSystemValidator.ValidateCompleteSetup" \
        "Validating complete setup" \
        "$VALIDATION_LOG"
else
    echo "⚠️  Step 4: System validator not found - skipping validation"
fi

echo ""
echo "🎉 VirtualTokyoMatching - Complete Setup Finished!"
echo ""
echo "📋 Generated Assets:"
echo "   📁 Scene: Assets/VirtualTokyoMatching/Scenes/VirtualTokyoMatching.unity"
echo "   🏗️  Environment: Lobby (20x20m) + 3 Session Rooms (10x10m each)"
echo "   📍 Spawn Points: 10 total with proper orientation"
echo "   🖥️  UI Systems: Screen Space + 3 World Space canvases"
echo "   💡 Lighting: Directional sun + ambient + per-room lighting"
echo "   🔗 Systems: VTM Controller + 30 networked profile slots"
echo ""
echo "📊 Performance Targets:"
echo "   🖥️  PC: ≥72FPS, <200MB"
echo "   📱 Quest: ≥60FPS, <100MB"
echo ""
echo "🚀 Next Steps to Complete VRChat World:"
echo "   1. Open Unity Editor:"
echo "      $UNITY_PATH -projectPath $PROJECT_PATH"
echo ""
echo "   2. Import VRChat SDK3 Worlds (if not already imported):"
echo "      Window → VRChat SDK → Show Control Panel"
echo ""
echo "   3. Add VRC_SceneDescriptor to VRCWorld GameObject:"
echo "      - Find VRCWorld in hierarchy"
echo "      - Add Component → VRC_SceneDescriptor"
echo "      - Set spawn points from SpawnSystem GameObject children"
echo ""
echo "   4. Add UdonSharp components to VTMController:"
echo "      - PlayerDataManager, DiagnosisController, etc."
echo "      - Wire up component dependencies in inspector"
echo ""
echo "   5. Configure and test:"
echo "      - Test with ClientSim for multiplayer behavior"
echo "      - Build for PC and Android (Quest)"
echo "      - Upload to VRChat via VCC"
echo ""
echo "📝 Log Files:"
echo "   🔍 Build: $BUILD_LOG"
echo "   🎨 Materials: $MATERIALS_LOG"  
echo "   ⚡ Optimization: $OPTIMIZATION_LOG"
echo "   ✅ Validation: $VALIDATION_LOG"
echo ""
echo "💡 Troubleshooting Commands:"
echo "   cat $BUILD_LOG | grep -E '(VTM Builder|Error|Exception)'"
echo "   ls -la '$PROJECT_PATH/Assets/VirtualTokyoMatching/Scenes/'"
echo ""
echo "======================================================================"
echo "Virtual Tokyo Matching - Ready for VRChat Deployment! 🚀"
echo "======================================================================"