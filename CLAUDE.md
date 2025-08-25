# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Virtual Tokyo Matching is a unityhub-based VRChat world for matchmaking using automated personality analysis. Built with Unity 2022 LTS + VCC, SDK3 Worlds, and UdonSharp.

## Core Architecture

The system is a self-contained VRChat world with no external APIs/databases supporting **progressive matching** - users can see recommendations even with incomplete questionnaires. Key components:

- **PlayerDataManager**: Handles persistent data for each user (questions 1-112, 30D vectors, flags)
- **DiagnosisController**: Manages the 112-question personality assessment with auto-save/resume
- **VectorBuilder**: Performs incremental vector updates on each answer, with provisional vectors for partial responses and final normalization on completion
- **PublicProfilePublisher**: Manages public visibility toggle, broadcasts 6D condensed data with progress indicators and provisional flags
- **CompatibilityCalculator**: Event-driven cosine similarity computation with queued recalculation on answer updates
- **RecommenderUI**: Displays top 3 compatibility matches with provisional badges and progress percentages
- **SessionRoomManager**: Handles 1-on-1 private room teleportation and 15-minute timers
- **ValuesSummaryGenerator**: Creates personality summaries from vectors (no manual profiles)
- **PerfGuard**: Frame-limited distributed processing with incremental recalculation triggers

## Data Design

### PlayerData (Private, per-user)
- `diag_q_001` through `diag_q_112`: int (0-5, 0 = unanswered)
- `vv_0` through `vv_29`: float (-1.0 to +1.0) - provisional updates during answering, normalized on completion
- `flags`: int (bitfield: public ON=1, provisional public allowed=2, etc.)
- `act_last_active`: int (activity timestamp)

### Public Sync Data (when user opts in, including provisional)
- `red_0` through `red_5`: float (6D condensed vectors via fixed projection matrix P)
- `tags`: string (auto-generated personality tags)
- `headline`: string (auto-generated summary text)
- `display_name`: string (no free-form profiles allowed)
- `partial_flag`: bool (indicates provisional/incomplete data)
- `progress`: int (0-112) or `progress_pct`: float (0-1) for completion percentage

## Key Constraints

- **Performance**: PC ≥72FPS, Quest ≥60FPS, sizes PC<200MB/Quest<100MB
- **Privacy**: Raw answers and 30D vectors never exposed to others, only 6D condensed data
- **No External Dependencies**: Everything must work within VRChat world constraints
- **No Manual Profiles**: All personality data is auto-generated from questionnaire responses
- **Session-only Images**: Avatar snapshots shown only during active sessions, not persisted

## Progressive Matching System

**Key Innovation**: Users see recommendations and can start conversations even with incomplete questionnaires.

- **Incremental Vector Updates**: Each answer immediately updates provisional 30D vectors (zero-filled for unanswered questions)
- **Event-Driven Recalculation**: Answer confirmations trigger queued compatibility recalculation for all affected users
- **Provisional UI Indicators**: Recommendation cards show "provisional" badges and progress percentages
- **Graduated Confidence**: Earlier answers have higher weight; provisional vectors have naturally lower norms in cosine similarity

## Performance Architecture

- **Frame Budget**: K operations per frame limit with distributed processing
- **Incremental Updates**: Only recalculate compatibility when vectors change (answer events, user join/leave, visibility toggle)
- **6D Compression**: Public matching uses compressed vectors (30D→6D via fixed projection matrix P)
- **Memory Constraints**: PC<200MB, Quest<100MB total world size

## 実装状況（Implementation Status）

プロジェクトは設計完了・実装段階にあります：

- ✅ **Core Scripts**: 9/9 complete - All main components implemented
- ❌ **Unity Project**: Not initialized (requires VCC setup)  
- ❌ **3D Scenes**: UI prefabs and world geometry pending
- ✅ **Architecture**: Event-driven, distributed processing ready

### 実装済みコンポーネント
- **PlayerDataManager**: PlayerData永続化・復元・リトライ機構
- **DiagnosisController**: 112問UI・中断再開・自動前進  
- **VectorBuilder**: 30D暫定ベクトル・増分更新・正規化
- **PublicProfilePublisher**: 30D→6D縮約・同期配布・公開制御
- **CompatibilityCalculator**: コサイン類似度・分散計算・上位3件選出
- **PerfGuard**: FPS監視・計算予算管理・適応的スロットリング
- **RecommenderUI**: 推薦カード・詳細UI・暫定バッジ・招待機能
- **SessionRoomManager**: 1on1個室管理・招待システム・15分タイマー
- **ValuesSummaryGenerator**: 30Dから自動要約生成

## Development Workflow

開発は Unity 2022 LTS + VCC + SDK3 Worlds + UdonSharp 構成で進めます：

### 必須セットアップ手順
1. **VCC Setup**: VRChat Creator Companion で SDK3 Worlds プロジェクト作成
2. **UdonSharp**: UdonSharp パッケージ導入
3. **Scripts Integration**: Assets/VirtualTokyoMatching/Scripts/ を Unity Project に配置
4. **Scene Creation**: UI Prefab・3D空間・テレポートポイント配置  
5. **Multi-client Testing**: エディタ複窓でのマルチプレイヤーテスト

### テスト・ビルドコマンド
```bash
# Unity エディタでの複窓テスト（同期挙動検証）
Unity.exe -projectPath . -username player1 &
Unity.exe -projectPath . -username player2 &

# Quest向けビルド（Android）
Unity -buildTarget Android -projectPath .

# PC向けビルド（Windows64）
Unity -buildTarget StandaloneWindows64 -projectPath .
```

## VRChat固有の制約・要件

### 同期変数・帯域制限
- **同期変数上限**: ~40個（現在red_0..5+meta=9個使用）
- **PlayerData容量**: キー長制限・値型制限（現在112+30+meta=約150キー）
- **帯域制限**: RequestSerialization頻度制御・Late-joiner対応必須

### UdonSharp制限
- **System.Collections制限**: Generic collections一部制限あり
- **非同期処理制限**: コルーチン・Task非対応
- **Quest制限**: 100MB容量・60FPS目標・モバイルGPU対応シェーダー

### 品質ゲート（Quality Gates）
- PlayerData キー互換性テスト
- 30ユーザー同時接続負荷テスト
- Quest 60FPS・PC 72FPS 達成確認
- Public公開前 Friends+ 1週間検証

## 重要な開発指針

### Claude Code利用時の制約
- UdonSharp対応コード生成を最優先とする
- 同期変数の追加・変更は最小限に留める
- PlayerDataキー管理の互換性を重視する  
- 外部API/DB連携は完全禁止
- VRChat SDK3 Worlds制約内での実装に限定

### 禁止事項
- 外部API/DB連携の追加
- 他ユーザーPlayerData書き込み
- 長文自由記述UIの導入
- 画像の永続保存
- 6D縮約を超える生データ公開

## Configuration Management

All data externalized as ScriptableObjects for runtime updates without rebuilds:
- 112 questions and answer choices
- Weight matrix W (112→30D transformation) 
- Projection matrix P (30D→6D public compression)
- Summary templates for auto-generated personality descriptions
- Performance parameters (K operations per frame, compatibility recalculation triggers)