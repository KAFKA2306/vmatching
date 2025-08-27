#!/bin/bash

# Virtual Tokyo Matching - Unity Project Setup Script
# Based on Ubuntu 22.04 VCC/VPM CLI Complete Guide

set -e  # Exit on any error

echo "=== Virtual Tokyo Matching - Unity Project Setup ==="
echo "Setting up VRChat world development environment..."

# Configuration
PROJECT_NAME="VirtualTokyoMatching"
PROJECT_PATH="$HOME/projects/$PROJECT_NAME"
VCC_SETTINGS_PATH="$HOME/.local/share/VRChatCreatorCompanion/settings.json"

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

print_status() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Phase 1: Prerequisites Check
echo
echo "=== Phase 1: Prerequisites Check ==="

print_status "Checking .NET SDK..."
if ! command -v dotnet &> /dev/null; then
    print_error ".NET SDK not found. Please install .NET 8.0 or higher."
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
print_status ".NET SDK version: $DOTNET_VERSION"

print_status "Checking Unity Hub..."
if ! command -v unityhub &> /dev/null; then
    print_error "Unity Hub not found at /usr/bin/unityhub"
    print_error "Please install Unity Hub and ensure it's in PATH"
    exit 1
fi

UNITY_HUB_PATH=$(which unityhub)
print_status "Unity Hub found at: $UNITY_HUB_PATH"

print_status "Checking Unity Editor..."
UNITY_EDITOR_PATH=$(find ~/Unity/Hub/Editor -name "Unity" -type f 2>/dev/null | head -1)
if [ -z "$UNITY_EDITOR_PATH" ]; then
    print_error "Unity Editor not found in ~/Unity/Hub/Editor/"
    print_error "Please install Unity 2022.3.22f1 LTS or similar via Unity Hub"
    exit 1
fi

UNITY_VERSION=$(basename $(dirname $(dirname $UNITY_EDITOR_PATH)))
print_status "Unity Editor found: $UNITY_VERSION at $UNITY_EDITOR_PATH"

# Phase 2: VPM CLI Setup
echo
echo "=== Phase 2: VPM CLI Setup ==="

print_status "Checking VPM CLI installation..."
if ! command -v vpm &> /dev/null; then
    print_status "Installing VPM CLI..."
    dotnet tool install --global vrchat.vpm.cli
else
    VPM_VERSION=$(vpm --version)
    print_status "VPM CLI found: $VPM_VERSION"
fi

print_status "Installing VPM templates..."
vpm install templates

print_status "Listing available repositories..."
vpm list repos

# Phase 3: VCC Settings Configuration
echo
echo "=== Phase 3: VCC Settings Configuration ==="

print_status "Creating VCC settings directory..."
mkdir -p "$HOME/.local/share/VRChatCreatorCompanion"

if [ -f "$VCC_SETTINGS_PATH" ]; then
    BACKUP_PATH="${VCC_SETTINGS_PATH}.backup.$(date +%Y%m%d_%H%M%S)"
    print_status "Backing up existing settings to: $BACKUP_PATH"
    cp "$VCC_SETTINGS_PATH" "$BACKUP_PATH"
fi

print_status "Generating VCC settings.json..."
cat > "$VCC_SETTINGS_PATH" << EOF
{
  "pathToUnityHub": "$UNITY_HUB_PATH",
  "pathToUnityExe": "$UNITY_EDITOR_PATH",
  "userProjects": [],
  "unityEditors": [
    {
      "version": "$UNITY_VERSION",
      "path": "$UNITY_EDITOR_PATH"
    }
  ],
  "preferredUnityEditors": {
    "${UNITY_VERSION%.*}": "$UNITY_VERSION"
  },
  "defaultProjectPath": "$HOME/projects",
  "lastUIState": 0,
  "skipUnityAutoFind": false,
  "userPackageFolders": [],
  "windowSizeData": {
    "width": 0,
    "height": 0,
    "x": 0,
    "y": 0
  },
  "skipRequirements": false,
  "lastNewsUpdate": "$(date -u +%Y-%m-%dT%H:%M:%S.000Z)",
  "allowPii": false,
  "projectBackupPath": "$HOME/.local/share/VRChatCreatorCompanion/ProjectBackups",
  "showPrereleasePackages": false,
  "trackCommunityRepos": true,
  "selectedProviders": 3,
  "userRepos": []
}
EOF

chmod 644 "$VCC_SETTINGS_PATH"

print_status "Validating settings.json..."
if python3 -m json.tool "$VCC_SETTINGS_PATH" >/dev/null 2>&1; then
    print_status "Settings JSON is valid"
else
    print_error "Settings JSON is invalid"
    exit 1
fi

# Phase 4: Project Creation
echo
echo "=== Phase 4: VirtualTokyoMatching Project Creation ==="

print_status "Creating projects directory..."
mkdir -p "$HOME/projects"

if [ -d "$PROJECT_PATH" ]; then
    print_warning "Project directory already exists: $PROJECT_PATH"
    read -p "Continue and potentially overwrite? (y/N): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        print_status "Setup cancelled by user"
        exit 0
    fi
fi

print_status "Creating VRChat World project..."
cd "$HOME/projects"
vpm new "$PROJECT_NAME" World -p "$HOME/projects"

# Verify project creation
if [ ! -d "$PROJECT_PATH" ]; then
    print_error "Project creation failed"
    exit 1
fi

cd "$PROJECT_PATH"

print_status "Verifying project structure..."
vpm check project .

# Phase 5: Package Installation
echo
echo "=== Phase 5: VRChat SDK Package Installation ==="

print_status "Installing VRChat Worlds SDK..."
vpm add package com.vrchat.worlds -p .

print_status "Resolving dependencies..."
vpm resolve project .

print_status "Installing UdonSharp..."
if ! vpm add package com.vrchat.udonsharp -p .; then
    print_warning "UdonSharp installation failed, trying specific version..."
    vpm add package com.vrchat.udonsharp@1.1.8 -p .
fi

vpm resolve project .

print_status "Installing ClientSim for testing..."
if ! vpm add package com.vrchat.clientsim -p .; then
    print_warning "ClientSim installation failed, trying specific version..."
    vpm add package com.vrchat.clientsim@1.2.6 -p .
fi

vpm resolve project .

print_status "Final dependency resolution..."
vpm resolve project .

print_status "Listing installed packages..."
vpm list packages -p .

# Phase 6: Project Structure Setup
echo
echo "=== Phase 6: Project Structure Setup ==="

print_status "Creating VirtualTokyoMatching folder structure..."

# Create main asset folders
mkdir -p "Assets/VirtualTokyoMatching/"{Scripts,ScriptableObjects,Prefabs,Materials,Scenes,Resources,Audio,Textures}

# Create script subfolders
mkdir -p "Assets/VirtualTokyoMatching/Scripts/"{Core,Assessment,Vector,Matching,UI,Safety,Session,Sync,Analysis,Performance}

# Create prefab subfolders
mkdir -p "Assets/VirtualTokyoMatching/Prefabs/"{UI,SessionRooms,Systems}

print_status "Copying configuration templates to Resources..."
if [ -d "../Assets/VirtualTokyoMatching/Resources" ]; then
    cp ../Assets/VirtualTokyoMatching/Resources/*.json Assets/VirtualTokyoMatching/Resources/ 2>/dev/null || print_warning "No configuration templates found to copy"
fi

# Phase 7: Unity Project Launch
echo
echo "=== Phase 7: Unity Project Launch ==="

print_status "Project setup complete!"
echo
echo "Project Location: $PROJECT_PATH"
echo "Unity Version: $UNITY_VERSION"
echo "VPM CLI Version: $(vpm --version)"
echo

print_status "To open the project in Unity:"
echo "Option 1: Unity Hub GUI - Add project from $PROJECT_PATH"
echo "Option 2: Command line - $UNITY_HUB_PATH -- --projectPath \"$PROJECT_PATH\""
echo

print_status "Next Steps:"
echo "1. Open project in Unity Editor"
echo "2. Import VirtualTokyoMatching scripts from the repository"
echo "3. Configure ScriptableObjects with your personality data"
echo "4. Set up the main scene using SCENE_SETUP.md guide"
echo "5. Test with ClientSim multi-client setup"
echo

# Optional: Launch Unity Hub
read -p "Launch Unity Hub now? (y/N): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    print_status "Launching Unity Hub..."
    "$UNITY_HUB_PATH" -- --projectPath "$PROJECT_PATH" &
    print_status "Unity Hub launched with project loaded"
fi

print_status "Setup completed successfully!"
print_status "Check setup_log_$(date +%Y%m%d_%H%M%S).txt for detailed output"

echo
echo "=== Virtual Tokyo Matching Setup Complete ==="