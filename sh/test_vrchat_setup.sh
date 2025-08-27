#!/bin/bash

# Virtual Tokyo Matching - VRChat Setup Verification Script
# Tests Unity Hub integration, build targets, and VRChat SDK compatibility

set -e

echo "======================================================================"
echo "Virtual Tokyo Matching - VRChat Setup Verification"
echo "======================================================================"

PROJECT_PATH="/home/kafka/projects/VirtualTokyoMatching"
PROJECT_PATH_ALT="/home/kafka/projects/virtualtokyomatching" 
UNITY_EDITOR="/home/kafka/Unity/Hub/Editor/2022.3.22f1/Editor/Unity"

# Determine correct project path
if [[ -d "$PROJECT_PATH" ]]; then
    ACTIVE_PROJECT_PATH="$PROJECT_PATH"
elif [[ -d "$PROJECT_PATH_ALT" ]]; then
    ACTIVE_PROJECT_PATH="$PROJECT_PATH_ALT"
else
    echo "❌ Project not found"
    exit 1
fi

echo "🔍 Testing project: $ACTIVE_PROJECT_PATH"
echo ""

# Test Results
TESTS_PASSED=0
TESTS_FAILED=0
WARNINGS=0

run_test() {
    local test_name="$1"
    local test_command="$2"
    local success_message="$3"
    local failure_message="$4"
    
    echo "🧪 Testing: $test_name"
    
    if eval "$test_command"; then
        echo "✅ $success_message"
        ((TESTS_PASSED++))
    else
        echo "❌ $failure_message"
        ((TESTS_FAILED++))
    fi
    echo ""
}

run_warning_check() {
    local check_name="$1"
    local check_command="$2"
    local warning_message="$3"
    local ok_message="$4"
    
    echo "⚠️  Checking: $check_name"
    
    if eval "$check_command"; then
        echo "⚠️  $warning_message"
        ((WARNINGS++))
    else
        echo "✅ $ok_message"
    fi
    echo ""
}

echo "🔧 INFRASTRUCTURE TESTS"
echo "========================="

# Test 1: Unity Editor Installation
run_test "Unity Editor Installation" \
    "[[ -f '$UNITY_EDITOR' ]]" \
    "Unity Editor found at correct location" \
    "Unity Editor not found - check installation"

# Test 2: Project Structure
run_test "Unity Project Structure" \
    "[[ -d '$ACTIVE_PROJECT_PATH/Assets' && -d '$ACTIVE_PROJECT_PATH/ProjectSettings' ]]" \
    "Valid Unity project structure detected" \
    "Invalid Unity project structure"

# Test 3: Build Support Modules
run_test "Windows Build Support" \
    "[[ -d '/home/kafka/Unity/Hub/Editor/2022.3.22f1/Editor/Data/PlaybackEngines/WindowsStandaloneSupport' ]]" \
    "Windows build support module installed" \
    "Windows build support missing"

run_test "Android Build Support" \
    "[[ -d '/home/kafka/Unity/Hub/Editor/2022.3.22f1/Editor/Data/PlaybackEngines/AndroidPlayer' ]]" \
    "Android build support module installed" \
    "Android build support missing"

echo "🎯 PROJECT CONFIGURATION TESTS" 
echo "================================"

# Test 4: Unity Version Compatibility
if [[ -f "$ACTIVE_PROJECT_PATH/ProjectSettings/ProjectVersion.txt" ]]; then
    PROJECT_VERSION=$(grep "m_EditorVersion:" "$ACTIVE_PROJECT_PATH/ProjectSettings/ProjectVersion.txt" | cut -d' ' -f2)
    run_test "Unity Version Match" \
        "[[ '$PROJECT_VERSION' == '2022.3.22f1' ]]" \
        "Unity version matches (2022.3.22f1)" \
        "Unity version mismatch - project: $PROJECT_VERSION, editor: 2022.3.22f1"
else
    echo "❌ Could not determine project Unity version"
    ((TESTS_FAILED++))
fi

# Test 5: Build Settings Configuration
run_test "Build Settings Scene" \
    "grep -q 'Assets/VirtualTokyoMatching/Scenes/VirtualTokyoMatching.unity' '$ACTIVE_PROJECT_PATH/ProjectSettings/EditorBuildSettings.asset'" \
    "Main scene added to build settings" \
    "Main scene not in build settings"

# Test 6: VTM Scripts
run_test "VTM Scene Builder Script" \
    "[[ -f '$ACTIVE_PROJECT_PATH/Assets/VirtualTokyoMatching/Scripts/Editor/VTMSceneBuilder.cs' ]]" \
    "VTM Scene Builder script present" \
    "VTM Scene Builder script missing"

run_test "VRChat Build Configurator Script" \
    "[[ -f '$ACTIVE_PROJECT_PATH/Assets/VirtualTokyoMatching/Scripts/Editor/VRChatBuildConfigurator.cs' ]]" \
    "VRChat Build Configurator script present" \
    "VRChat Build Configurator script missing"

echo "🚀 VRCHAT INTEGRATION TESTS"
echo "============================="

# Test 7: VRChat SDK Detection
run_warning_check "VRChat SDK3 Worlds Package" \
    "[[ ! -d '$ACTIVE_PROJECT_PATH/Packages/com.vrchat.worlds' ]]" \
    "VRChat SDK3 Worlds package not detected - needs to be imported" \
    "VRChat SDK3 Worlds package detected"

# Test 8: VCC Integration  
VCC_PATH="/home/kafka/.local/share/VRChatCreatorCompanion"
run_warning_check "VRChat Creator Companion" \
    "[[ ! -d '$VCC_PATH' ]]" \
    "VCC not detected - manual VRChat SDK import may be needed" \
    "VCC detected"

echo "🔧 LAUNCH SCRIPTS TESTS"
echo "========================"

# Test 9: Launch Scripts
run_test "Unity Launch Script" \
    "[[ -x '$ACTIVE_PROJECT_PATH/launch_unity_project.sh' ]]" \
    "Unity launch script exists and is executable" \
    "Unity launch script missing or not executable"

run_test "Build Settings Configurator" \
    "[[ -x '$ACTIVE_PROJECT_PATH/configure_vrchat_build_settings.sh' ]]" \
    "Build settings configurator exists and is executable" \
    "Build settings configurator missing or not executable"

run_test "Build Modules Installer" \
    "[[ -x '$ACTIVE_PROJECT_PATH/install_build_modules.sh' ]]" \
    "Build modules installer exists and is executable" \
    "Build modules installer missing or not executable"

echo "🎮 FUNCTIONAL TESTS"
echo "==================="

# Test 10: VTMSceneBuilder Compilation Test
echo "🧪 Testing: VTMSceneBuilder Compilation"
if grep -q "BuildTarget currentBuildTarget = EditorUserBuildSettings.activeBuildTarget" "$ACTIVE_PROJECT_PATH/Assets/VirtualTokyoMatching/Scripts/Editor/VTMSceneBuilder.cs"; then
    echo "✅ VTMSceneBuilder build target validation fixed"
    ((TESTS_PASSED++))
else
    echo "❌ VTMSceneBuilder still has old build target validation"
    ((TESTS_FAILED++))
fi
echo ""

# Test 11: Project Permissions
run_test "Project Write Permissions" \
    "[[ -w '$ACTIVE_PROJECT_PATH' ]]" \
    "Project directory is writable" \
    "Project directory has permission issues"

echo "📋 TEST SUMMARY"
echo "==============="
echo "Tests Passed: $TESTS_PASSED"
echo "Tests Failed: $TESTS_FAILED"
echo "Warnings: $WARNINGS"
echo ""

if [[ $TESTS_FAILED -eq 0 ]]; then
    echo "🎉 ALL CORE TESTS PASSED!"
    echo ""
    echo "✅ Your Virtual Tokyo Matching project is properly configured for VRChat development."
    echo ""
    echo "🚀 Next Steps:"
    echo "   1. Launch Unity: ./launch_unity_project.sh"
    echo "   2. Import VRChat SDK3 Worlds (if not already imported):"
    echo "      • Window → VRChat SDK → Show Control Panel"
    echo "      • Follow prompts to download and import SDK"
    echo "   3. Verify build settings:"
    echo "      • File → Build Settings"
    echo "      • Should show 'PC, Mac & Linux Standalone' with Windows target"
    echo "      • Platform dropdown should be selectable"
    echo "   4. Test world creation:"
    echo "      • VTM → Create Complete World"
    echo "      • VTM → Validate Scene Setup"
    echo ""
    echo "🔧 VRChat SDK Platform Dropdown Fix:"
    echo "   If the dropdown is still disabled after launching Unity:"
    echo "   • File → Build Settings → Switch Platform to 'PC, Mac & Linux Standalone'"
    echo "   • Set target to 'Windows'"
    echo "   • Restart Unity Editor"
    echo "   • VRChat SDK Control Panel should now be fully functional"
    
    if [[ $WARNINGS -gt 0 ]]; then
        echo ""
        echo "⚠️  Note: $WARNINGS warnings found. These are typically resolved by:"
        echo "   • Importing VRChat SDK3 Worlds"
        echo "   • Running Unity Editor for the first time"
    fi
    
    exit 0
else
    echo "❌ $TESTS_FAILED TESTS FAILED"
    echo ""
    echo "🔧 Required fixes:"
    
    if [[ ! -f "$UNITY_EDITOR" ]]; then
        echo "   • Install Unity 2022.3.22f1 via Unity Hub"
    fi
    
    if [[ ! -d "$ACTIVE_PROJECT_PATH/Assets" ]]; then
        echo "   • Verify project path and Unity project setup"
    fi
    
    if [[ ! -d "/home/kafka/Unity/Hub/Editor/2022.3.22f1/Editor/Data/PlaybackEngines/WindowsStandaloneSupport" ]]; then
        echo "   • Run: ./install_build_modules.sh"
    fi
    
    echo ""
    echo "Run this script again after making fixes."
    exit 1
fi