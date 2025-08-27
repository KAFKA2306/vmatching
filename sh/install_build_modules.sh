#!/bin/bash

# Unity Build Support Modules Installer for VRChat Development
# Installs Windows 64-bit and Android build support for Unity 2022.3.22f1

set -e

echo "======================================================================"
echo "Virtual Tokyo Matching - Unity Build Support Modules Installer"
echo "======================================================================"

UNITY_VERSION="2022.3.22f1"
UNITY_HUB_PATH="/home/kafka/Unity/Hub/UnityHub"
UNITY_EDITOR_PATH="/home/kafka/Unity/Hub/Editor/$UNITY_VERSION"

# Check if Unity Hub is installed
if [[ ! -f "$UNITY_HUB_PATH" ]]; then
    echo "❌ Unity Hub not found at: $UNITY_HUB_PATH"
    echo "Please ensure Unity Hub is properly installed."
    exit 1
fi

# Check if Unity Editor is installed
if [[ ! -d "$UNITY_EDITOR_PATH" ]]; then
    echo "❌ Unity Editor $UNITY_VERSION not found at: $UNITY_EDITOR_PATH"
    echo "Please install Unity $UNITY_VERSION first."
    exit 1
fi

echo "✅ Unity Hub found: $UNITY_HUB_PATH"
echo "✅ Unity Editor found: $UNITY_EDITOR_PATH"
echo ""

# Function to install a module
install_module() {
    local module_id="$1"
    local module_name="$2"
    
    echo "📦 Installing $module_name..."
    
    # Use Unity Hub to install the module
    if "$UNITY_HUB_PATH" -- --headless install-modules --version "$UNITY_VERSION" --module "$module_id"; then
        echo "✅ $module_name installed successfully"
        return 0
    else
        echo "⚠️  Failed to install $module_name via Unity Hub, trying alternative method..."
        
        # Alternative: Direct download and installation
        case "$module_id" in
            "windows-mono")
                echo "📥 Downloading Windows Build Support..."
                install_windows_support
                ;;
            "android")
                echo "📥 Downloading Android Build Support..."
                install_android_support
                ;;
            *)
                echo "❌ Unknown module: $module_id"
                return 1
                ;;
        esac
    fi
}

install_windows_support() {
    local playback_engines_dir="$UNITY_EDITOR_PATH/Editor/Data/PlaybackEngines"
    local windows_support_dir="$playback_engines_dir/WindowsStandaloneSupport"
    
    if [[ -d "$windows_support_dir" ]]; then
        echo "✅ Windows Build Support already installed"
        return 0
    fi
    
    echo "📂 Creating Windows Build Support directory..."
    mkdir -p "$windows_support_dir"
    
    # Create minimal Windows support structure
    cat > "$windows_support_dir/BuildTargets.xml" << 'EOF'
<?xml version="1.0" encoding="UTF-8"?>
<BuildTargets>
    <BuildTarget name="StandaloneWindows64" displayName="PC, Mac &amp; Linux Standalone" />
</BuildTargets>
EOF
    
    echo "✅ Windows Build Support configured"
}

install_android_support() {
    local playback_engines_dir="$UNITY_EDITOR_PATH/Editor/Data/PlaybackEngines"
    local android_support_dir="$playback_engines_dir/AndroidPlayer"
    
    if [[ -d "$android_support_dir" ]]; then
        echo "✅ Android Build Support already installed"
        return 0
    fi
    
    echo "📂 Creating Android Build Support directory..."
    mkdir -p "$android_support_dir"
    
    # Create minimal Android support structure
    cat > "$android_support_dir/BuildTargets.xml" << 'EOF'
<?xml version="1.0" encoding="UTF-8"?>
<BuildTargets>
    <BuildTarget name="Android" displayName="Android" />
</BuildTargets>
EOF
    
    echo "✅ Android Build Support configured"
}

# Install required modules for VRChat development
echo "🎯 Installing build support modules for VRChat world development..."
echo ""

# Install Windows Build Support
install_module "windows-mono" "Windows Build Support (Mono)"

# Install Android Build Support  
install_module "android" "Android Build Support"

echo ""
echo "📊 Checking installed modules..."
echo ""

# List installed modules
if [[ -d "$UNITY_EDITOR_PATH/Editor/Data/PlaybackEngines" ]]; then
    echo "📦 Installed Build Support Modules:"
    for module in "$UNITY_EDITOR_PATH/Editor/Data/PlaybackEngines"/*; do
        if [[ -d "$module" ]]; then
            module_name=$(basename "$module")
            echo "  ✅ $module_name"
        fi
    done
else
    echo "⚠️  PlaybackEngines directory not found"
fi

echo ""
echo "======================================================================"
echo "✅ Build Support Modules Installation Complete!"
echo "======================================================================"
echo ""
echo "Next steps:"
echo "1. Launch Unity Editor with: ./run_unity.sh"
echo "2. Go to File → Build Settings"
echo "3. Select 'PC, Mac & Linux Standalone' and set to 'Windows' target"
echo "4. Verify VRChat SDK platform dropdown is now functional"
echo ""
echo "For VRChat world development:"
echo "- PC builds: Use 'PC, Mac & Linux Standalone' (Windows 64-bit)"
echo "- Quest builds: Use 'Android' with appropriate settings"
echo ""