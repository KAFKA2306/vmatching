# Virtual Tokyo Matching - VRChat World Implementation

## Project Overview

Virtual Tokyo Matching is a comprehensive personality-based matching system for VRChat worlds. It provides progressive matching capabilities through a 112-question assessment, allowing users to receive recommendations even with incomplete questionnaires through provisional vector calculations.

## Core Features

### 🧠 Progressive Personality Assessment
- **112-question assessment** with 5-choice responses
- **Resume functionality** - continue from where you left off
- **Incremental saving** - every answer is immediately saved to PlayerData
- **Provisional vectors** - get recommendations even with partial completion

### 🔄 Real-time Matching System
- **Event-driven compatibility calculation** using cosine similarity
- **30D → 6D dimensionality reduction** for privacy and performance
- **Distributed processing** with frame-rate protection (PerfGuard)
- **Progressive recommendations** - more accurate as you answer more questions

### 🛡️ Privacy & Safety First
- **Raw responses never shared** - only 6D reduced vectors are public
- **Explicit consent required** for public sharing
- **Emergency hide functionality** - instantly go private
- **Session-only avatars** - no permanent image storage

### 💬 1-on-1 Interaction System
- **Private session rooms** with automatic teleportation
- **15-minute timed sessions** with warnings and auto-end
- **Invitation system** with accept/decline functionality
- **Feedback collection** for improved future matching

## Technical Architecture

### Core Components

#### Data Management
- **PlayerDataManager**: Handles persistent storage, retry logic, and data validation
- **VectorBuilder**: Performs incremental vector updates and normalization
- **PublicProfilePublisher**: Manages 30D→6D reduction and sync distribution

#### Matching Engine
- **CompatibilityCalculator**: Computes cosine similarity with distributed processing
- **ValuesSummaryGenerator**: Creates personality summaries from vector data
- **PerfGuard**: Maintains target frame rates through calculation throttling

#### User Interface
- **MainUIController**: Orchestrates 5-button lobby navigation
- **DiagnosisController**: Manages assessment UI and progression
- **RecommenderUI**: Displays match cards with provisional indicators
- **SafetyController**: Handles privacy controls and safety features

#### Session Management
- **SessionRoomManager**: Manages private rooms, invitations, and timing

### Performance Specifications

#### Target Performance
- **PC**: ≥72 FPS, <200MB world size, <5s full recalculation
- **Quest**: ≥60 FPS, <100MB world size, <10s full recalculation
- **Distributed Processing**: Frame-limited calculations with adaptive throttling

#### Scalability
- **Max Users**: Designed for 20-40 concurrent users
- **Sync Efficiency**: Minimal sync variables (9 total per user)
- **Memory Management**: Efficient vector caching and cleanup

## Data Flow Architecture

```
Assessment → 30D Vector → 6D Public → Compatibility → Recommendations → 1-on-1 Sessions
     ↓           ↓            ↓            ↓              ↓             ↓
PlayerData  Incremental   Sync Vars   Distributed    UI Cards    Private Rooms
(Private)   Updates       (Public)    Processing     (Public)    (Ephemeral)
```

## Security & Privacy Model

### Private Data (PlayerData)
- Individual question responses (112 × int)
- Full 30D personality vectors (30 × float)
- User flags and preferences
- Session feedback (local only)

### Public Data (Sync Variables)
- 6D reduced vectors (6 × float)
- Auto-generated personality tags (string)
- Completion percentage and provisional flag
- Display name only

### Safety Features
- **Instant privacy toggle** - hide profile immediately
- **Emergency controls** - exit sessions and hide data
- **Data reset option** - complete removal of all saved data
- **Consent-based sharing** - explicit opt-in required

## Implementation Status

✅ **Complete Core Implementation**
- All 11 UdonSharp components implemented
- ScriptableObject configuration system ready
- Event-driven architecture with proper error handling
- Performance monitoring and throttling system

❌ **Unity Project Setup Required**
- VCC project creation and UdonSharp installation
- Scene construction with UI prefabs
- 3D world environment modeling
- ScriptableObject asset configuration

## Quick Start Guide

### Prerequisites
- Unity 2022.3 LTS
- VRChat Creator Companion (VCC)
- UdonSharp package

### Setup Steps
1. **Create VCC Project**: Use SDK3 Worlds template
2. **Install UdonSharp**: Add community package
3. **Import Scripts**: Copy all C# files to Unity project
4. **Configure Assets**: Set up ScriptableObject configurations
5. **Build Scene**: Create UI prefabs and world environment
6. **Test & Deploy**: Local testing → Friends+ → Public

See [ProjectSetup.md](./ProjectSetup.md) for detailed instructions.

## Configuration

The system uses ScriptableObjects for all data configuration:

### Question Database (QuestionDatabase.cs)
```csharp
// Example question configuration
Question 1: {
    text: "新しい環境に適応するのは得意ですか？",
    choices: ["全く違う", "やや違う", "どちらでもない", "やや当てはまる", "非常に当てはまる"],
    targetAxis: 0,
    weights: [-2, -1, 0, 1, 2]
}
```

### Vector Configuration (VectorConfiguration.cs)
- **W Matrix**: 112×30 transformation weights
- **P Matrix**: 30×6 projection for public data
- **Axis Labels**: Human-readable names for all dimensions

### Summary Templates (SummaryTemplates.cs)
- **Personality descriptions** for each axis polarity
- **Tag generation rules** based on vector values
- **Headline templates** with confidence thresholds

### Performance Settings (PerformanceSettings.cs)
- **Frame budgets** (PC: 10, Quest: 5 calculations/frame)
- **Quality settings** (texture resolution, optimization flags)
- **Monitoring thresholds** and adaptive parameters

## Development Workflow

### Branch Strategy
- **main**: Stable release versions
- **develop**: Integration branch
- **feature/***: Individual feature development

### Testing Requirements
- **Local Multi-client**: Unity editor with multiple instances
- **Friends+ Testing**: 1 week validation period
- **Performance Validation**: FPS and memory targets
- **Data Integrity**: PlayerData persistence testing

### Quality Gates
- No external API dependencies
- VRChat SDK3 compliance
- Performance targets met (PC 72fps, Quest 60fps)
- Privacy requirements satisfied (no raw data exposure)

## API Reference

### Core Events
```csharp
// PlayerDataManager Events
OnPlayerDataLoaded()    // Fired when data is loaded
OnPlayerDataSaved()     // Fired when data is saved
OnPlayerDataReset()     // Fired when data is reset

// Assessment Events  
OnQuestionAnswered()    // Fired when question is answered
OnAssessmentComplete()  // Fired when all questions completed

// Vector Events
OnVectorUpdated()       // Fired on incremental vector update
OnVectorFinalized()     // Fired when vector is finalized

// Profile Events
OnProfilePublished()    // Fired when profile goes public
OnProfileHidden()       // Fired when profile goes private

// Session Events
OnSessionStarted()      // Fired when 1-on-1 session begins
OnSessionEnded()        // Fired when session ends
```

### Data Access Methods
```csharp
// PlayerDataManager
int GetQuestionResponse(int questionIndex)
float[] GetVector30D()
bool IsAssessmentComplete()
float GetCompletionPercentage()

// PublicProfilePublisher  
float[] GetCurrent6DVector()
string[] GetCurrentTags()
bool IsPublic()
bool IsProvisional()

// CompatibilityCalculator
CompatibilityResult[] GetTopRecommendations()
bool IsCalculating()
float GetCalculationProgress()
```

## Troubleshooting

### Common Issues

**Q: Assessment doesn't resume properly**
A: Check PlayerDataManager event wiring and ensure OnPlayerDataLoaded is called

**Q: Recommendations not updating**  
A: Verify CompatibilityCalculator has PerfGuard reference and events are properly wired

**Q: Sync data not appearing for other users**
A: Confirm PublicProfilePublisher sync mode is Manual and RequestSerialization is called

**Q: Performance issues on Quest**
A: Adjust PerformanceSettings calculation budgets and texture resolutions

### Debug Tools

**Performance Monitor**: Enable `showFrameTimeUI` in PerformanceSettings for real-time metrics

**Calculation Debug**: Use `ForceRecalculation()` and `RecalculatePerformanceMetrics()` for testing

**Data Validation**: Call `ValidateDatabase()` on QuestionDatabase to check configuration

## Contributing

This is a complete implementation ready for deployment. Key areas for customization:

1. **Question Content**: Modify QuestionDatabase with domain-specific questions
2. **UI Styling**: Customize prefabs and materials for world theme
3. **3D Environment**: Design lobby, assessment area, and private rooms
4. **Personality Model**: Adjust W matrix and axis definitions
5. **Summary Templates**: Localize or customize personality descriptions

## License & Usage

This implementation is designed for VRChat world deployment. Ensure compliance with:
- VRChat Terms of Service
- Community Guidelines for social/dating content  
- Local privacy laws regarding personality data collection

## Support & Documentation

- **Architecture Documentation**: [docs/architecture.md](./docs/archtecture.md)
- **Requirements Specification**: [docs/requirement.md](./docs/requirement.md)
- **Design Document**: [docs/design.md](./docs/design.md)
- **Setup Guide**: [ProjectSetup.md](./ProjectSetup.md)
- **Development Environment**: [docs/devenv.md](./docs/devenv.md)

---

**Status**: ✅ Core implementation complete, ready for Unity project integration and world deployment.

**Next Steps**: Follow ProjectSetup.md to create the Unity project and configure for VRChat deployment.