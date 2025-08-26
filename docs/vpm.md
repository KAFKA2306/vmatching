# VPM CLI Setup and Configuration Guide

## Overview
This document covers VPM (VRChat Package Manager) CLI setup, configuration, and troubleshooting for Virtual Tokyo Matching development on Ubuntu 22.04 LTS. All issues have been resolved and the setup process is fully automated.

## Prerequisites
- .NET SDK 8.0 or higher
- Unity Hub and Unity Editor 2022.3.22f1 LTS
- Network connectivity for package downloads

## Quick Setup (Automated)

### Using the Setup Script
```bash
# Run the complete automated setup
cd /home/kafka/projects/virtualtokyomatching
./setup_unity_project.sh
```

The script automatically handles:
- VPM CLI installation and configuration
- Unity Hub and Editor detection
- `settings.json` generation with correct paths
- VRChat World project creation
- Package installation and dependency resolution

## Manual Installation

### 1. Install VPM CLI
```bash
# Install VPM CLI as a global .NET tool
dotnet tool install --global vrchat.vpm.cli

# Verify installation
vpm --version
# Expected: 0.1.28+<hash>
```

### 2. Install VPM Templates
```bash
# Install official VRChat project templates
vpm install templates

# List available repositories
vpm list repos
# Expected output:
# com.vrchat.repos.official | Official (VRChat)
# com.vrchat.repos.curated | Curated (VRChat)
```

## Configuration

### Settings.json Generation
The setup script automatically creates a properly formatted `settings.json` at:
`~/.local/share/VRChatCreatorCompanion/settings.json`

### Key Configuration Elements
```json
{
  "pathToUnityHub": "/usr/bin/unityhub",
  "pathToUnityExe": "/home/kafka/Unity/Hub/Editor/2022.3.22f1/Editor/Unity",
  "unityEditors": [
    {
      "version": "2022.3.22f1",
      "path": "/home/kafka/Unity/Hub/Editor/2022.3.22f1/Editor/Unity"
    }
  ],
  "preferredUnityEditors": {
    "2022.3": "2022.3.22f1"
  },
  "defaultProjectPath": "/home/kafka/projects"
}
```

## Project Management

### Creating VRChat World Projects
```bash
# Create new project
vpm new VirtualTokyoMatching World -p ~/projects

# Navigate to project
cd ~/projects/VirtualTokyoMatching

# Verify project structure
vpm check project .
# Expected: "Project is WorldVPM"
```

### Package Installation
```bash
# Install core VRChat packages
vpm add package com.vrchat.worlds -p .
vpm add package com.vrchat.udonsharp -p .
vpm add package com.vrchat.clientsim -p .

# Resolve dependencies
vpm resolve project .

# List installed packages
vpm list packages -p .
```

## Common VPM Commands

### Project Operations
```bash
# Check project status
vpm check project .

# Resolve dependencies
vpm resolve project .

# List packages
vpm list packages -p .

# Add specific package version
vpm add package com.vrchat.udonsharp@1.1.8 -p .

# Remove package
vpm remove package PACKAGE_ID -p .
```

### Repository Management
```bash
# List repositories
vpm list repos

# Check Unity Hub
vpm check hub

# Check Unity installation
vpm check unity
```

## Troubleshooting

### 1. JSON Parsing Errors ✅ FIXED

**Previous Issue**: `Unexpected character encountered while parsing value`
**Root Cause**: Malformed `settings.json` with incorrect array syntax
**Solution**: Automated generation with proper JSON structure

```bash
# If you still encounter JSON errors, regenerate settings:
mv ~/.local/share/VRChatCreatorCompanion/settings.json ~/.local/share/VRChatCreatorCompanion/settings.json.broken
vpm install templates
vpm list repos
```

### 2. Unity Editor Not Detected ✅ FIXED

**Previous Issue**: "Found No Supported Editors"
**Root Cause**: Linux Unity auto-detection limitations
**Solution**: Manual path configuration in `settings.json`

The setup script automatically detects and configures Unity paths.

### 3. Package Compatibility Issues ✅ FIXED

**Previous Issue**: Package version conflicts
**Solution**: Staged installation with specific versions

```bash
# If you encounter compatibility issues:
vpm add package com.vrchat.worlds -p .
vpm resolve project .
vpm add package com.vrchat.udonsharp@1.1.8 -p .
vpm resolve project .
vpm add package com.vrchat.clientsim@1.2.6 -p .
vpm resolve project .
```

### 4. Command Syntax Errors ✅ FIXED

**Previous Issue**: Missing `package` subcommand
**Correct Syntax**:
```bash
# ✅ Correct
vpm add package com.vrchat.worlds -p .

# ❌ Incorrect
vpm add com.vrchat.worlds
```

## VRChat Package Ecosystem

### Core Packages
- **com.vrchat.base**: VRChat Base SDK
- **com.vrchat.worlds**: VRChat Worlds SDK (required)
- **com.vrchat.udonsharp**: UdonSharp scripting system (recommended)
- **com.vrchat.clientsim**: Client simulation for testing

### Package Versions (Tested)
- VRChat Worlds SDK: Latest stable
- UdonSharp: 1.1.8+ (recommended for stability)
- ClientSim: 1.2.6+ (latest testing features)

## Integration with Unity Hub

### Automatic Recognition
VPM-created projects are automatically recognized by Unity Hub:
1. Proper `ProjectSettings/ProjectVersion.txt` generated
2. `Packages/manifest.json` with VRChat dependencies
3. Project appears in Unity Hub's project list

### Manual Addition
If Unity Hub doesn't recognize the project:
1. Open Unity Hub
2. Click "Add" → Navigate to project folder
3. Select the project directory
4. Project will appear in Unity Hub

## Best Practices

### 1. Always Use Project Path Parameter
```bash
# Always specify -p . when in project directory
vpm add package com.vrchat.worlds -p .
vpm resolve project .
vpm check project .
```

### 2. Staged Package Installation
```bash
# Add packages one at a time and resolve
vpm add package com.vrchat.worlds -p .
vpm resolve project .

vpm add package com.vrchat.udonsharp -p .
vpm resolve project .
```

### 3. Regular Validation
```bash
# Regularly check project status
vpm check project .
vpm list packages -p .
```

### 4. Backup Settings
```bash
# Backup settings before changes
cp ~/.local/share/VRChatCreatorCompanion/settings.json ~/.local/share/VRChatCreatorCompanion/settings.json.backup
```

## Advanced Configuration

### Custom Package Sources
```json
{
  "userRepos": [
    {
      "name": "Custom Repository",
      "url": "https://example.com/packages.json",
      "id": "com.example.repo"
    }
  ]
}
```

### Environment Variables
```bash
# Disable VPM telemetry
export VPM_DISABLE_TELEMETRY=1

# Enable verbose logging
export VPM_LOG_LEVEL=debug
```

## Project Structure

### VirtualTokyoMatching Layout
```
Assets/VirtualTokyoMatching/
├── Scripts/
│   ├── Core/                    # PlayerDataManager, VTMController
│   ├── Assessment/              # 112-question diagnosis system
│   ├── Vector/                  # 30D→6D transformation
│   ├── Matching/                # Real-time compatibility
│   ├── UI/                      # User interfaces
│   ├── Safety/                  # Privacy protection
│   ├── Session/                 # 1-on-1 room management
│   ├── Sync/                    # Network synchronization
│   ├── Analysis/                # Personality analysis
│   ├── Performance/             # Optimization systems
│   └── Editor/                  # Unity Editor tools
├── ScriptableObjects/           # Configuration assets
├── Scenes/                      # Unity scenes
├── Prefabs/                     # Reusable components
├── Materials/                   # Materials and shaders
└── Resources/                   # Runtime-loaded assets
```

## Performance Optimization

### VPM Cache Management
```bash
# Clear package cache if needed
rm -rf ~/.local/share/VRChatCreatorCompanion/cache

# Reinstall templates
vpm install templates
```

### Dependency Resolution
```bash
# Force dependency resolution
vpm resolve project . --force

# Check for package updates
vpm list packages -p . --outdated
```

## CI/CD Integration

### Automated Setup for CI
```bash
# Install VPM in CI environment
dotnet tool install --global vrchat.vpm.cli

# Install templates
vpm install templates

# Create settings.json programmatically
cat > ~/.local/share/VRChatCreatorCompanion/settings.json << EOF
{
  "pathToUnityHub": "/usr/bin/unityhub",
  "pathToUnityExe": "/opt/unity/Editor/Unity",
  "userProjects": [],
  "unityEditors": [
    {
      "version": "2022.3.22f1",
      "path": "/opt/unity/Editor/Unity"
    }
  ]
}
EOF

# Restore project dependencies
vpm resolve project .
```

## Alternative Tools

### vrc-get (Rust-based Alternative)
```bash
# Install vrc-get
curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh
source ~/.cargo/env
cargo install vrc-get

# Usage
vrc-get new VirtualTokyoMatching --template world
cd VirtualTokyoMatching
vrc-get add com.vrchat.udonsharp
```

## Status Summary

### ✅ Working Features
- VPM CLI installation and configuration
- Project creation and package management
- Unity Hub integration
- Automated setup scripts
- Dependency resolution
- Package version management

### 🔧 Fixed Issues
- JSON parsing errors in settings.json
- Unity Editor detection on Linux
- Package compatibility conflicts
- Command syntax errors
- Project recognition problems

### 🎯 Virtual Tokyo Matching Integration
The VPM setup enables:
- 112-question personality assessment system
- 30D→6D vector compression for privacy
- Real-time compatibility matching
- Progressive matching with incomplete questionnaires
- 1-on-1 session room management
- Performance optimization for PC (≥72FPS) and Quest (≥60FPS)

This comprehensive VPM guide ensures smooth Virtual Tokyo Matching development with reliable package management and troubleshooting capabilities.