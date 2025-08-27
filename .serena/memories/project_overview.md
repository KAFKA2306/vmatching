# Virtual Tokyo Matching - Project Overview

## Project Purpose
Virtual Tokyo Matching is a Unity-based VRChat world that implements a sophisticated personality-based matchmaking system. The project creates a VRChat world where users complete a 112-question personality assessment and are matched with compatible users for 1-on-1 private conversations.

## Key Features
- **Progressive Matching**: Users can see recommendations even with incomplete questionnaires
- **112-Question Personality Assessment**: 5-choice questions with pause/resume functionality  
- **30D → 6D Vector Compression**: Privacy-preserving personality vector reduction
- **Real-time Compatibility**: Cosine similarity matching with top 3 recommendations
- **Private Session Rooms**: 1-on-1 conversation spaces with 15-minute timers
- **Auto-generated Profiles**: No manual profiles - everything generated from assessment data
- **Privacy Controls**: Comprehensive safety and visibility controls

## Tech Stack
- **Platform**: Unity 2022.3 LTS + VRChat SDK3 Worlds + UdonSharp
- **Language**: C# (UdonSharp variant for VRChat compatibility)
- **Package Management**: VRChat Creator Companion (VCC) + VPM CLI
- **Development Environment**: Ubuntu 22.04 LTS (Linux)
- **Target Platforms**: PC (Windows) and Quest (Android)

## Architecture Highlights
- **Event-driven Design**: All components communicate via UdonSharp events
- **Distributed Processing**: Frame-limited calculations with PerfGuard system
- **Data Privacy**: Raw answers stored locally, only 6D condensed vectors shared publicly
- **Performance Optimized**: Target 72FPS PC / 60FPS Quest with size limits <200MB PC / <100MB Quest

## Current Project Status (Phase 4 - Compilation Issues)

### ✅ **Core Implementation Complete**
All 9 main UdonSharp components are implemented and feature-complete:
- **PlayerDataManager**: Persistent data with retry mechanisms
- **DiagnosisController**: 112-question UI with pause/resume
- **VectorBuilder**: 30D provisional vector + incremental updates + normalization
- **PublicProfilePublisher**: 30D→6D compression + sync distribution
- **CompatibilityCalculator**: Cosine similarity + distributed computation + top 3 selection
- **PerfGuard**: FPS monitoring + computational budget management
- **RecommenderUI**: Recommendation cards + detail UI + provisional badges
- **SessionRoomManager**: 1-on-1 room management + invite system + 15min timers
- **ValuesSummaryGenerator**: Auto-generated personality summaries

### ✅ **Project Structure Complete**
- **Assets folder**: Fully organized with Scripts, ScriptableObjects, Tests, Resources
- **Configuration system**: ScriptableObject-based external configuration
- **Test suite**: Comprehensive testing framework for all components
- **Build scripts**: Automated setup and build processes

### ❌ **Current Blocking Issues (84+ compilation errors)**
The project has significant compilation issues that need resolution:

1. **C# Syntax Errors**: 
   - `CS0592`: Header attributes on wrong declaration types (QuestionDatabase.cs)
   - Requires moving `[Header]` attributes from classes to fields

2. **Extension Method Issues**:
   - `CS1109`: Extension methods in nested classes (ProgressiveMatchingTests, MultiUserSynchronizationTests)
   - Requires moving `EnumerableExtensions` to top-level namespace

3. **Test Framework Issues**:
   - `CS0246`: Missing UnityTestAttribute/UnityTest references
   - Requires proper .asmdef files with UnityEngine.TestRunner references

### 🔄 **Immediate Next Steps**
1. Fix compilation errors (Header attributes, extension methods, test framework)
2. Create proper Assembly Definition files for test frameworks
3. Validate Unity project compilation in headless mode
4. Complete scene construction and UI prefab integration

### 📂 **Project Locations**
- **Main Project**: `/home/kafka/projects/VirtualTokyoMatching` (Unity project)
- **Working Directory**: `/home/kafka/projects/virtualtokyomatching` (development)
- **Core Scripts**: `Assets/VirtualTokyoMatching/Scripts/` (9 main components)
- **Configuration**: `Assets/VirtualTokyoMatching/ScriptableObjects/` + `Resources/`

## Performance Constraints
- **PC Target**: 72+ FPS, <200MB world size
- **Quest Target**: 60+ FPS, <100MB world size  
- **Network**: Minimal sync data (6D vectors only), event-driven updates
- **Memory**: Distributed processing, frame-limited calculations

## Privacy and Safety Design
- **No External APIs**: Self-contained VRChat world only
- **Data Privacy**: Raw 112 answers never leave user's local storage
- **Public Data**: Only 6D compressed vectors + auto-generated summaries
- **Safety Controls**: Emergency hide, data reset, opt-out functionality
- **VRChat Integration**: Blocking/muting system compatibility

## Development Workflow Status
- ✅ **Scripts**: All core functionality implemented
- ❌ **Compilation**: 84+ errors requiring fixes (CS0592, CS1109, CS0246)  
- ❌ **Unity Project**: Compilation must pass before scene construction
- ❌ **Scene Integration**: UI prefabs and world geometry pending
- ❌ **Testing**: Multi-client testing requires working build
- ❌ **VRChat Upload**: Full pipeline testing needed before release

The project is in the **debugging and compilation phase** - all core functionality exists but requires error resolution before proceeding to Unity scene construction and final integration.