# Virtual Tokyo Matching

[![Deploy web to GitHub Pages](https://github.com/KAFKA2306/vmatching/actions/workflows/pages.yml/badge.svg)](https://github.com/KAFKA2306/vmatching/actions/workflows/pages.yml)

**相性を探したい。でも、112問の回答そのものを他人へ公開したくはない。**

Virtual Tokyo Matching は、VRChat内で性格診断を進めながら、**必要以上の個人情報を公開せずに「話してみたい相手」へ到達する**ことを目指すmatching systemです。

repositoryには診断、30D→6D vector、推薦、公開profile、安全UI、1on1 session、performance guard、scene setup / validator、testsの実装が存在します。一方、公開VRChat worldでのruntime成功や現在のUnity Editor versionは、このREADMEだけからは完了扱いしません。

## Vision

「相性の良い人を見つける」ために、利用者へプロフィールの過剰公開や長い入力完了を要求しない体験を作ります。

目指す状態は次です。

- 診断途中でも暫定推薦を見られる
- 詳細回答は本人だけが保持する
- 他人へ公開する情報は縮約・同意済みのものに限定する
- 推薦から1on1会話へ自然に移動できる
- 不安を感じた瞬間に非公開・退出・resetできる

## Design philosophy

- **Privacy before matching accuracy.** 112問回答と30軸をそのまま他者同期しない。
- **Progress before completion.** 全質問完了を待たず、回答途中の暫定vectorで推薦を更新する。
- **Consent before exposure.** 公開profileは明示同意時だけ同期し、OFF/退出時にclearできる設計にする。
- **Recommendation is assistance, not truth.** cosine similarityを人間関係の断定として扱わない。
- **Safety is always reachable.** emergency hide / exit / resetをmatching flowの外側へ追いやらない。
- **Performance is a product constraint.** 推薦計算をevent-drivenにし、VRのframe budgetを守る。
- **Repository evidence before completion claims.** scriptが存在することと、実worldで検証済みであることを分ける。

## Why / 差別化

一般的なmatching体験では、詳しいprofileを公開するほど推薦材料が増えます。しかしVRChatでは、**会う前から詳細な回答を他人へ見せること自体が摩擦**になり得ます。

Virtual Tokyo Matching は、「どれだけ詳しく公開するか」ではなく、**どこまで情報を減らしても会話のきっかけを作れるか**を設計の中心に置きます。

112問、30D、6D、UdonSharpは差別化そのものではありません。価値は、詳細回答を保持したまま公開情報を縮約し、途中参加・推薦・安全導線を一つのworld体験へ接続することです。

## Current implementation

`Assets/VirtualTokyoMatching/` には少なくとも次の実装があります。

```text
Scripts/
  Assessment/DiagnosisController.cs
  Core/PlayerDataManager.cs
  Vector/VectorBuilder.cs
  Matching/CompatibilityCalculator.cs
  Sync/PublicProfilePublisher.cs
  UI/RecommenderUI.cs
  Session/SessionRoomManager.cs
  Safety/SafetyController.cs
  Performance/PerfGuard.cs
  Analysis/ValuesSummaryGenerator.cs
  Editor/VTMSceneBuilder.cs
  Editor/VTMVRChatValidator.cs
  Testing/VTMSystemValidator.cs
```

ScriptableObject / template:

- `QuestionDatabase`
- `VectorConfiguration`
- `SummaryTemplates`
- `PerformanceSettings`

Testsも `Assets/VirtualTokyoMatching/Tests/` に配置されています。

## Experience flow

```text
112-question assessment
  → personal 30D vector
  → reduced 6D public representation
  → compatibility calculation
  → top recommendations
  → mutual consent
  → 1on1 session room

at any point:
  hide / unpublish / reset / exit
```

### Data boundary

| data | visibility intent |
|---|---|
| raw 112 answers | self only |
| 30D internal vector | self only |
| reduced 6D vector | publish only with consent |
| tags / short summary | publish only with consent |
| progress / provisional state | minimal public state when enabled |

Public profileをOFFにした場合、詳細回答を別経路で同期する設計にはしません。

## Matching

`CompatibilityCalculator.cs` がreduced vector間のsimilarityを計算し、`RecommenderUI.cs` が候補表示を担当します。

推薦値は「人間として相性が良い」という真理値ではなく、**会話候補を絞るためのranking signal**として扱います。

## 1on1 session

`SessionRoomManager.cs` はmatching後のsession transitionを担います。

設計上のflow:

```text
recommendation
  → mutual consent
  → room allocation
  → teleport
  → timed conversation
  → return
```

満室・競合・cancelを通常stateとして扱い、相手との接触を強制しないことを前提にします。

## Safety / privacy

`SafetyController.cs` をmatching品質と同格のcore componentとして扱います。

- emergency hide
- profile unpublish
- reset
- exit
- exposure minimization

matching率を上げるためにprivacy boundaryを緩めることはDesign goalではありません。

## Performance

`PerfGuard.cs` とevent-driven updateにより、毎frame全員分を再計算する設計を避けます。

PC / Questの具体的FPS・build-size目標はperformance targetとして扱い、**実機計測前に達成済みとは書きません**。

## Environment truth boundary

現在のrepositoryで確認できるpackage例:

- `com.vrchat.base`: `3.5.2`
- `com.vrchat.worlds`: `3.5.2`
- `com.vrchat.udonsharp`: `1.1.8`

`ProjectSettings/ProjectVersion.txt` は現在 `UnknownUnityVersion` です。そのためREADMEでは特定Unity versionをverified requirementとして固定しません。

Unity / VRChat SDK compatibilityは、実際にEditorでprojectを開く時点のofficial compatibilityとproject stateを再確認してください。

## Repository map

```text
Assets/VirtualTokyoMatching/
  Scripts/             runtime / editor / validator
  ScriptableObjects/   typed configuration
  Resources/           configuration templates
  Tests/               contract / behavior tests
Packages/              Unity package manifest
ProjectSettings/       project settings
docs/                  design / operation notes
web/                   supporting web assets
```

## What is verified vs not verified

### Repository evidence exists

- diagnosis / vector / matching source
- safety / session / performance source
- editor setup / validator source
- ScriptableObject definitions
- tests
- VRChat SDK / UdonSharp package declarations

### Not claimed by README without fresh runtime evidence

- current public VRChat world deployment
- successful VRChat Build & Test
- Quest / PC runtime FPS targets achieved
- current Unity Editor version
- real-user matching effectiveness

## Next verification boundary

```text
repository tests
  → Unity project resolution
  → Editor compilation
  → scene build / validator
  → ClientSim or multi-client behavior
  → VRChat Build & Test
  → controlled Friends+ observation
```

各段階を通る前に次の段階を「完了」と呼びません。

## Done

このprojectの成功は、質問数やmatching algorithmの複雑さでは測りません。

**利用者が詳細な回答を公開しすぎず、途中でも相手候補を見つけ、安全に会話へ進むか撤退するかを自分で選べること**をDoneの中心に置きます。