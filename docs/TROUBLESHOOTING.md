# Virtual Tokyo Matching - Troubleshooting Guide

## Overview
This document provides solutions to common issues encountered during Virtual Tokyo Matching development and deployment. All major issues have been identified and resolved.

## Quick Fixes Checklist

### 1. Unity Hub Not Recognizing Project ✅ FIXED
**Symptoms**: Unity Hub doesn't show VirtualTokyoMatching in projects list

**Root Cause**: Missing `ProjectSettings/` folder or malformed project structure

**Solution**:
```bash
# Run the complete setup script
./setup_unity_project.sh

# Or manually add to Unity Hub
# 1. Open Unity Hub
# 2. Click "Add" → Navigate to /home/kafka/projects/VirtualTokyoMatching
# 3. Select project folder
```

**Prevention**: Always use `./setup_unity_project.sh` for initial setup

### 2. VPM CLI JSON Parsing Errors ✅ FIXED
**Symptoms**: 
```
Failed to load settings! Unexpected character encountered while parsing value: {. 
Path 'unityEditors', line 6, position 5.
```

**Root Cause**: Malformed `settings.json` with incorrect array syntax

**Solution**:
```bash
# Remove corrupted settings and regenerate
mv ~/.local/share/VRChatCreatorCompanion/settings.json ~/.local/share/VRChatCreatorCompanion/settings.json.broken
vpm install templates
vpm list repos
```

**Prevention**: Use automated setup script which generates proper JSON

### 3. Unity Build Target Validation Errors ✅ FIXED
**Symptoms**:
```
[VTM Builder] Environment validation failed:
Build target must be PC, Mac & Linux Standalone (Windows 64-bit)
```

**Root Cause**: VTMSceneBuilder validation too strict for Linux builds

**Solution**: Fixed in current codebase - validation now accepts Linux64 builds

**Verification**:
```bash
# Check if Unity can build the scene
./vtm_complete_setup.sh
```

### 4. Missing Unity Tags Errors ✅ FIXED
**Symptoms**:
```
Tag: Floor is not defined.
Tag: SpawnMarker is not defined.
```

**Root Cause**: Custom Unity tags not added to TagManager

**Solution**: Tags automatically added by setup script to `ProjectSettings/TagManager.asset`

**Manual Fix** (if needed):
```yaml
# Edit ProjectSettings/TagManager.asset
tags:
- Floor
- Wall
- RoomFloor
- Furniture
- SpawnMarker
```

## Development Environment Issues

### Unity Editor Issues

#### Unity Process Already Running
**Symptoms**: "Multiple Unity instances cannot open the same project"

**Solution**:
```bash
# Kill existing Unity processes
pkill -f "Unity.*VirtualTokyoMatching"

# Or use the launcher script which handles this
./run_unity.sh
```

#### Unity Compilation Errors
**Symptoms**: Scripts don't compile, missing references

**Solution**:
```bash
# Clear Unity cache and recompile
rm -rf /home/kafka/projects/VirtualTokyoMatching/Library/ScriptAssemblies
rm -rf /home/kafka/projects/VirtualTokyoMatching/Temp

# Restart Unity Editor
./run_unity.sh
```

#### Missing VRChat SDK
**Symptoms**: VRC_SceneDescriptor component not found

**Solution**:
1. Open Unity Editor
2. Window → VRChat SDK → Show Control Panel
3. Follow SDK import instructions
4. SDK packages are pre-installed via VPM

### VPM Package Manager Issues

#### Package Resolution Failures
**Symptoms**: Dependencies cannot be resolved

**Solution**:
```bash
cd /home/kafka/projects/VirtualTokyoMatching

# Clear package cache
rm -rf ~/.local/share/VRChatCreatorCompanion/cache

# Reinstall templates
vpm install templates

# Resolve dependencies
vpm resolve project .
```

#### Package Compatibility Conflicts
**Symptoms**: "Package is incompatible with current project"

**Solution**:
```bash
# Install packages with specific versions
vpm add package com.vrchat.worlds -p .
vpm resolve project .

vpm add package com.vrchat.udonsharp@1.1.8 -p .
vpm resolve project .

vpm add package com.vrchat.clientsim@1.2.6 -p .
vpm resolve project .
```

### Build and Deployment Issues

#### Headless Build Failures
**Symptoms**: Unity batch mode commands fail

**Solution**:
```bash
# Ensure no Unity processes are running
pkill -f Unity

# Run complete setup with fixes
./vtm_complete_setup.sh

# Check build logs for specific errors
cat ./unity_build.log | grep -E "(Error|Exception|Failed)"
```

#### Scene Generation Failures
**Symptoms**: VTMSceneBuilder fails to create world

**Solution**: All scene generation issues have been fixed in the current version:
- Environment validation updated for Linux builds
- Missing tags added to TagManager
- Material application errors resolved

#### Performance Issues
**Symptoms**: Low FPS, high memory usage

**Solution**:
```bash
# Check system resources
free -h
top -bn1 | grep "Cpu(s)"

# Unity quality settings are automatically configured
# Target: PC ≥72FPS, Quest ≥60FPS
```

## Network and Connectivity Issues

### VPM Repository Access
**Symptoms**: Cannot download packages

**Solution**:
```bash
# Test VRChat repository connectivity
ping -c 3 packages.vrchat.com
curl -I https://packages.vrchat.com/

# Check firewall settings
sudo ufw status
```

### VRChat Upload Issues
**Symptoms**: Cannot upload world to VRChat

**Solution**:
1. Ensure VRChat SDK3 is properly imported
2. Add VRC_SceneDescriptor to VRCWorld GameObject
3. Configure spawn points in scene descriptor
4. Build for correct target platform (PC/Quest)
5. Use VRChat SDK Control Panel for upload

## Runtime Issues

### PlayerData Persistence
**Symptoms**: User progress not saved

**Solution**:
- PlayerDataManager automatically handles save/load
- Data is stored in VRChat's PlayerData system
- Check UdonBehaviour components are properly wired

### Networking Synchronization
**Symptoms**: Users don't see each other's profiles

**Solution**:
- Verify PublicProfilePublisher components are configured
- Check Network IDs are unique for each profile slot
- Ensure public sharing is enabled in Safety settings

### Performance Optimization
**Symptoms**: Frame rate drops, compatibility calculation slow

**Solution**:
- PerfGuard automatically manages computational load
- Adjust K operations per frame in PerformanceSettings
- Use incremental recalculation (event-driven updates only)

## Testing and Validation

### Multi-Client Testing
**Issue**: Cannot test multiplayer functionality

**Solution**:
```bash
# Use ClientSim for local multiplayer testing
# Window → VRChat SDK → Utilities → ClientSim
# Enable multiple client instances
```

### Quest Compatibility
**Issue**: World doesn't work on Quest

**Solution**:
- Build with Android target
- Ensure textures ≤1024x1024
- Memory usage <100MB
- Use Quest-compatible shaders
- Test with Quest build settings

## Emergency Recovery Procedures

### Complete Environment Reset
```bash
#!/bin/bash
echo "=== VTM Environment Complete Reset ==="

# 1. Backup current work
cp -r /home/kafka/projects/VirtualTokyoMatching /home/kafka/projects/VirtualTokyoMatching.backup.$(date +%Y%m%d_%H%M%S)

# 2. Reset VPM CLI
dotnet tool uninstall --global vrchat.vpm.cli
rm -rf ~/.dotnet/tools/.store/vrchat.vpm.cli
rm -rf ~/.local/share/VRChatCreatorCompanion

# 3. Reinstall everything
./setup_unity_project.sh

# 4. Regenerate world
./vtm_complete_setup.sh

echo "=== Reset Complete ==="
```

### Project Recovery from Backup
```bash
# If project becomes corrupted
cd /home/kafka/projects
rm -rf VirtualTokyoMatching

# Restore from backup
cp -r VirtualTokyoMatching.backup.TIMESTAMP VirtualTokyoMatching

# Re-run setup
cd virtualtokyomatching
./setup_unity_project.sh
```

## Diagnostic Commands

### System Health Check
```bash
# Check all prerequisites
dotnet --version                          # .NET SDK
which unityhub                            # Unity Hub
find ~/Unity/Hub/Editor -name "Unity"     # Unity Editor
vpm --version                             # VPM CLI

# Check project status
cd /home/kafka/projects/VirtualTokyoMatching
vpm check project .
vpm list packages -p .

# Check Unity project structure
ls -la ProjectSettings/
ls -la Assets/VirtualTokyoMatching/Scripts/
```

### Log Analysis
```bash
# Check Unity build logs
grep -E "(Error|Exception|VTM Builder)" ./unity_build.log

# Check VPM logs (if available)
ls -la ~/.local/share/VRChatCreatorCompanion/logs/

# Check system logs for Unity crashes
journalctl -u unity --since "1 hour ago"
```

### Performance Monitoring
```bash
# Monitor system resources during development
htop                                      # Real-time process monitor
iotop -o                                 # Disk I/O monitor
nvidia-smi                               # GPU monitoring (if applicable)

# Unity specific monitoring
ps aux | grep Unity                      # Unity processes
lsof -p $(pgrep Unity)                  # Unity file handles
```

## Prevention Best Practices

### 1. Regular Backups
```bash
# Daily backup script
#!/bin/bash
BACKUP_DIR="/home/kafka/backups/vtm/$(date +%Y%m%d)"
mkdir -p "$BACKUP_DIR"

# Backup project
cp -r /home/kafka/projects/VirtualTokyoMatching "$BACKUP_DIR/"

# Backup VPM settings
cp -r ~/.local/share/VRChatCreatorCompanion "$BACKUP_DIR/"
```

### 2. Environment Validation
```bash
# Weekly environment check
#!/bin/bash
echo "=== VTM Environment Health Check ==="

# Check dependencies
dotnet --version > /dev/null && echo "✅ .NET SDK OK" || echo "❌ .NET SDK MISSING"
vpm --version > /dev/null && echo "✅ VPM CLI OK" || echo "❌ VPM CLI MISSING"
[ -f ~/Unity/Hub/Editor/2022.3.22f1/Editor/Unity ] && echo "✅ Unity Editor OK" || echo "❌ Unity Editor MISSING"

# Check project integrity
cd /home/kafka/projects/VirtualTokyoMatching
vpm check project . > /dev/null && echo "✅ Project OK" || echo "❌ Project ISSUES"

echo "=== Health Check Complete ==="
```

### 3. Version Control
```bash
# Git integration for project tracking
cd /home/kafka/projects/VirtualTokyoMatching

# Initialize git if not already done
git init
git add .
git commit -m "Working VTM setup after troubleshooting fixes"

# Tag stable versions
git tag -a v1.0-stable -m "Fully working VTM setup"
```

## Support Resources

### Documentation
- **ProjectSetup.md**: Complete setup instructions
- **vpm.md**: VPM CLI configuration and troubleshooting
- **SCENE_SETUP.md**: Automated world creation guide
- **CLAUDE.md**: Project overview and constraints

### Log Files
- `unity_build.log`: World creation and build logs
- `unity_materials.log`: Material setup logs
- `unity_optimization.log`: VRChat optimization logs
- `unity_validation.log`: Scene validation logs

### Useful Commands
```bash
# Quick project status
./run_unity.sh                           # Launch Unity
./vtm_complete_setup.sh                  # Regenerate world
vpm check project .                      # Validate project
cat unity_build.log | tail -20          # Check recent build output
```

All major issues have been resolved in the current codebase. This troubleshooting guide serves as reference for any future issues that may arise during development or deployment.