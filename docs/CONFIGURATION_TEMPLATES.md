# Virtual Tokyo Matching - Configuration Templates

## ScriptableObject Configuration Guide

This document provides sample configurations for all ScriptableObjects needed to run the Virtual Tokyo Matching system.

## 1. Performance Settings Configuration

**File**: `Assets/VirtualTokyoMatching/Resources/DefaultPerformanceSettings.asset`

### Recommended Values:
```csharp
[Header("Frame Rate Limits")]
maxCalculationsPerFrame: 10        // PC: 10, Quest: 5 (auto-adjusted)
targetFrameRatePC: 72              // Target FPS for PC
targetFrameRateQuest: 60           // Target FPS for Quest

[Header("Calculation Thresholds")]
maxFullRecalculationsPC: 5         // Max seconds for full compatibility recalc (PC)
maxFullRecalculationsQuest: 10     // Max seconds for full compatibility recalc (Quest)
incrementalUpdateInterval: 0.5f    // Seconds between incremental updates

[Header("Memory Optimization")]
maxCachedProfiles: 50              // Max profiles to keep in memory
maxRecommendationsToCalculate: 10  // Max compatibility calculations
recommendationsToDisplay: 3        // Top N matches to show

[Header("Network Optimization")]
syncDataUpdateRate: 1.0f           // Seconds between sync variable updates
maxSyncUpdatesPerFrame: 3          // Limit sync updates per frame

[Header("Quality Settings")]
maxTextureResolutionPC: 2048       // Max texture size for PC
maxTextureResolutionQuest: 1024    // Max texture size for Quest
useMipmaps: true
enableLightBaking: true

[Header("Debug Settings")]
enablePerformanceLogging: false    // Enable for debugging
showFrameTimeUI: false            // Show debug UI overlay
performanceLogInterval: 5.0f      // Seconds between performance logs
```

## 2. Vector Configuration

**File**: `Assets/VirtualTokyoMatching/Resources/VectorConfig.asset`

### 30D Axis Names (Japanese):
```csharp
axisNames[30] = {
    "外向性",      // 0: Extraversion
    "創造性",      // 1: Creativity
    "協調性",      // 2: Cooperativeness
    "論理性",      // 3: Logic
    "感情表現",    // 4: Emotional Expression
    "責任感",      // 5: Responsibility
    "冒険心",      // 6: Adventurousness
    "理想主義",    // 7: Idealism
    "社交性",      // 8: Sociability
    "楽観性",      // 9: Optimism
    "自立性",      // 10: Independence
    "献身性",      // 11: Dedication
    "革新性",      // 12: Innovation
    "持続力",      // 13: Persistence
    "表現力",      // 14: Expressiveness
    "分析力",      // 15: Analytical
    "共感性",      // 16: Empathy
    "積極性",      // 17: Assertiveness
    "多様性",      // 18: Diversity
    "計画性",      // 19: Planning
    "直感力",      // 20: Intuition
    "競争心",      // 21: Competitiveness
    "協力性",      // 22: Collaboration
    "自由性",      // 23: Freedom
    "現実性",      // 24: Realism
    "情熱",        // 25: Passion
    "慎重さ",      // 26: Caution
    "適応力",      // 27: Adaptability
    "伝統重視",    // 28: Tradition
    "進歩性"       // 29: Progressiveness
};
```

### 6D Reduced Axis Names:
```csharp
reducedAxisNames[6] = {
    "社交性",      // 0: Social (combines extraversion, sociability, expression)
    "創造性",      // 1: Creative (combines creativity, innovation, idealism)
    "協調性",      // 2: Cooperative (combines cooperation, empathy, collaboration)
    "理性",        // 3: Rational (combines logic, analysis, planning)
    "感情",        // 4: Emotional (combines emotion, passion, intuition)
    "行動性"       // 5: Behavioral (combines assertiveness, adventure, independence)
};
```

### Sample Transformation Matrices:

#### Weight Matrix (112→30D) - Sample for first 10 questions:
```csharp
// Question 0: "新しい人と話すのは得意ですか？" (Good at talking to new people?)
// Target axis 0 (Extraversion) and 8 (Sociability)
SetWeight(0, 0, 0.8f);  // Strong contribution to extraversion
SetWeight(0, 8, 0.6f);  // Moderate contribution to sociability
SetWeight(0, 17, 0.4f); // Slight contribution to assertiveness

// Question 1: "創作活動に興味がありますか？" (Interested in creative activities?)
// Target axis 1 (Creativity) and 12 (Innovation)
SetWeight(1, 1, 0.9f);  // Strong creativity
SetWeight(1, 12, 0.5f); // Moderate innovation
SetWeight(1, 14, 0.3f); // Slight expressiveness

// ... Continue for all 112 questions
```

#### Projection Matrix (30D→6D):
```csharp
// Social axis (combines multiple social traits)
SetProjectionWeight(0, 0, 0.8f);   // Extraversion → Social
SetProjectionWeight(8, 0, 0.7f);   // Sociability → Social  
SetProjectionWeight(14, 0, 0.5f);  // Expressiveness → Social
SetProjectionWeight(17, 0, 0.6f);  // Assertiveness → Social

// Creative axis
SetProjectionWeight(1, 1, 0.8f);   // Creativity → Creative
SetProjectionWeight(12, 1, 0.7f);  // Innovation → Creative
SetProjectionWeight(7, 1, 0.5f);   // Idealism → Creative

// Cooperative axis
SetProjectionWeight(2, 2, 0.8f);   // Cooperativeness → Cooperative
SetProjectionWeight(16, 2, 0.7f);  // Empathy → Cooperative
SetProjectionWeight(22, 2, 0.6f);  // Collaboration → Cooperative

// Rational axis
SetProjectionWeight(3, 3, 0.8f);   // Logic → Rational
SetProjectionWeight(15, 3, 0.7f);  // Analytical → Rational
SetProjectionWeight(19, 3, 0.6f);  // Planning → Rational

// Emotional axis
SetProjectionWeight(4, 4, 0.8f);   // Emotional Expression → Emotional
SetProjectionWeight(25, 4, 0.7f);  // Passion → Emotional
SetProjectionWeight(20, 4, 0.5f);  // Intuition → Emotional

// Behavioral axis
SetProjectionWeight(6, 5, 0.7f);   // Adventurousness → Behavioral
SetProjectionWeight(10, 5, 0.6f);  // Independence → Behavioral
SetProjectionWeight(21, 5, 0.5f);  // Competitiveness → Behavioral
```

## 3. Question Database

**File**: `Assets/VirtualTokyoMatching/Resources/QuestionDatabase.asset`

### Sample Questions (first 10 of 112):

```csharp
questions[0] = {
    text: "新しい人と話すのは得意ですか？",
    choices: [
        "全く得意ではない",
        "あまり得意ではない", 
        "どちらとも言えない",
        "やや得意",
        "とても得意"
    ],
    targetAxis: 0, // Extraversion
    weights: [-2.0f, -1.0f, 0.0f, 1.0f, 2.0f]
};

questions[1] = {
    text: "創作活動（絵、音楽、文章など）に興味がありますか？",
    choices: [
        "全く興味がない",
        "あまり興味がない",
        "どちらとも言えない", 
        "やや興味がある",
        "とても興味がある"
    ],
    targetAxis: 1, // Creativity
    weights: [-2.0f, -1.0f, 0.0f, 1.0f, 2.0f]
};

questions[2] = {
    text: "チームワークを重視しますか？",
    choices: [
        "個人で作業する方が良い",
        "どちらかと言えば個人作業",
        "どちらでも構わない",
        "どちらかと言えばチーム作業",
        "チーム作業の方が断然良い"
    ],
    targetAxis: 2, // Cooperativeness
    weights: [-2.0f, -1.0f, 0.0f, 1.0f, 2.0f]
};

questions[3] = {
    text: "物事を論理的に考える方ですか？",
    choices: [
        "感情を重視する",
        "どちらかと言えば感情重視",
        "バランスを取る",
        "どちらかと言えば論理重視",
        "論理を最重視する"
    ],
    targetAxis: 3, // Logic
    weights: [-2.0f, -1.0f, 0.0f, 1.0f, 2.0f]
};

questions[4] = {
    text: "自分の感情を表現するのは得意ですか？",
    choices: [
        "とても苦手",
        "やや苦手",
        "普通",
        "やや得意",
        "とても得意"
    ],
    targetAxis: 4, // Emotional Expression  
    weights: [-2.0f, -1.0f, 0.0f, 1.0f, 2.0f]
};

questions[5] = {
    text: "約束や責任を重視しますか？",
    choices: [
        "あまり重視しない",
        "それほど重視しない",
        "普通に重視する",
        "かなり重視する", 
        "最優先で重視する"
    ],
    targetAxis: 5, // Responsibility
    weights: [-2.0f, -1.0f, 0.0f, 1.0f, 2.0f]
};

questions[6] = {
    text: "新しいことに挑戦するのは好きですか？",
    choices: [
        "安定した環境が良い",
        "どちらかと言えば安定重視",
        "どちらでも良い",
        "どちらかと言えば挑戦好き",
        "常に新しい挑戦を求める"
    ],
    targetAxis: 6, // Adventurousness
    weights: [-2.0f, -1.0f, 0.0f, 1.0f, 2.0f]
};

questions[7] = {
    text: "理想を追求することは大切ですか？",
    choices: [
        "現実的であることが重要",
        "どちらかと言えば現実重視",
        "バランスが大切",
        "どちらかと言えば理想重視",
        "理想の追求が最も大切"
    ],
    targetAxis: 7, // Idealism
    weights: [-2.0f, -1.0f, 0.0f, 1.0f, 2.0f]
};

questions[8] = {
    text: "パーティーやイベントは好きですか？",
    choices: [
        "とても苦手",
        "あまり好きではない",
        "どちらとも言えない",
        "やや好き",
        "とても好き"
    ],
    targetAxis: 8, // Sociability
    weights: [-2.0f, -1.0f, 0.0f, 1.0f, 2.0f]
};

questions[9] = {
    text: "物事を楽観的に考えますか？",
    choices: [
        "悲観的に考えがち",
        "どちらかと言えば悲観的",
        "現実的に考える",
        "どちらかと言えば楽観的",
        "いつも楽観的"
    ],
    targetAxis: 9, // Optimism
    weights: [-2.0f, -1.0f, 0.0f, 1.0f, 2.0f]
};

// Continue for questions[10] through questions[111]...
```

## 4. Summary Templates

**File**: `Assets/VirtualTokyoMatching/Resources/SummaryTemplates.asset`

### Configuration Values:
```csharp
significanceThreshold: 0.3f        // Minimum |value| to be significant
maxTagsPerProfile: 3              // Max tags to generate
maxSentencesInSummary: 2          // Max sentences in summary
```

### Sample Axis Templates:

```csharp
axisTemplates[0] = { // Extraversion
    axisName: "外向性",
    axisIndex: 0,
    positiveDescription: "社交的で人との関わりを大切にされます",
    positiveTags: ["社交的", "外向的", "人懐っこい"],
    negativeDescription: "内省的で静かな環境を好まれます", 
    negativeTags: ["内向的", "思慮深い", "集中型"],
    neutralDescription: "状況に応じて社交的にも内省的にも振る舞われます",
    neutralTags: ["バランス型", "適応型", "状況対応"]
};

axisTemplates[1] = { // Creativity
    axisName: "創造性",
    axisIndex: 1,
    positiveDescription: "創造的で新しいアイデアを生み出すことを得意とされます",
    positiveTags: ["創造的", "アイデア豊富", "革新的"],
    negativeDescription: "実用性を重視し確実な方法を選ばれます",
    negativeTags: ["実用的", "現実的", "堅実"],
    neutralDescription: "創造性と実用性のバランスを取られます",
    neutralTags: ["バランス型", "実用創造", "柔軟"]
};

axisTemplates[2] = { // Cooperativeness  
    axisName: "協調性",
    axisIndex: 2,
    positiveDescription: "チームワークを重視し他者との協力を大切にされます",
    positiveTags: ["協調的", "チームワーク", "協力的"],
    negativeDescription: "独立性を重視し個人での作業を好まれます",
    negativeTags: ["独立志向", "個人主義", "自立型"],
    neutralDescription: "状況に応じて協力と独立を使い分けられます",
    neutralTags: ["柔軟", "状況対応", "バランス型"]
};

// Continue for all 30 axes...
```

### Headline Templates:
```csharp
headlineTemplates = [
    {
        template: "創造的で社交的な方です",
        minConfidence: 0.8f
    },
    {
        template: "協調性を重視する方です", 
        minConfidence: 0.6f
    },
    {
        template: "バランス感覚に優れた方です",
        minConfidence: 0.4f
    },
    {
        template: "新しいつながりを求めています",
        minConfidence: 0.0f // Default fallback
    }
];
```

## 5. Unity Inspector Configuration Checklist

### MainUIController Dependencies:
```
✅ PlayerDataManager
✅ DiagnosisController  
✅ RecommenderUI
✅ SafetyController
✅ SessionRoomManager
✅ All UI GameObject references
✅ Button color settings
```

### DiagnosisController Dependencies:
```
✅ PlayerDataManager
✅ VectorBuilder
✅ QuestionDatabase (from Resources)
✅ All UI references (buttons, texts, sliders)
✅ Event target arrays
```

### VectorBuilder Dependencies:
```
✅ PlayerDataManager
✅ VectorConfiguration (from Resources)
✅ QuestionDatabase (from Resources)  
✅ Event target arrays
```

### PublicProfilePublisher Dependencies:
```
✅ PlayerDataManager
✅ VectorBuilder
✅ VectorConfiguration (from Resources)
✅ ValuesSummaryGenerator
✅ Event target arrays
✅ Sync variable setup (automatic)
```

### CompatibilityCalculator Dependencies:
```
✅ PerfGuard
✅ PlayerDataManager
✅ Event target arrays
✅ Settings (max recommendations, intervals)
```

### RecommenderUI Dependencies:
```
✅ CompatibilityCalculator
✅ SessionRoomManager
✅ ValuesSummaryGenerator
✅ All UI references (cards, detail panels)
✅ Color settings
```

### SessionRoomManager Dependencies:
```
✅ PlayerDataManager
✅ Session room array setup
✅ Lobby spawn point
✅ UI references (invitation dialogs)
✅ Event target arrays
```

### PerfGuard Dependencies:
```
✅ PerformanceSettings (from Resources)
✅ Event target arrays
```

### ValuesSummaryGenerator Dependencies:
```
✅ SummaryTemplates (from Resources)
✅ VectorConfiguration (from Resources)
✅ Localization settings
```

### SafetyController Dependencies:
```
✅ PlayerDataManager
✅ PublicProfilePublisher
✅ SessionRoomManager  
✅ All UI references (toggles, panels, buttons)
```

## 6. Testing Configuration

### Test Data Setup:
1. **Create test question database** with simple questions
2. **Set low significance threshold** (0.1) for easier testing
3. **Enable performance logging** during development
4. **Use small calculation budgets** (K=2-3) for testing
5. **Create test user profiles** with known vector values

### Debug Settings:
```csharp
PerformanceSettings:
- enablePerformanceLogging: true
- showFrameTimeUI: true
- maxCalculationsPerFrame: 3 (for testing)

All Components:
- Add Debug.Log statements for state changes
- Use descriptive log messages
- Include component names in logs
```

This configuration will give you a fully functional Virtual Tokyo Matching system ready for deployment in VRChat!