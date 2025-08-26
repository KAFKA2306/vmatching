# Virtual Tokyo Matching - Code Style and Conventions

## Language and Framework Constraints

### UdonSharp Requirements
- **All scripts must inherit from `UdonSharpBehaviour`** - VRChat's custom Unity scripting system
- **Namespace**: All classes in `VirtualTokyoMatching` namespace
- **Sync Mode**: Each class must declare `[UdonBehaviourSyncMode()]` attribute
- **Import Requirements**: 
  ```csharp
  using UdonSharp;
  using UnityEngine;
  using VRC.SDKBase;
  using VRC.Udon;
  ```

### VRChat SDK3 Limitations
- **No System.Collections Generic**: Limited collections support in UdonSharp
- **No async/await**: Coroutines not supported, use frame-distributed processing
- **No LINQ**: Manual iteration and filtering required
- **Event-driven architecture**: Components communicate via UdonSharp events

## Naming Conventions

### Classes and Methods
- **PascalCase** for public classes, methods, properties
- **camelCase** for private fields and variables
- **SCREAMING_SNAKE_CASE** for constants
- **Descriptive names**: `PlayerDataManager` not `DataMgr`

### Fields and Variables
```csharp
// Private fields with descriptive names
private PlayerDataManager playerDataManager;
private bool isDataLoaded;
private int currentProgress;

// Public properties with Pascal case
public bool IsDataLoaded { get; private set; }
public int CurrentProgress { get; private set; }

// Constants
private const string KEY_PREFIX = "vtm_";
private const int MAX_RETRY_COUNT = 3;
```

### Event Handling
- **Event arrays**: `UdonBehaviour[] onDataLoadedTargets`
- **Event methods**: `string onDataLoadedEvent`
- **Descriptive event names**: `onQuestionAnswered`, `onVectorUpdated`, `onMatchingComplete`

## Architecture Patterns

### Event-Driven Communication
```csharp
// Event targets and method names
[Header("Events")]
public UdonBehaviour[] onDataLoadedTargets;
public string onDataLoadedEvent = "OnPlayerDataLoaded";

// Event dispatch method
private void SendEventToTargets(UdonBehaviour[] targets, string eventName)
{
    if (targets != null)
    {
        foreach (var target in targets)
        {
            if (target != null)
                target.SendCustomEvent(eventName);
        }
    }
}
```

### Performance-Conscious Design
```csharp
// Frame-limited processing
private void Update()
{
    if (PerfGuard.CanPerformWork(this))
    {
        PerformCalculation();
    }
}

// Distributed computation patterns
private void ProcessIncrementally()
{
    int processed = 0;
    while (processed < operationsPerFrame && hasWork)
    {
        // Do work
        processed++;
    }
}
```

### Data Privacy Patterns
```csharp
// Private data (never synced)
private int[] questionResponses = new int[112];  // 0-5, 0=unanswered
private float[] vector30D = new float[30];       // Full personality vector

// Public sync data (reduced/condensed only)
[UdonSynced] private float[] reduced6D = new float[6];  // Condensed for matching
[UdonSynced] private string personalityTags = "";       // Auto-generated only
```

## Documentation Standards

### Class Documentation
```csharp
/// <summary>
/// Manages persistent PlayerData storage and retrieval for user assessment data.
/// Handles incremental saves, resume functionality, and data validation.
/// </summary>
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class PlayerDataManager : UdonSharpBehaviour
```

### Method Documentation
```csharp
/// <summary>
/// Saves a single question response and triggers vector update.
/// </summary>
/// <param name="questionIndex">0-based question index (0-111)</param>
/// <param name="response">Response value (1-5, 0 for unanswered)</param>
public void SaveQuestionResponse(int questionIndex, int response)
```

## File Organization

### Directory Structure
```
Assets/VirtualTokyoMatching/
├── Scripts/
│   ├── Core/           # PlayerDataManager, core systems
│   ├── Assessment/     # DiagnosisController, question UI
│   ├── Vector/         # VectorBuilder, math operations
│   ├── Matching/       # CompatibilityCalculator, algorithms
│   ├── UI/             # All UI controllers
│   ├── Safety/         # Privacy, safety, moderation
│   ├── Session/        # Room management, teleportation
│   ├── Sync/           # Network synchronization
│   ├── Performance/    # PerfGuard, optimization
│   ├── Analysis/       # Summary generation
│   ├── Editor/         # Unity Editor tools
│   └── Testing/        # Validation, debugging
```

### ScriptableObject Patterns
```csharp
[CreateAssetMenu(fileName = "QuestionDatabase", menuName = "VTM/Question Database")]
public class QuestionDatabase : ScriptableObject
{
    [Header("Assessment Questions")]
    public Question[] questions = new Question[112];
    
    [System.Serializable]
    public class Question
    {
        [TextArea(3, 5)]
        public string text;
        public string[] choices = new string[5];
        public int targetAxis; // 0-29
        public float[] weights = new float[5];
    }
}
```

## Performance Guidelines

### Memory Management
- **Avoid frequent allocations**: Reuse arrays and objects
- **Pool objects**: Create object pools for frequently instantiated items
- **Limit sync variables**: Minimize UdonSynced data to essential 6D vectors only

### Frame Budget Management
- **Distributed processing**: Use PerfGuard for computation throttling
- **Incremental updates**: Process data in small chunks per frame
- **Event-driven recalculation**: Only recalculate when data changes

## Security and Privacy

### Data Protection
- **Never sync raw answers**: Only sync condensed 6D vectors
- **Auto-generation only**: No manual text input for profiles
- **Immediate cleanup**: Clear public data when user opts out

### VRChat Compliance
- **No external APIs**: Everything must work within VRChat world constraints
- **Content guidelines**: Age-appropriate, consent-based interactions
- **Moderation support**: Integration with VRChat's blocking/muting systems

## Prohibited Patterns
- ❌ **Manual profile creation**: All personality data must be auto-generated
- ❌ **Raw data exposure**: Never sync 30D vectors or raw answers
- ❌ **External dependencies**: No APIs, databases, or web services
- ❌ **Blocking operations**: No synchronous heavy computations
- ❌ **Free-form text input**: Prevent inappropriate content through design