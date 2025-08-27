# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Virtual Tokyo Matching is a Unity-based VRChat world implementing personality-based matchmaking through a 112-question assessment. Built with Unity 2022.3 LTS + VRChat Creator Companion (VCC) + SDK3 Worlds + UdonSharp on Ubuntu 22.04 LTS.

## Current Project Status - CRITICAL

**❌ COMPILATION BLOCKED**: The project has 84+ compilation errors that must be fixed before any Unity operations:

1. **CS0592 Errors**: `[Header]` attributes placed on classes instead of fields
2. **CS1109 Errors**: Extension methods defined in nested classes instead of top-level
3. **CS0246 Errors**: Missing UnityEngine.TestRunner references in test assemblies

**Priority**: Fix compilation errors before scene creation, building, or testing.

## Essential Development Commands

### Immediate Error Resolution
```bash
# Check compilation status
~/Unity/Hub/Editor/2022.3.22f1/Editor/Unity -quit -batchmode -nographics -logFile ./compile_check.log -projectPath .

# Count errors by type  
grep "CS0592" compile_check.log | wc -l  # Header attribute errors
grep "CS1109" compile_check.log | wc -l  # Extension method errors
grep "CS0246" compile_check.log | wc -l  # Test framework errors
```

### VPM Project Management
```bash
# Verify project structure and dependencies
vpm check project .
vpm list packages -p .
vpm resolve project .

# Essential VRChat packages (already configured)
vpm add package com.vrchat.worlds -p .
vpm add package com.vrchat.udonsharp -p .  
vpm add package com.vrchat.clientsim -p .
```

### Unity Operations (After Compilation Fixed)
```bash
# Unity Editor launch
~/Unity/Hub/Editor/2022.3.22f1/Editor/Unity -projectPath ~/projects/VirtualTokyoMatching

# Headless Unity operations
Unity -quit -batchmode -nographics -logFile ./unity_operations.log -projectPath .

# Automated world creation (when compilation passes)
Unity -batchmode -quit -projectPath . -executeMethod VirtualTokyoMatching.Editor.VTMSceneBuilder.CreateCompleteWorld -logFile ./world_creation.log
```

### Project Setup Scripts
```bash
# Complete project setup (located in sh/ directory)
sh/setup_unity_project.sh           # Initial VCC/VPM setup
sh/vtm_headless_build.sh            # Automated world creation
sh/test_vrchat_setup.sh             # VRChat SDK validation
sh/configure_vrchat_build_settings.sh # VRChat-specific settings
```

## Core Architecture

**Event-driven UdonSharp system** with 9 main components:

- **PlayerDataManager**: Persistent storage for 112 questions + 30D vectors (private data only)
- **DiagnosisController**: Assessment UI with pause/resume, auto-save on each answer
- **VectorBuilder**: Incremental 30D vector updates, provisional vectors during assessment
- **PublicProfilePublisher**: Privacy-controlled 30D→6D compression for public matching
- **CompatibilityCalculator**: Distributed cosine similarity computation, top 3 selection
- **RecommenderUI**: Match display with provisional badges, progress indicators
- **SessionRoomManager**: 1-on-1 private rooms, 15-minute timers, teleportation
- **ValuesSummaryGenerator**: Auto-generated personality descriptions (no manual profiles)
- **PerfGuard**: Frame budget management, distributed processing controller

**Communication Pattern**: Components communicate exclusively via UdonSharp events, no direct references.

## UdonSharp and VRChat Constraints

### Critical Code Requirements
```csharp
// All scripts MUST inherit from UdonSharpBehaviour
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class ExampleScript : UdonSharpBehaviour

// Required imports for all UdonSharp scripts
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
```

### Compilation Error Patterns to Avoid
```csharp
// ❌ CS0592: Header on class (WRONG)
[Header("Configuration")]
public class MyClass : UdonSharpBehaviour

// ✅ Header on field (CORRECT)
public class MyClass : UdonSharpBehaviour 
{
    [Header("Configuration")]
    public GameObject target;
}

// ❌ CS1109: Nested extension methods (WRONG)
public class TestClass
{
    public static class Extensions { /* methods */ }
}

// ✅ Top-level extension methods (CORRECT)
namespace VirtualTokyoMatching.Tests
{
    public static class Extensions { /* methods */ }
}
```

### VRChat SDK Limitations
- **No System.Collections Generic**: Use arrays and manual iteration
- **No async/await**: Use frame-distributed processing with PerfGuard
- **No LINQ**: Manual filtering and aggregation required
- **Sync Variable Limit**: ~40 variables max (currently using 9)

## Data Architecture

### Private PlayerData (Never Synchronized)
- **Questions**: `diag_q_001` through `diag_q_112` (int: 0-5, 0=unanswered)
- **Vectors**: `vv_0` through `vv_29` (float: -1.0 to +1.0, full personality vector)  
- **Flags**: Privacy settings, provisional flags, activity timestamps

### Public Sync Data (Privacy-Controlled)
- **Compressed Vectors**: `red_0` through `red_5` (6D condensed via projection matrix)
- **Generated Content**: Auto-generated personality tags and headlines only
- **Progress Indicators**: Completion percentage, provisional flags for incomplete data

**Critical Privacy Rule**: Raw answers and 30D vectors never leave user's device.

## Performance Targets

- **PC**: ≥72 FPS, <200MB world size, <5s full recalculation
- **Quest**: ≥60 FPS, <100MB world size, <10s full recalculation  
- **Frame Budget**: K operations per frame limit with PerfGuard throttling
- **Network**: Event-driven sync updates only, no polling or timers

## Directory Structure

```
Assets/VirtualTokyoMatching/
├── Scripts/
│   ├── Core/          # PlayerDataManager, system foundations
│   ├── Assessment/    # DiagnosisController, questionnaire UI
│   ├── Vector/        # VectorBuilder, mathematical operations
│   ├── Matching/      # CompatibilityCalculator, algorithms  
│   ├── UI/            # RecommenderUI, main interface controllers
│   ├── Session/       # SessionRoomManager, private rooms
│   ├── Sync/          # PublicProfilePublisher, networking
│   ├── Performance/   # PerfGuard, optimization systems
│   ├── Analysis/      # ValuesSummaryGenerator, text generation
│   ├── Safety/        # Privacy controls, safety systems
│   ├── Editor/        # Unity Editor tools, automated builders
│   └── Testing/       # Validation, debugging tools
├── ScriptableObjects/ # External configuration data
├── Resources/         # Template JSON files for configuration
└── Tests/             # Automated testing (currently broken - CS errors)
```

## Configuration System

All runtime data externalized as ScriptableObjects:
- **QuestionDatabase**: 112 questions, 5 choices each, axis mappings, weights
- **VectorConfiguration**: 112→30D weight matrix W, 30D→6D projection matrix P
- **SummaryTemplates**: Auto-generation templates for personality descriptions
- **PerformanceSettings**: Frame budgets, FPS targets, optimization parameters

## Development Priority Order

1. **Fix Compilation Errors** (blocking everything else)
   - Move `[Header]` attributes from classes to fields
   - Extract extension methods to top-level namespaces
   - Create proper .asmdef files with TestRunner references

2. **Unity Scene Integration** (after compilation passes)
   - Run automated scene builder: `VTMSceneBuilder.CreateCompleteWorld`
   - Configure VRC_SceneDescriptor and spawn points
   - Wire UdonSharp components in Inspector

3. **VRChat SDK Integration**
   - Import VRChat SDK3 Worlds package
   - Configure build settings for PC and Quest targets  
   - Test with ClientSim multi-user simulation

4. **Performance Validation**
   - Multi-client testing with Unity Editor instances
   - Frame rate and memory profiling
   - VRChat world upload and testing

## Prohibited Patterns

- **External APIs**: No web services, databases, or network calls outside VRChat
- **Manual Profiles**: All personality content must be auto-generated from assessments
- **Raw Data Exposure**: Never sync 30D vectors or individual question answers
- **Free-form Text**: No user text input to prevent inappropriate content
- **Blocking Operations**: All heavy computation must be frame-distributed