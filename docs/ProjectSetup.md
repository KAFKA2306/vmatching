# Virtual Tokyo Matching - Unity Project Setup Guide

## Project Structure Overview

The Virtual Tokyo Matching system is now implemented with all core UdonSharp components. Here's how to set up the Unity project for VRChat deployment.

## Prerequisites

- Ubuntu 22.04 LTS (as specified in requirements)
- Unity Hub installed
- Unity 2022 LTS (latest) 
- VRChat Creator Companion (VCC)

## Step 1: VCC Project Creation

1. Open VRChat Creator Companion
2. Create new project → Select "3D World"
3. Choose Unity 2022.3 LTS version
4. Name: "VirtualTokyoMatching"
5. Wait for SDK3 Worlds to be installed automatically

## Step 2: UdonSharp Installation

1. In VCC, go to your project
2. Click "Manage Project"
3. Add Package: UdonSharp (community package)
4. Wait for compilation to complete

## Step 3: Project File Structure

Create the following directory structure in your Unity project:

```
Assets/
├── VirtualTokyoMatching/
│   ├── Scripts/
│   │   ├── Analysis/
│   │   │   └── ValuesSummaryGenerator.cs
│   │   ├── Assessment/
│   │   │   └── DiagnosisController.cs
│   │   ├── Core/
│   │   │   └── PlayerDataManager.cs
│   │   ├── Matching/
│   │   │   └── CompatibilityCalculator.cs
│   │   ├── Performance/
│   │   │   └── PerfGuard.cs
│   │   ├── Safety/
│   │   │   └── SafetyController.cs
│   │   ├── Session/
│   │   │   └── SessionRoomManager.cs
│   │   ├── Sync/
│   │   │   └── PublicProfilePublisher.cs
│   │   ├── UI/
│   │   │   ├── MainUIController.cs
│   │   │   └── RecommenderUI.cs
│   │   └── Vector/
│   │       └── VectorBuilder.cs
│   ├── ScriptableObjects/
│   │   ├── QuestionDatabase.cs
│   │   ├── VectorConfiguration.cs
│   │   ├── SummaryTemplates.cs
│   │   └── PerformanceSettings.cs
│   ├── Prefabs/
│   │   ├── UI/
│   │   ├── Rooms/
│   │   └── Systems/
│   ├── Materials/
│   ├── Textures/
│   └── Audio/
```

## Step 4: Scene Setup

Create the main scene with these components:

### World Structure
```
VTM_World
├── Lobby
│   ├── SpawnPoint
│   ├── UI_Canvas_Main
│   └── LobbyEnvironment
├── AssessmentArea
│   ├── UI_Canvas_Assessment
│   └── AssessmentEnvironment  
├── PrivateRooms (3 rooms)
│   ├── Room_01
│   │   ├── SpawnPoint_1A
│   │   ├── SpawnPoint_1B
│   │   └── UI_Canvas_Session
│   ├── Room_02
│   └── Room_03
└── SystemManagers
    ├── PlayerDataManager
    ├── MainUIController
    ├── DiagnosisController
    ├── VectorBuilder
    ├── PublicProfilePublisher
    ├── CompatibilityCalculator
    ├── RecommenderUI
    ├── SessionRoomManager
    ├── ValuesSummaryGenerator
    ├── PerfGuard
    └── SafetyController
```

## Step 5: ScriptableObject Configuration

Create ScriptableObject assets for configuration:

1. Right-click in Project → Create → VTM → Question Database
   - Configure 112 questions with 5-choice answers
   - Set target axes (0-29) and choice weights

2. Create → VTM → Vector Configuration
   - Configure W matrix (112×30 transformation)
   - Configure P matrix (30×6 projection)
   - Set axis names and labels

3. Create → VTM → Summary Templates
   - Configure personality descriptions for each axis
   - Set positive/negative/neutral trait descriptions
   - Configure headline templates

4. Create → VTM → Performance Settings
   - Set target FPS (PC: 72, Quest: 60)
   - Configure calculation budgets
   - Set texture resolution limits

## Step 6: UI Prefab Creation

### Main Lobby UI Prefab
```
UI_Canvas_Main
├── LobbyPanel
│   ├── WelcomeText
│   ├── ProgressSlider
│   ├── Button_StartAssessment
│   ├── Button_ContinueAssessment
│   ├── Button_PublicSharing
│   ├── Button_ViewRecommendations
│   ├── Button_GoToRoom
│   └── StatusText
├── LoadingScreen
└── BackToLobbyButton
```

### Assessment UI Prefab
```
UI_Canvas_Assessment
├── AssessmentPanel
│   ├── QuestionText
│   ├── ChoiceButtons[5]
│   ├── NavigationButtons
│   ├── ProgressBar
│   └── QuestionCounter
└── StatusDisplay
```

### Recommendations UI Prefab
```
UI_Canvas_Recommendations
├── RecommendationsPanel
│   ├── RecommendationCard[3]
│   │   ├── PlayerName
│   │   ├── CompatibilityRing
│   │   ├── HeadlineText
│   │   ├── TagContainer
│   │   ├── ProvisionalBadge
│   │   ├── ProgressSlider
│   │   ├── ViewDetailsButton
│   │   └── InviteButton
│   └── LoadingIndicator
├── DetailPanel
│   ├── DetailInfo
│   ├── RadarChart
│   ├── TagList
│   └── ActionButtons
└── StatusText
```

## Step 7: Component Wiring

For each UdonSharp script, configure the Inspector references:

### PlayerDataManager
- Set event targets for data loaded/saved/reset events

### DiagnosisController
- Link QuestionDatabase ScriptableObject
- Connect UI elements (questionText, choiceButtons, etc.)
- Set PlayerDataManager and VectorBuilder references

### VectorBuilder
- Link VectorConfiguration ScriptableObject
- Set PlayerDataManager reference
- Configure event targets

### PublicProfilePublisher
- Set dependencies (PlayerDataManager, VectorBuilder, etc.)
- Configure sync mode to Manual

### CompatibilityCalculator
- Link PerfGuard and other dependencies
- Set calculation parameters

### RecommenderUI
- Connect all UI elements
- Link CompatibilityCalculator and SessionRoomManager

### SessionRoomManager
- Configure session rooms (spawn points, UI, etc.)
- Set lobby spawn point
- Configure sync mode to Manual

## Step 8: Performance Optimization

### Texture Settings
- PC: Max 2048×2048
- Quest: Max 1024×1024
- Enable mipmaps for distance optimization

### Lighting
- Use baked lighting for static geometry
- Minimize real-time lights
- Use light probes for dynamic objects

### Audio
- Compress audio files
- Use 3D spatial audio for immersion
- Set appropriate audio occlusion for rooms

## Step 9: Testing Workflow

### Local Testing
1. Build and Test → Windows
2. Open multiple Unity instances for multiplayer testing
3. Test all user flows:
   - Assessment start/continue/complete
   - Public sharing on/off
   - Recommendation viewing
   - 1-on-1 invitations
   - Session management

### VCC Testing
1. Build and Test → Android (for Quest)
2. Test in VRChat client
3. Verify PlayerData persistence
4. Test multiplayer sync

## Step 10: Build Configuration

### PC Build Settings
- Platform: Standalone Windows 64-bit
- Target: Windows
- Quality: High
- Max size: <200MB

### Quest Build Settings  
- Platform: Android
- Target: Quest/Quest 2
- Quality: Medium
- Max size: <100MB
- Texture compression: ASTC

## Step 11: World Upload

1. VRChat SDK → Build & Publish
2. Set world name: "Virtual Tokyo Matching"
3. Set capacity: 20-40 players (recommended)
4. Configure tags: social, dating, personality, matching
5. Upload and test in Friends+ before Public

## Configuration Templates

The system requires these data configurations:

### Example Question Entry
```
Question 1:
Text: "新しい環境に適応するのは得意ですか？"
Choices: ["全く違う", "やや違う", "どちらでもない", "やや当てはまる", "非常に当てはまる"]
Target Axis: 0 (Adaptability)
Weights: [-2, -1, 0, 1, 2]
```

### Performance Targets
- PC: ≥72 FPS, <200MB, recalculation ≤5s
- Quest: ≥60 FPS, <100MB, recalculation ≤10s

## Security Considerations

- Raw assessment responses (112 questions) remain private
- Only 6D reduced vectors + summaries are shared publicly
- Public sharing requires explicit user consent
- Emergency hide functions available at all times
- Session data is ephemeral (not persisted)

## Next Steps

1. Configure all ScriptableObject assets with real data
2. Create UI prefabs with proper styling
3. Design and model the 3D world environment
4. Test thoroughly with multiple users
5. Optimize for target performance metrics
6. Deploy to Friends+ for beta testing
7. Launch publicly after validation

This implementation provides a complete foundation for the Virtual Tokyo Matching VRChat world with progressive personality matching capabilities.