# Virtual Tokyo Matching - Project Setup Guide

## Overview
This document provides comprehensive setup instructions for the Virtual Tokyo Matching Unity project using VPM CLI on Ubuntu 22.04 LTS. All setup issues have been resolved and the automated build pipeline is fully functional.

## Prerequisites
- **OS**: Ubuntu 22.04 LTS with network connectivity
- **Unity**: Unity Hub + Unity Editor 2022.3.22f1 LTS installed
- **.NET SDK**: Latest version (required for VPM CLI)
- **Git**: For version control and project management

## Quick Start (Automated Setup) ✅ COMPLETED

**Status**: Unity project successfully set up on August 26, 2024

### 1. Unity Project Ready
The Unity project has been automatically configured at:
```
/home/kafka/projects/VirtualTokyoMatching
```

**Included Components:**
- Unity 2022.3.22f1 LTS with VRChat SDK3 Worlds v3.7.6
- VPM (VRChat Package Manager) CLI integration
- UdonSharp runtime with all VTM scripts
- Automated build tools and VRChat optimization fixes

### 2. Launch Unity Project
```bash
# Navigate to project directory
cd /home/kafka/projects/VirtualTokyoMatching

# Launch Unity Editor (automated script available)
./launch_unity.sh

# Alternative: Direct Unity launch
/home/kafka/Unity/Hub/Editor/2022.3.22f1/Editor/Unity -projectPath .
```

### 3. VRChat Fixes Applied
**Resolved Issues:**
- ✅ UI Canvas player-following issue (converted to World Space)
- ✅ Floor material color changes (stable white Unlit materials)
- ✅ Quest UI optimization (wall-mounted design, enhanced visibility)
- ✅ Build automation tools and validation system

## Manual Setup (Step by Step)

### 1. VPM CLI Installation and Configuration

#### Install VPM CLI
```bash
# Install VPM CLI globally
dotnet tool install --global vrchat.vpm.cli

# Install VPM templates
vpm install templates

# Verify installation
vpm --version
```

#### Configure VPM Settings
The setup script automatically creates proper `settings.json` with correct paths:
```json
{
  "pathToUnityHub": "/usr/bin/unityhub",
  "pathToUnityExe": "/home/kafka/Unity/Hub/Editor/2022.3.22f1/Editor/Unity",
  "unityEditors": [
    {
      "version": "2022.3.22f1",
      "path": "/home/kafka/Unity/Hub/Editor/2022.3.22f1/Editor/Unity"
    }
  ]
}
```

#### Fix Common VPM Issues
If you encounter JSON parsing errors:
```bash
# Backup and regenerate settings
mv ~/.local/share/VRChatCreatorCompanion/settings.json ~/.local/share/VRChatCreatorCompanion/settings.json.broken
vpm install templates
vpm list repos
```

### 2. Unity Project Creation

#### Create VRChat World Project
```bash
# Create project directory
mkdir -p ~/projects
cd ~/projects

# Create VRChat World project
vpm new VirtualTokyoMatching World -p ~/projects
cd VirtualTokyoMatching

# Verify project structure
vpm check project .
```

#### Install Required Packages
```bash
# Install VRChat Worlds SDK
vpm add package com.vrchat.worlds -p .

# Install UdonSharp
vpm add package com.vrchat.udonsharp -p .

# Install ClientSim for testing
vpm add package com.vrchat.clientsim -p .

# Resolve all dependencies
vpm resolve project .
```

### 3. Project Structure Setup

#### Create Asset Folders
```bash
cd Assets
mkdir -p VirtualTokyoMatching/{Scripts,ScriptableObjects,Prefabs,Materials,Scenes,Resources,Audio,Textures}
mkdir -p VirtualTokyoMatching/Scripts/{Core,Assessment,Vector,Matching,UI,Safety,Session,Sync,Analysis,Performance,Editor}
mkdir -p VirtualTokyoMatching/Prefabs/{UI,SessionRooms,Systems}
```

#### Copy VTM Scripts
All VTM scripts are already included in the repository under `Assets/VirtualTokyoMatching/Scripts/`.

### 4. Unity Tags Configuration

The setup script automatically adds required tags to `ProjectSettings/TagManager.asset`:
- `Floor`
- `Wall`
- `RoomFloor`
- `Furniture`
- `SpawnMarker`

### 5. Scene Generation (Automated)

#### Using VTM Scene Builder
The `vtm_complete_setup.sh` script automatically:
1. Creates the complete world structure (Lobby + 3 Session Rooms)
2. Sets up materials and lighting
3. Applies VRChat optimizations
4. Validates the complete setup

#### Generated Content
- **Scene**: `Assets/VirtualTokyoMatching/Scenes/VirtualTokyoMatching.unity`
- **Environment**: 20x20m Lobby + 3x 10x10m Session Rooms
- **Spawn Points**: 10 total with proper VRChat compliance
- **UI Systems**: 4 canvas systems with Japanese localization
- **Lighting**: Directional sun + ambient + per-room lighting
- **Systems**: VTM Controller + 30 networked profile slots

## Unity Hub Integration

### Adding Project to Unity Hub
1. Open Unity Hub
2. Click "Add" or "Open"
3. Navigate to `/home/kafka/projects/VirtualTokyoMatching`
4. Select the project folder
5. The project will appear in Unity Hub's project list

### Launching from Unity Hub
- Simply click on "VirtualTokyoMatching" in Unity Hub's project list
- Unity Editor will open with the project loaded

## VRChat SDK Integration

### Import VRChat SDK3 Worlds
1. Open Unity Editor with the VTM project
2. Go to **Window → VRChat SDK → Show Control Panel**
3. Follow SDK installation instructions if not already imported
4. SDK packages are automatically included via VPM

### Configure VRC Scene Descriptor
1. Find `VRCWorld` GameObject in the scene hierarchy
2. Add Component → `VRC_SceneDescriptor`
3. Configure spawn points from `SpawnSystem` children
4. Set respawn height and other VRChat-specific settings

## ScriptableObject Configuration

### Required Assets in Resources/
Create these ScriptableObjects with configuration data:
- **QuestionDatabase**: 112 personality assessment questions
- **VectorConfiguration**: 112→30D transformation matrix (W), 30→6D projection matrix (P)
- **SummaryTemplates**: Personality description templates
- **PerformanceSettings**: FPS targets and computation budgets

### Sample Configuration Files
Template files are included in `Assets/VirtualTokyoMatching/Resources/`:
- `SampleQuestionDatabase.json`
- `VectorConfigurationTemplate.json`
- `SummaryTemplatesConfiguration.json`
- `PerformanceSettingsTemplate.json`

## Performance Targets

### PC Build (Windows 64-bit)
- **Target**: ≥72 FPS
- **Memory**: <200MB
- **Texture Limit**: 2048x2048
- **Compatibility Recalc**: ≤5 seconds

### Quest Build (Android)
- **Target**: ≥60 FPS
- **Memory**: <100MB
- **Texture Limit**: 1024x1024
- **Compatibility Recalc**: ≤10 seconds

## Testing Workflow

### Local Testing
1. Use Unity Play mode for basic functionality
2. Test UI transitions, data persistence, recommendation updates
3. Verify public/private profile toggles work correctly

### Multi-Client Testing
1. Use VRChat SDK's ClientSim for multiplayer testing
2. Test synchronization between multiple users
3. Verify compatibility calculations and recommendations
4. Test session room invitations and timers

### VRChat Deployment
1. Build for target platform (PC/Quest)
2. Upload via VRChat SDK Control Panel
3. Test in Friends+ mode for 1 week
4. Deploy to Public after validation

## Troubleshooting

### Common Issues and Solutions

#### Unity Hub Not Recognizing Project
- **Cause**: Missing `ProjectSettings/` folder
- **Solution**: Run `./setup_unity_project.sh` to create proper Unity project structure

#### VPM CLI JSON Parsing Errors
- **Cause**: Malformed `settings.json`
- **Solution**: Remove and regenerate: `mv ~/.local/share/VRChatCreatorCompanion/settings.json{,.broken} && vpm install templates`

#### Build Target Validation Errors
- **Cause**: VTMSceneBuilder expecting Windows64 on Linux
- **Solution**: Fixed in current version - validation now accepts Linux64 builds

#### Tag Not Found Errors
- **Cause**: Missing Unity tags for materials
- **Solution**: Tags automatically added by setup script to `TagManager.asset`

## Available Scripts

### Setup Scripts
- **`setup_unity_project.sh`**: Complete project setup from scratch
- **`vtm_complete_setup.sh`**: Generate complete world structure
- **`vtm_headless_build.sh`**: Headless world building
- **`run_unity.sh`**: Launch Unity Editor with project

### Utility Scripts
- **VTM/Create Complete World**: Unity menu item for manual world creation
- **VTM/Setup Materials**: Unity menu item for material configuration
- **VTM/Optimize For VRChat**: Unity menu item for VRChat optimization
- **VTM/Validate Scene Setup**: Unity menu item for scene validation

## Next Steps

1. **Open Unity Editor**: Use `./run_unity.sh` or Unity Hub
2. **Import VRChat SDK**: Window → VRChat SDK → Show Control Panel
3. **Configure VRC_SceneDescriptor**: Add to VRCWorld GameObject
4. **Add UdonSharp Components**: Wire up VTM system components
5. **Test and Deploy**: Use ClientSim → Friends+ → Public progression

## Architecture Overview

### Core Systems
- **PlayerDataManager**: Persistent user data (questions 1-112, 30D vectors)
- **DiagnosisController**: 112-question personality assessment UI
- **VectorBuilder**: Incremental vector updates and normalization
- **CompatibilityCalculator**: Cosine similarity computation
- **PublicProfilePublisher**: 6D compressed data broadcasting
- **RecommenderUI**: Top 3 compatibility matches display

### Data Privacy
- **Private**: Raw answers (112 questions) + 30D vectors
- **Public**: 6D compressed vectors + auto-generated tags/headlines
- **Progressive**: Provisional matching with incomplete questionnaires
- **Consensual**: Public sharing requires explicit opt-in

### Performance Architecture
- **Frame Budget**: Distributed processing with K operations/frame limit
- **Event-Driven**: Recalculation triggered by answer updates only
- **Incremental**: Provisional vectors during assessment, normalized on completion
- **Memory Optimized**: PC<200MB, Quest<100MB constraints

This setup guide provides a complete, tested workflow for Virtual Tokyo Matching development on Ubuntu 22.04 with Unity 2022.3 LTS and VRChat SDK3.