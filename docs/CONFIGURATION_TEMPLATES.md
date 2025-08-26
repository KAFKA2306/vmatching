# バーチャル東京マッチング - 設定テンプレート

## ScriptableObject 設定ガイド

このドキュメントは、バーチャル東京マッチングシステムを実行するために必要なすべてのScriptableObjectのサンプル設定を提供します。

## 1. パフォーマンス設定

**ファイル**: [`Assets/VirtualTokyoMatching/Resources/DefaultPerformanceSettings.asset`](Assets/VirtualTokyoMatching/Resources/DefaultPerformanceSettings.asset)

### 推奨値:
```csharp
[Header("フレームレート制限")]
maxCalculationsPerFrame: 10        // PC: 10, Quest: 5 (自動調整)
targetFrameRatePC: 72              // PCの目標FPS
targetFrameRateQuest: 60           // Questの目標FPS

[Header("計算しきい値")]
maxFullRecalculationsPC: 5         // 完全な互換性再計算の最大秒数 (PC)
maxFullRecalculationsQuest: 10     // 完全な互換性再計算の最大秒数 (Quest)
incrementalUpdateInterval: 0.5f    // 増分更新の間隔 (秒)

[Header("メモリ最適化")]
maxCachedProfiles: 50              // メモリに保持するプロファイルの最大数
maxRecommendationsToCalculate: 10  // 互換性計算の最大数
recommendationsToDisplay: 3        // 表示する上位N件のマッチ

[Header("ネットワーク最適化")]
syncDataUpdateRate: 1.0f           // 同期変数更新の間隔 (秒)
maxSyncUpdatesPerFrame: 3          // フレームあたりの同期更新の制限

[Header("品質設定")]
maxTextureResolutionPC: 2048       // PCの最大テクスチャサイズ
maxTextureResolutionQuest: 1024    // Questの最大テクスチャサイズ
useMipmaps: true
enableLightBaking: true

[Header("デバッグ設定")]
enablePerformanceLogging: false    // デバッグのために有効化
showFrameTimeUI: false            // デバッグUIオーバーレイを表示
performanceLogInterval: 5.0f      // パフォーマンスログの間隔 (秒)
```

## 2. ベクトル設定

**ファイル**: [`Assets/VirtualTokyoMatching/Resources/VectorConfig.asset`](Assets/VirtualTokyoMatching/Resources/VectorConfig.asset)

### 30D軸名 (日本語):
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

### 6D削減軸名:
```csharp
reducedAxisNames[6] = {
    "社交性",      // 0: Social (外向性、社交性、表現力を結合)
    "創造性",      // 1: Creative (創造性、革新性、理想主義を結合)
    "協調性",      // 2: Cooperative (協調性、共感性、協力を結合)
    "理性",        // 3: Rational (論理性、分析力、計画性を結合)
    "感情",        // 4: Emotional (感情、情熱、直感力を結合)
    "行動性"       // 5: Behavioral (冒険心、自立性、競争心を結合)
};
```

### サンプル変換行列:

#### 重み行列 (112→30D) - 最初の10問の例:
```csharp
// 質問0: "新しい人と話すのは得意ですか？"
// ターゲット軸 0 (外向性) と 8 (社交性)
SetWeight(0, 0, 0.8f);  // 外向性への強い貢献
SetWeight(0, 8, 0.6f);  // 社交性への中程度の貢献
SetWeight(0, 17, 0.4f); // 積極性へのわずかな貢献

// 質問1: "創作活動に興味がありますか？"
// ターゲット軸 1 (創造性) と 12 (革新性)
SetWeight(1, 1, 0.9f);  // 強い創造性
SetWeight(1, 12, 0.5f); // 中程度の革新性
SetWeight(1, 14, 0.3f); // わずかな表現力

// ... 全112問について続く
```

#### 射影行列 (30D→6D):
```csharp
// 社交軸 (複数の社交的特性を結合)
SetProjectionWeight(0, 0, 0.8f);   // 外向性 → 社交
SetProjectionWeight(8, 0, 0.7f);   // 社交性 → 社交  
SetProjectionWeight(14, 0, 0.5f);  // 表現力 → 社交
SetProjectionWeight(17, 0, 0.6f);  // 積極性 → 社交

// 創造軸
SetProjectionWeight(1, 1, 0.8f);   // 創造性 → 創造
SetProjectionWeight(12, 1, 0.7f);  // 革新性 → 創造
SetProjectionWeight(7, 1, 0.5f);   // 理想主義 → 創造

// 協調軸
SetProjectionWeight(2, 2, 0.8f);   // 協調性 → 協調
SetProjectionWeight(16, 2, 0.7f);  // 共感性 → 協調
SetProjectionWeight(22, 2, 0.6f);  // 協力性 → 協調

// 理性軸
SetProjectionWeight(3, 3, 0.8f);   // 論理性 → 理性
SetProjectionWeight(15, 3, 0.7f);  // 分析力 → 理性
SetProjectionWeight(19, 3, 0.6f);  // 計画性 → 理性

// 感情軸
SetProjectionWeight(4, 4, 0.8f);   // 感情表現 → 感情
SetProjectionWeight(25, 4, 0.7f);  // 情熱 → 感情
SetProjectionWeight(20, 4, 0.5f);  // 直感力 → 感情

// 行動軸
SetProjectionWeight(6, 5, 0.7f);   // 冒険心 → 行動
SetProjectionWeight(10, 5, 0.6f);  // 自立性 → 行動
SetProjectionWeight(21, 5, 0.5f);  // 競争心 → 行動
```

## 3. 質問データベース

**ファイル**: [`Assets/VirtualTokyoMatching/Resources/QuestionDatabase.asset`](Assets/VirtualTokyoMatching/Resources/QuestionDatabase.asset)

### サンプル質問 (112問中最初の10問):

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

// questions[10]からquestions[111]まで続く...
```

## 4. 要約テンプレート

**ファイル**: [`Assets/VirtualTokyoMatching/Resources/SummaryTemplates.asset`](Assets/VirtualTokyoMatching/Resources/SummaryTemplates.asset)

### 設定値:
```csharp
significanceThreshold: 0.3f        // 有意であるための最小|値|
maxTagsPerProfile: 3              // 生成するタグの最大数
maxSentencesInSummary: 2          // 要約の最大文数
```

### サンプル軸テンプレート:

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

// 全30軸について続く...
```

### 見出しテンプレート:
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
        minConfidence: 0.0f // デフォルトのフォールバック
    }
];
```

## 5. Unityインスペクター設定チェックリスト

### MainUIControllerの依存関係:
```
✅ PlayerDataManager
✅ DiagnosisController  
✅ RecommenderUI
✅ SafetyController
✅ SessionRoomManager
✅ すべてのUI GameObject参照
✅ ボタンの色設定
```

### DiagnosisControllerの依存関係:
```
✅ PlayerDataManager
✅ VectorBuilder
✅ QuestionDatabase (Resourcesから)
✅ すべてのUI参照 (ボタン、テキスト、スライダー)
✅ イベントターゲット配列
```

### VectorBuilderの依存関係:
```
✅ PlayerDataManager
✅ VectorConfiguration (Resourcesから)
✅ QuestionDatabase (Resourcesから)  
✅ イベントターゲット配列
```

### PublicProfilePublisherの依存関係:
```
✅ PlayerDataManager
✅ VectorBuilder
✅ VectorConfiguration (Resourcesから)
✅ ValuesSummaryGenerator
✅ イベントターゲット配列
✅ 同期変数設定 (自動)
```

### CompatibilityCalculatorの依存関係:
```
✅ PerfGuard
✅ PlayerDataManager
✅ イベントターゲット配列
✅ 設定 (最大推奨数、間隔)
```

### RecommenderUIの依存関係:
```
✅ CompatibilityCalculator
✅ SessionRoomManager
✅ ValuesSummaryGenerator
✅ すべてのUI参照 (カード、詳細パネル)
✅ 色設定
```

### SessionRoomManagerの依存関係:
```
✅ PlayerDataManager
✅ セッションルーム配列設定
✅ ロビースポーンポイント
✅ UI参照 (招待ダイアログ)
✅ イベントターゲット配列
```

### PerfGuardの依存関係:
```
✅ PerformanceSettings (Resourcesから)
✅ イベントターゲット配列
```

### ValuesSummaryGeneratorの依存関係:
```
✅ SummaryTemplates (Resourcesから)
✅ VectorConfiguration (Resourcesから)
✅ ローカライズ設定
```

### SafetyControllerの依存関係:
```
✅ PlayerDataManager
✅ PublicProfilePublisher
✅ SessionRoomManager  
✅ すべてのUI参照 (トグル、パネル、ボタン)
```

## 6. テスト設定

### テストデータ設定:
1. シンプルな質問で**テスト質問データベースを作成**
2. テストを容易にするために**有意性しきい値を低く設定** (0.1)
3. 開発中は**パフォーマンスロギングを有効化**
4. テストのために**小さな計算予算を使用** (K=2-3)
5. 既知のベクトル値を持つ**テストユーザープロファイルを作成**

### デバッグ設定:
```csharp
PerformanceSettings:
- enablePerformanceLogging: true
- showFrameTimeUI: true
- maxCalculationsPerFrame: 3 (テスト用)

すべてのコンポーネント:
- 状態変更のためにDebug.Logステートメントを追加
- 説明的なログメッセージを使用
- ログにコンポーネント名を含める
```

この設定により、VRChatにデプロイする準備が整った、完全に機能するバーチャル東京マッチングシステムが提供されます！