# Virtual Tokyo Matching - Development Commands

## Current Development Status
**Project Phase**: Compilation Error Resolution (84+ errors)  
**Priority**: Fix CS0592, CS1109, CS0246 errors before scene construction

## Immediate Debug and Fix Commands

### Compilation Error Diagnosis
```bash
# Check current compilation status
Unity -quit -batchmode -nographics -logFile ./compile_check.log -projectPath .
cat compile_check.log | grep "CS[0-9]"

# Count errors by type
grep "CS0592" compile_check.log | wc -l  # Header attribute errors
grep "CS1109" compile_check.log | wc -l  # Extension method errors  
grep "CS0246" compile_check.log | wc -l  # Test framework errors
```

### Quick Error Resolution Scripts
```bash
# Run comprehensive project setup (includes error fixes)
./vtm_complete_setup.sh

# Run headless build with error fixes
./vtm_headless_build.sh

# Test VRChat-specific setup
./test_vrchat_setup.sh
```

### Manual Unity Project Operations
```bash
# Unity Editor launch (if needed for Inspector-based fixes)
/usr/bin/unityhub -- --projectPath ~/projects/VirtualTokyoMatching

# Direct Unity Editor (bypass Hub)
~/Unity/Hub/Editor/2022.3.22f1/Editor/Unity -projectPath ~/projects/VirtualTokyoMatching

# Headless Unity operations
Unity -quit -batchmode -nographics -logFile ./unity_compile_test.log -projectPath .
```

## VPM and Dependency Management

### Project Validation
```bash
# Verify VPM project structure
vpm check project .

# List and validate packages
vpm list packages -p .

# Resolve dependencies after changes
vpm resolve project .
```

### Package Management
```bash
# Essential VRChat packages (already installed)
vpm add package com.vrchat.worlds -p .
vpm add package com.vrchat.udonsharp -p .
vpm add package com.vrchat.clientsim -p .

# Package status and versions
vpm --version
vpm list repos
```

## Error-Specific Fix Commands

### CS0592 (Header Attribute) Fixes
```bash
# Find problematic Header attributes
grep -n "\[Header.*\]" Assets/VirtualTokyoMatching/Scripts/**/*.cs

# Target file: Assets/VirtualTokyoMatching/ScriptableObjects/QuestionDatabase.cs:26
# Solution: Move [Header] from class to field declaration
```

### CS1109 (Extension Method) Fixes
```bash  
# Find nested extension methods
grep -r "static.*Extension" Assets/VirtualTokyoMatching/Tests/

# Target files: ProgressiveMatchingTests.cs, MultiUserSynchronizationTests.cs
# Solution: Move EnumerableExtensions to top-level namespace
```

### CS0246 (Test Framework) Fixes
```bash
# Check for missing .asmdef files
find Assets/VirtualTokyoMatching/Tests/ -name "*.asmdef"

# Create test assembly definitions
# Solution: Add VirtualTokyoMatching.Tests.asmdef with UnityEngine.TestRunner reference
```

## Scene Creation and Integration (After Compilation Fixed)

### Automated Scene Setup
```bash
# Unity Editor scene creation tools
Unity -batchmode -quit -projectPath . -executeMethod VirtualTokyoMatching.Editor.VTMSceneSetupTool.CreateSceneSetup -logFile /tmp/unity_scene_setup.log

# Complete world creation
Unity -batchmode -quit -projectPath . -executeMethod VirtualTokyoMatching.Editor.VTMSceneBuilder.CreateCompleteWorld -logFile /tmp/unity_world_build.log
```

### Build Validation
```bash
# PC build validation
Unity -buildTarget StandaloneWindows64 -projectPath . -logFile ./build_pc.log

# Quest/Android build validation  
Unity -buildTarget Android -projectPath . -logFile ./build_quest.log
```

## Performance and Testing Commands

### Multi-Client Testing (Post-Compilation)
```bash
# Launch multiple Unity instances for sync testing
Unity.exe -projectPath . -username player1 &
Unity.exe -projectPath . -username player2 &

# ClientSim testing in Unity Editor
# (Manual operation: Unity → VRChat SDK → ClientSim)
```

### Performance Validation
```bash
# Full system validation
Unity -quit -batchmode -nographics -logFile ./unity_final_test.log -projectPath "/home/kafka/projects/VirtualTokyoMatching" -executeMethod VirtualTokyoMatching.Editor.VTMSceneBuilder.CreateCompleteWorld

# Check performance logs
grep -i "fps\|memory\|performance" ./unity_final_test.log
```

## Git Operations and Version Control

### Standard Development Workflow
```bash
# Check current status (shows docs/tasks.md and docs/vrchat.md modified)
git status

# Commit current fixes
git add .
git commit -m "Fix compilation errors: CS0592, CS1109, CS0246"

# Push changes
git push origin main
```

### Branching for Major Fixes
```bash
# Create compilation fix branch
git checkout -b fix/compilation-errors
git commit -m "Resolve 84+ compilation errors for Unity build"
git push origin fix/compilation-errors
```

## System Diagnostics

### Environment Validation
```bash
# Unity Hub and Editor paths
ls -la ~/Unity/Hub/Editor/2022.3.22f1/Editor/Unity
which unityhub

# VCC settings and directories
cat ~/.local/share/VRChatCreatorCompanion/settings.json
ls -la ~/.local/share/VRChatCreatorCompanion/

# System resources
free -h        # Memory usage
df -h ~/projects  # Disk space
```

### Log Analysis
```bash
# View recent Unity logs
find /tmp -name "*unity*.log" -mtime -1 -exec tail -20 {} \;

# Filter for specific error types
grep "error\|Error\|ERROR" ./compile_check.log
grep "warning\|Warning\|WARNING" ./compile_check.log
```

## Current Workflow Priority

1. **Fix Compilation (Immediate)**:
   ```bash
   # Run automated fixes
   ./vtm_complete_setup.sh
   
   # Validate compilation
   Unity -quit -batchmode -nographics -logFile ./compile_validation.log -projectPath .
   ```

2. **Verify Error Resolution**:
   ```bash
   # Check error count reduction
   grep "CS[0-9]" ./compile_validation.log | wc -l
   ```

3. **Proceed to Scene Integration** (After 0 errors):
   ```bash
   # Create complete world
   Unity -batchmode -quit -projectPath . -executeMethod VirtualTokyoMatching.Editor.VTMSceneBuilder.CreateCompleteWorld
   ```

4. **Final Testing and Build**:
   ```bash
   # Multi-platform build validation
   ./vtm_headless_build.sh
   ```

## Troubleshooting Commands

### Common Issues and Solutions
```bash
# Clear Unity cache (if needed)
rm -rf ~/projects/VirtualTokyoMatching/Library/

# Reset Unity project (extreme cases)
Unity -createProject ~/projects/VirtualTokyoMatching

# VPM dependency reset
vpm install templates
vpm resolve project .

# Check VRChat SDK integration
./test_vrchat_setup.sh
```

The **immediate priority** is compilation error resolution before proceeding with Unity scene construction and VRChat integration.