# Virtual Tokyo Matching - Development Commands

## Essential Commands for Development

### VPM CLI Operations
```bash
# Project status and verification
vpm --version                    # Check VPM CLI version
vpm check project .             # Verify project structure
vpm list packages -p .          # List installed packages
vpm resolve project .           # Resolve dependencies

# Package management
vpm add package com.vrchat.worlds -p .
vpm add package com.vrchat.udonsharp -p .
vpm add package com.vrchat.clientsim -p .
vpm remove package <ID> -p .

# Diagnostics
vpm check hub                    # Verify Unity Hub integration
vpm check unity                  # Check Unity Editor paths
vpm list repos                   # List package repositories
```

### Unity Operations
```bash
# Project startup
/usr/bin/unityhub -- --projectPath ~/projects/VirtualTokyoMatching

# Direct Unity Editor (if Hub unavailable)
~/Unity/Hub/Editor/2022.3.22f1/Editor/Unity -projectPath ~/projects/VirtualTokyoMatching

# Version check
~/Unity/Hub/Editor/2022.3.22f1/Editor/Unity --version

# Headless operations (batch mode)
Unity -quit -batchmode -nographics -logFile ./compile_check.log -projectPath .
```

### Automated Setup Scripts
```bash
# Complete project setup (Linux/macOS)
./setup_unity_project.sh

# Complete project setup (Windows)
./setup_unity_project.ps1

# Full world creation and optimization
./vtm_complete_setup.sh

# Headless build process
./vtm_headless_build.sh
```

### Unity Editor Menu Commands
```
# Scene creation (in Unity Editor)
VTM/Create Complete World        # Automated scene builder
VTM/Create Scene Setup           # Basic scene structure

# Build operations
File → Build Settings → Build And Run
```

### System Verification
```bash
# VCC settings check
cat ~/.local/share/VRChatCreatorCompanion/settings.json
python3 -m json.tool ~/.local/share/VRChatCreatorCompanion/settings.json

# System resources
free -h                          # Memory usage
df -h ~/projects                 # Disk space
```

### Git Operations
```bash
# Standard git workflow
git status
git add .
git commit -m "message"
git push

# Project-specific branches
git checkout main               # Main development branch
```

## Performance Testing Commands
```bash
# Unity performance validation
Unity -batchmode -quit -projectPath . -executeMethod VirtualTokyoMatching.Editor.VTMSceneSetupTool.CreateSceneSetup -logFile /tmp/unity_test.log

# Multi-client testing (requires multiple Unity instances)
Unity.exe -projectPath . -username player1 &
Unity.exe -projectPath . -username player2 &
```

## Development Workflow
1. **Start Development**: `vpm check project .` → Unity Hub launch
2. **Add Dependencies**: `vpm add package <ID> -p .` → `vpm resolve project .`
3. **Scene Creation**: Unity Editor → VTM/Create Complete World
4. **Testing**: ClientSim in Unity Editor for VRChat simulation
5. **Build**: File → Build Settings (PC: Windows, Quest: Android)

## Troubleshooting Commands
```bash
# VPM issues
vpm install templates           # Update project templates
vpm resolve project .           # Fix dependency issues

# Unity issues
rm -rf Library/                 # Force project regeneration (if needed)
Unity -createProject .          # Recreate Unity project structure
```