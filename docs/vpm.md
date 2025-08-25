# Virtual Tokyo Matching - Development Tasks Guide

## プロジェクト概要

**VirtualTokyoMatching**は112問性格診断ベースのリアルタイムマッチングシステムを持つVRChatワールドです。Ubuntu 22.04環境でのVPM CLI を使用した完全な開発ガイドです。

### 主要機能
- **112問性格診断**: 中断・再開対応の進捗保存システム
- **30D→6D変換**: プライバシー保護のための次元縮約
- **リアルタイムマッチング**: コサイン類似度による上位3名推薦
- **1on1個室システム**: 双方同意による15分セッション
- **自動要約生成**: 手入力不要の価値観プロフィール生成

## Phase 1: 開発環境構築

### 1.4 プロジェクト作成
```bash
# VRChatワールドプロジェクト作成
vpm new VirtualTokyoMatching World -p ~/projects
cd ~/projects/VirtualTokyoMatching

# 必須パッケージ追加
vpm add package com.vrchat.worlds -p .
vpm add package com.vrchat.udonsharp -p .
vpm add package com.vrchat.clientsim -p .
vpm resolve project .

# Unity Hub でプロジェクト開く
/usr/bin/unityhub -- --projectPath ~/projects/VirtualTokyoMatching
```

## Phase 2: プロジェクト構造構築

### 2.1 Assets フォルダ構成
```
Assets/VirtualTokyoMatching/
├── Scripts/
│   ├── Core/                    # PlayerDataManager, VTMController
│   ├── Assessment/              # DiagnosisController, 112問診断
│   ├── Vector/                  # VectorBuilder, 30D→6D変換
│   ├── Matching/                # CompatibilityCalculator, 推薦エンジン
│   ├── UI/                      # MainUIController, RecommenderUI
│   ├── Safety/                  # SafetyController, プライバシー保護
│   ├── Session/                 # SessionRoomManager, 1on1個室
│   ├── Sync/                    # PublicProfilePublisher, 同期処理
│   └── Performance/             # PerfGuard, ValuesSummaryGenerator
├── ScriptableObjects/
│   ├── QuestionDatabase.asset   # 112問・5択・軸マッピング
│   ├── VectorConfiguration.asset # 変換行列設定
│   ├── SummaryTemplates.asset   # 性格要約テンプレート
│   └── PerformanceSettings.asset # 最適化設定
├── Scenes/
│   └── VirtualTokyoMatching.unity
├── Prefabs/
│   ├── UI/                      # UIプレファブ
│   ├── SessionRooms/            # 個室プレファブ
│   └── Systems/                 # システムプレファブ
├── Resources/                   # ランタイムロード用設定
├── Materials/                   # UI・環境マテリアル
├── Textures/                    # 最適化済みテクスチャ
└── Audio/                       # 音響効果
```

## Phase 3: コアシステム実装

### 3.1 PlayerDataManager（データ永続化）
```csharp
// タスク: PlayerData永続化システム
- VRChat PlayerData APIを使用した進捗保存
- キー管理（diag_q_001～112, vv_0～29, flags等）
- 中断・再開機能
- データリセット・復旧機能
- イベント駆動アーキテクチャ（onDataLoaded等）
```

### 3.2 DiagnosisController（112問診断）
```csharp
// タスク: 性格診断システム
- 112問・5択形式のUI実装
- 中断・再開対応（未回答=0で管理）
- 回答ごとの即座保存
- 進捗表示・ナビゲーション
- スキップ・戻る機能
```

### 3.3 VectorBuilder（ベクトル変換）
```csharp
// タスク: 次元変換システム
- 112問回答→30軸ベクトル変換（重み行列W）
- 暫定ベクトルの逐次更新
- -1.0～+1.0正規化
- 30軸→6軸縮約（プライバシー保護）
- イベント通知（onVectorUpdated等）
```

### 3.4 CompatibilityCalculator（マッチング）
```csharp
// タスク: 相性計算エンジン
- コサイン類似度計算
- 分散処理・フレーム制限
- 上位3名推薦システム
- 増分再計算（入退室・回答更新時）
- パフォーマンス最適化
```

### 3.5 PublicProfilePublisher（同期処理）
```csharp
// タスク: ネットワーク同期
- 6軸縮約データ同期
- 公開ON/OFF制御
- UdonSynced変数管理
- Late-joiner対応
- 同期負荷最適化
```

### 3.6 RecommenderUI（推薦表示）
```csharp
// タスク: 推薦UIシステム
- 推薦カード表示（相性%・タグ・進捗）
- 詳細パネル（要約・レーダーチャート）
- 招待ボタン・1on1導線
- 暫定バッジ表示
- リアルタイム更新
```

### 3.7 SessionRoomManager（個室管理）
```csharp
// タスク: 1on1セッション
- 双方同意システム
- 個室割当・テレポート
- 15分タイマー・終了ベル
- フィードバック収集
- 占有管理・解放処理
```

### 3.8 ValuesSummaryGenerator（要約生成）
```csharp
// タスク: 自動要約システム
- 30軸から性格要約生成
- タグ自動生成
- ヘッドライン作成
- テンプレートベース処理
- 多言語対応準備
```

### 3.9 PerfGuard（性能管理）
```csharp
// タスク: パフォーマンス最適化
- フレーム予算管理（K値制御）
- 計算キュー管理
- FPS監視・調整
- Quest最適化
- リソース使用量制限
```

### 3.10 SafetyController（安全機能）
```csharp
// タスク: プライバシー・安全
- 公開制御UI
- 緊急非表示機能
- データリセット
- ミュート/ブロック連携
- 行動規範表示
```

## Phase 4: ScriptableObject設定

### 4.1 QuestionDatabase作成
```csharp
// タスク: 質問データベース
- 112問の質問テキスト
- 5択選択肢設定
- 軸マッピング（0-29）
- 重み値設定（-2.0～+2.0）
- カテゴリ分類
```

### 4.2 VectorConfiguration作成
```csharp
// タスク: ベクトル設定
- 30軸名称（日本語）
- 112→30D変換行列W
- 30D→6D投影行列P
- 軸説明・ラベル
- 変換パラメータ
```

### 4.3 SummaryTemplates作成
```csharp
// タスク: 要約テンプレート
- 30軸別記述テンプレート
- ポジティブ/ネガティブ記述
- タグ生成ルール
- ヘッドライン形式
- 信頼度閾値設定
```

### 4.4 PerformanceSettings作成
```csharp
// タスク: 性能設定
- フレーム予算（PC: 10, Quest: 5）
- 目標FPS（PC: 72, Quest: 60）
- 計算間隔・キュー制限
- メモリ・帯域制限
- デバッグ設定
```

## Phase 5: シーン構築

### 5.1 Environment（環境）
```csharp
// タスク: 空間設計
- Lobby（メイン交流エリア）
- SessionRooms（3つの個室）
- SpawnPoints（8-10箇所）
- Lighting（ベイク照明）
- Audio（環境音）
```

### 5.2 UI System
```csharp
// タスク: UIシステム
- MainLobbyCanvas（Screen Space）
- AssessmentCanvas（World Space）
- RecommenderCanvas（World Space）
- SafetyCanvas（World Space）
- EventSystem設定
```

### 5.3 Systems Integration
```csharp
// タスク: システム統合
- VTMController（メインオーケストレーター）
- NetworkedProfiles（同期プロファイル）
- VRCSceneDescriptor設定
- 依存関係配線
- イベントチェーン構築
```

## Phase 6: テスト・最適化

### 6.1 機能テスト
```bash
# タスク: 機能検証
- 診断システム（中断・再開・完了）
- マッチング（推薦・詳細・招待）
- 個室システム（割当・タイマー・帰還）
- 同期処理（入退室・公開切替）
- 安全機能（非表示・リセット）
```

### 6.2 パフォーマンステスト
```bash
# タスク: 性能検証
- フレームレート測定（PC/Quest）
- メモリ使用量監視
- ネットワーク帯域確認
- 計算負荷分析
- 同時接続テスト
```

### 6.3 Quest最適化
```bash
# タスク: Quest対応
- テクスチャ圧縮（1024px上限）
- ドローコール削減
- ライト数制限
- シェーダー最適化
- 容量削減（<100MB）
```

## Phase 7: 公開準備

### 7.1 Build & Test
```bash
# タスク: ビルド検証
- PC版ビルド・テスト
- Quest版ビルド・テスト
- ClientSim検証
- マルチプレイヤーテスト
- エラーハンドリング確認
```

### 7.2 段階的公開
```bash
# タスク: 公開フロー
- Private：開発者テスト
- Friends+：ベータテスト（1週間）
- Public：正式公開
- フィードバック収集・改善
```

## Phase 8: 運用・保守

### 8.1 監視・分析
```bash
# タスク: 運用監視
- 利用状況分析
- パフォーマンス監視
- エラーログ分析
- ユーザーフィードバック対応
```

### 8.2 継続改善
```bash
# タスク: アップデート
- 質問データベース調整
- アルゴリズム改善
- UI/UX改良
- パフォーマンス最適化
- 新機能追加検討
```

## 開発コマンドクイックリファレンス

### VPM操作
```bash
# 基本操作
vpm --version
vpm check project .
vpm list packages -p .
vpm resolve project .

# パッケージ管理
vpm add package <ID> -p .
vpm remove package <ID> -p .

# 診断
vpm check hub
vpm check unity
vpm list repos
```

### Unity操作
```bash
# プロジェクト起動
/usr/bin/unityhub -- --projectPath ~/projects/VirtualTokyoMatching

# ビルド検証
# Unity Editor: File → Build Settings → Build And Run
```

### デバッグ・トラブルシューティング
```bash
# 設定確認
cat ~/.local/share/VRChatCreatorCompanion/settings.json
python3 -m json.tool ~/.local/share/VRChatCreatorCompanion/settings.json

# Unity環境確認
~/Unity/Hub/Editor/2022.3.22f1/Editor/Unity --version

# システムリソース確認
free -h
df -h ~/projects
```

## 品質基準

### パフォーマンス目標
- **PC**: ≥72FPS, <200MB, 全再計算≤5秒
- **Quest**: ≥60FPS, <100MB, 全再計算≤10秒
- **同期**: 最小限の同期変数、効率的な帯域使用

### 機能要件
- 中断・再開の完全対応
- 暫定マッチングの正常動作
- プライバシー保護の徹底
- 1on1導線の安定性
- エラー耐性・復旧機能

### 安全性要件
- 生データの非公開
- 公開OFF時の完全非表示
- 緊急停止機能
- 行動規範の明示
- モデレーション連携

***

**VirtualTokyoMatching**は、Ubuntu 22.04 + VPM CLI環境で完全に開発可能な、112問性格診断ベースの次世代VRChatマッチングワールドです。この包括的なタスクガイドに従って、段階的に実装を進めることで、プロダクション品質のVRChatワールドを構築できます。
