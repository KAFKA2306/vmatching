#!/bin/bash

# Virtual Tokyo Matching - VRChat Build Settings Configurator
# Configures Unity project for VRChat world development with proper build targets

set -e

echo "======================================================================"
echo "Virtual Tokyo Matching - VRChat Build Settings Configurator"
echo "======================================================================"

PROJECT_PATH="/home/kafka/projects/VirtualTokyoMatching"
PROJECT_PATH_ALT="/home/kafka/projects/virtualtokyomatching"
UNITY_EDITOR="/home/kafka/Unity/Hub/Editor/2022.3.22f1/Editor/Unity"

# Determine correct project path
if [[ -d "$PROJECT_PATH" ]]; then
    ACTIVE_PROJECT_PATH="$PROJECT_PATH"
    echo "✅ Using project path: $PROJECT_PATH"
elif [[ -d "$PROJECT_PATH_ALT" ]]; then
    ACTIVE_PROJECT_PATH="$PROJECT_PATH_ALT"  
    echo "✅ Using project path: $PROJECT_PATH_ALT"
else
    echo "❌ Project not found. Please ensure the project exists."
    exit 1
fi

echo ""

# Function to update Unity project settings via command line
configure_build_settings() {
    echo "⚙️  Configuring build settings for VRChat..."
    
    # Create Unity method to configure build settings
    EDITOR_SCRIPT_DIR="$ACTIVE_PROJECT_PATH/Assets/VirtualTokyoMatching/Scripts/Editor"
    mkdir -p "$EDITOR_SCRIPT_DIR"
    
    cat > "$EDITOR_SCRIPT_DIR/VRChatBuildConfigurator.cs" << 'EOF'
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace VirtualTokyoMatching.Editor
{
    public class VRChatBuildConfigurator
    {
        [MenuItem("VTM/Configure VRChat Build Settings")]
        public static void ConfigureVRChatBuildSettings()
        {
            Debug.Log("[VTM Build Config] Configuring build settings for VRChat worlds...");
            
            // Set build target to Windows 64-bit for PC builds
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64)
            {
                Debug.Log("[VTM Build Config] Switching to Windows 64-bit build target...");
                EditorUserBuildSettings.SwitchActiveBuildTarget(
                    BuildTargetGroup.Standalone, 
                    BuildTarget.StandaloneWindows64
                );
            }
            
            // Configure standalone player settings
            ConfigureStandaloneSettings();
            
            // Configure Android settings for Quest builds
            ConfigureAndroidSettings();
            
            // Add scene to build settings
            AddSceneToBuildSettings();
            
            Debug.Log("[VTM Build Config] VRChat build settings configuration completed!");
        }
        
        static void ConfigureStandaloneSettings()
        {
            Debug.Log("[VTM Build Config] Configuring standalone (PC) settings...");
            
            // Set Windows as the standalone build target
            EditorUserBuildSettings.selectedStandaloneTarget = BuildTarget.StandaloneWindows64;
            
            // Configure player settings for VRChat
            PlayerSettings.companyName = "VirtualTokyoMatching";
            PlayerSettings.productName = "VirtualTokyoMatching";
            
            // Performance settings for VRChat
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.gpuSkinning = true;
            PlayerSettings.graphicsJobs = true;
            
            // VRChat-specific settings
            PlayerSettings.runInBackground = true;
            PlayerSettings.captureSingleScreen = false;
            PlayerSettings.muteOtherAudioSources = false;
            
            // Graphics settings
            PlayerSettings.use32BitDisplayBuffer = true;
            PlayerSettings.preserveFramebufferAlpha = false;
            
            Debug.Log("[VTM Build Config] Standalone settings configured");
        }
        
        static void ConfigureAndroidSettings()
        {
            Debug.Log("[VTM Build Config] Configuring Android (Quest) settings...");
            
            // Android settings for Quest
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            
            // Graphics settings for Quest
            PlayerSettings.Android.startInFullscreen = true;
            PlayerSettings.Android.renderOutsideSafeArea = true;
            
            // Performance settings for Quest
            PlayerSettings.Android.blitType = AndroidBlitType.Never;
            
            Debug.Log("[VTM Build Config] Android settings configured");
        }
        
        static void AddSceneToBuildSettings()
        {
            Debug.Log("[VTM Build Config] Adding scenes to build settings...");
            
            string scenePath = "Assets/VirtualTokyoMatching/Scenes/VirtualTokyoMatching.unity";
            
            var scenes = EditorBuildSettings.scenes.ToList();
            
            // Check if scene already exists in build settings
            bool sceneExists = scenes.Any(s => s.path == scenePath);
            
            if (!sceneExists)
            {
                // Add scene if it exists
                if (System.IO.File.Exists(scenePath))
                {
                    scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                    EditorBuildSettings.scenes = scenes.ToArray();
                    Debug.Log($"[VTM Build Config] Added scene to build: {scenePath}");
                }
                else
                {
                    Debug.LogWarning($"[VTM Build Config] Scene not found: {scenePath}");
                }
            }
            else
            {
                Debug.Log($"[VTM Build Config] Scene already in build settings: {scenePath}");
            }
        }
        
        [MenuItem("VTM/Set Build Target PC")]
        public static void SetBuildTargetPC()
        {
            Debug.Log("[VTM Build Config] Switching to PC build target...");
            EditorUserBuildSettings.SwitchActiveBuildTarget(
                BuildTargetGroup.Standalone, 
                BuildTarget.StandaloneWindows64
            );
            Debug.Log("[VTM Build Config] Build target set to PC (Windows 64-bit)");
        }
        
        [MenuItem("VTM/Set Build Target Quest")]
        public static void SetBuildTargetQuest()
        {
            Debug.Log("[VTM Build Config] Switching to Quest build target...");
            EditorUserBuildSettings.SwitchActiveBuildTarget(
                BuildTargetGroup.Android, 
                BuildTarget.Android
            );
            Debug.Log("[VTM Build Config] Build target set to Quest (Android)");
        }
        
        [MenuItem("VTM/Validate VRChat Compatibility")]
        public static void ValidateVRChatCompatibility()
        {
            Debug.Log("[VTM Build Config] Validating VRChat compatibility...");
            
            int issues = 0;
            
            // Check build target
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64 &&
                EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                Debug.LogWarning("[VTM Build Config] Build target should be Windows 64-bit or Android for VRChat");
                issues++;
            }
            else
            {
                Debug.Log($"[VTM Build Config] ✅ Build target OK: {EditorUserBuildSettings.activeBuildTarget}");
            }
            
            // Check color space
            if (PlayerSettings.colorSpace != ColorSpace.Linear)
            {
                Debug.LogWarning("[VTM Build Config] Linear color space recommended for VRChat");
                issues++;
            }
            else
            {
                Debug.Log("[VTM Build Config] ✅ Color space OK: Linear");
            }
            
            // Check VRChat SDK
            bool vrcSdkFound = false;
            #if VRC_SDK_VRCSDK3
            vrcSdkFound = true;
            #endif
            
            if (vrcSdkFound)
            {
                Debug.Log("[VTM Build Config] ✅ VRChat SDK3 detected");
            }
            else
            {
                Debug.LogWarning("[VTM Build Config] VRChat SDK3 not detected - please import SDK");
                issues++;
            }
            
            if (issues == 0)
            {
                Debug.Log("[VTM Build Config] ✅ VRChat compatibility validation passed!");
            }
            else
            {
                Debug.LogWarning($"[VTM Build Config] ⚠️  Found {issues} compatibility issues");
            }
        }
    }
}
EOF
    
    echo "✅ VRChat build configurator script created"
}

# Function to run Unity in batch mode to configure settings
run_unity_configuration() {
    echo "🚀 Running Unity configuration in batch mode..."
    
    # Create log file
    LOG_FILE="$ACTIVE_PROJECT_PATH/unity_build_config.log"
    
    # Run Unity in batch mode to execute configuration
    "$UNITY_EDITOR" \
        -batchmode \
        -projectPath "$ACTIVE_PROJECT_PATH" \
        -executeMethod VirtualTokyoMatching.Editor.VRChatBuildConfigurator.ConfigureVRChatBuildSettings \
        -logFile "$LOG_FILE" \
        -quit
    
    local exit_code=$?
    
    echo ""
    echo "📋 Configuration Results:"
    
    if [[ $exit_code -eq 0 ]]; then
        echo "✅ Unity build configuration completed successfully"
        
        # Show relevant log entries
        if [[ -f "$LOG_FILE" ]]; then
            echo ""
            echo "📄 Configuration Log (last 20 lines):"
            tail -20 "$LOG_FILE" | grep -E "(VTM Build Config|Build target|configured)"
        fi
    else
        echo "❌ Unity build configuration failed (exit code: $exit_code)"
        
        if [[ -f "$LOG_FILE" ]]; then
            echo ""
            echo "❌ Error Log:"
            tail -10 "$LOG_FILE"
        fi
        
        return $exit_code
    fi
}

# Check Unity Editor availability
if [[ ! -f "$UNITY_EDITOR" ]]; then
    echo "❌ Unity Editor not found at: $UNITY_EDITOR"
    echo "Please ensure Unity is properly installed."
    exit 1
fi

# Run configuration steps
echo "🔧 Step 1: Creating build configuration script..."
configure_build_settings

echo ""
echo "🔧 Step 2: Executing Unity build configuration..."
run_unity_configuration

echo ""
echo "======================================================================"
echo "✅ VRChat Build Settings Configuration Complete!"
echo "======================================================================"
echo ""
echo "📋 What was configured:"
echo "   • Build target set to Windows 64-bit for PC builds"
echo "   • VRChat-compatible player settings applied"
echo "   • Scene added to build settings"
echo "   • Android settings configured for Quest builds"
echo ""
echo "🎯 Next steps:"
echo "   1. Launch Unity Editor: ./launch_unity_project.sh"
echo "   2. Verify File → Build Settings shows correct platform"
echo "   3. Import VRChat SDK3 Worlds if not already present"
echo "   4. Use VTM menu items for further configuration"
echo ""
echo "🔧 Manual verification:"
echo "   • File → Build Settings → Platform should show 'PC, Mac & Linux Standalone'"
echo "   • Target should be set to 'Windows'"
echo "   • VRChat SDK Control Panel should be accessible"
echo ""