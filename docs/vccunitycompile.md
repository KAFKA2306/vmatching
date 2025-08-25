# Ubuntu 22.04での VCC/VPM CLI完全ガイド - VRChatワールド開発環境構築とトラブルシューティング

## 概要

Ubuntu 22.04環境での**VRChat Package Manager (VPM) CLI**を使用したVRChatワールド開発環境の完全構築ガイドです。**VirtualTokyoMatching**プロジェクト（112問性格診断ベースのリアルタイムマッチングシステム）の実装を通じて、実際のエラーケースとその解決方法を網羅的に解説します。

## エラー分析と根本原因

### 主要エラーの分類

#### 1. VPMコマンド構文エラー
```bash
# ❌ 間違った形式
vpm add com.vrchat.udonsharp
# エラー: Required command was not provided

# ✅ 正しい形式  
vpm add package com.vrchat.udonsharp -p .
```

**失敗理由**: VPMコマンドには`package`サブコマンドが必須で、プロジェクトパス指定(`-p .`)も必要

#### 2. settings.json構文エラー
```bash
# 典型的エラー出力
[ERR] Failed to load settings! Unexpected character encountered while parsing value: {. 
Path 'unityEditors', line 6, position 5.
```

**失敗理由**: 
- JSON構造の破損（中括弧・クォートの不整合）
- `cat`コマンドでのEOFマーカー処理失敗
- `unityEditors`フィールドの形式違い（配列 vs オブジェクト）

#### 3. プロジェクト認識エラー
```bash
# エラー例
Project version not found
Can't load project manifest ./Packages/manifest.json
```

**失敗理由**: プロジェクトルート以外のディレクトリでVPMコマンドを実行

#### 4. Unity Editor検出エラー
```bash
# Linux特有の制限
Found No Supported Editors  
Unity is not installed
```

**失敗理由**: Linux環境でのUnity自動検出機能未対応（既知の制限事項）

## 段階別解決手順

### Phase 1: 基本環境構築

#### 1.1 前提条件確認
```bash
# .NET SDK確認
dotnet --version
dotnet --list-sdks
# 必要: .NET 8.0以上

# Unity環境確認
which unityhub
find ~/Unity/Hub/Editor -name "Unity" -type f 2>/dev/null

# 期待する出力例
# /usr/bin/unityhub
# /home/kafka/Unity/Hub/Editor/2022.3.22f1/Editor/Unity
```

#### 1.2 VPM CLI基本セットアップ
```bash
# VPM CLI完全リセット（問題がある場合）
dotnet tool uninstall --global vrchat.vpm.cli
rm -rf ~/.dotnet/tools/.store/vrchat.vpm.cli

# 新規インストール
dotnet tool install --global vrchat.vpm.cli

# バージョン確認
vpm --version
# 期待する出力: 0.1.28+ハッシュ
```

#### 1.3 初期テンプレート導入
```bash
# 設定フォルダの完全リセット（必要に応じて）
rm -rf ~/.local/share/VRChatCreatorCompanion

# テンプレート導入
vpm install templates

# 導入確認
vpm list repos
# 期待する出力:
# com.vrchat.repos.official | Official (VRChat)
# com.vrchat.repos.curated | Curated (VRChat)
```

### Phase 2: settings.json完全設定

#### 2.1 問題のあるsettings.jsonの診断
```bash
# 現在のファイル内容確認
cat ~/.local/share/VRChatCreatorCompanion/settings.json

# JSON構文チェック
python3 -m json.tool ~/.local/share/VRChatCreatorCompanion/settings.json
```

#### 2.2 正しいsettings.json作成（完全版）
```bash
# 既存ファイルのバックアップ
cp ~/.local/share/VRChatCreatorCompanion/settings.json{,.backup.$(date +%Y%m%d_%H%M%S)}

# 実際のパスを取得
UNITY_HUB_PATH=$(which unityhub)
UNITY_EDITOR_PATH=$(find ~/Unity/Hub/Editor -name "Unity" -type f 2>/dev/null | head -1)
UNITY_VERSION=$(basename $(dirname $(dirname $UNITY_EDITOR_PATH)))

echo "Unity Hub: $UNITY_HUB_PATH"
echo "Unity Editor: $UNITY_EDITOR_PATH"  
echo "Unity Version: $UNITY_VERSION"

# 動的settings.json生成
cat > ~/.local/share/VRChatCreatorCompanion/settings.json << EOF
{
  "pathToUnityHub": "$UNITY_HUB_PATH",
  "pathToUnityExe": "$UNITY_EDITOR_PATH",
  "userProjects": [],
  "unityEditors": [
    {
      "version": "$UNITY_VERSION",
      "path": "$UNITY_EDITOR_PATH"
    }
  ],
  "preferredUnityEditors": {
    "${UNITY_VERSION%.*}": "$UNITY_VERSION"
  },
  "defaultProjectPath": "/home/$USER/projects",
  "lastUIState": 0,
  "skipUnityAutoFind": false,
  "userPackageFolders": [],
  "windowSizeData": {
    "width": 0,
    "height": 0,
    "x": 0,
    "y": 0
  },
  "skipRequirements": false,
  "lastNewsUpdate": "$(date -u +%Y-%m-%dT%H:%M:%S.000Z)",
  "allowPii": false,
  "projectBackupPath": "/home/$USER/.local/share/VRChatCreatorCompanion/ProjectBackups",
  "showPrereleasePackages": false,
  "trackCommunityRepos": true,
  "selectedProviders": 3,
  "userRepos": []
}
EOF

# ファイル権限設定
chmod 644 ~/.local/share/VRChatCreatorCompanion/settings.json
```

#### 2.3 設定検証
```bash
# JSON構文確認
python3 -m json.tool ~/.local/share/VRChatCreatorCompanion/settings.json >/dev/null && echo "JSON OK" || echo "JSON ERROR"

# VPM動作確認
vpm --version
vpm list repos
vpm check hub
vpm list unity  # Unity認識は制限ありでも問題なし
```

### Phase 3: プロジェクト作成と管理

#### 3.1 VirtualTokyoMatchingプロジェクト作成
```bash
# プロジェクトディレクトリ準備
mkdir -p ~/projects

# VRChatワールドプロジェクト新規作成
vpm new VirtualTokyoMatching World -p ~/projects

# 作成確認
cd ~/projects/VirtualTokyoMatching
ls -la
# 期待するファイル: Assets/, Packages/, ProjectSettings/, etc.

# プロジェクト状態確認
vpm check project .
# 期待する出力: Project is WorldVPM
```

#### 3.2 パッケージ追加（正しい手順）
```bash
# 必須パッケージの段階的追加
cd ~/projects/VirtualTokyoMatching

# 1. VRChat Worlds SDK（基本）
vpm add package com.vrchat.worlds -p .
echo "VRChat Worlds SDK追加完了"

# 2. UdonSharp（スクリプティング）
vpm add package com.vrchat.udonsharp -p .
echo "UdonSharp追加完了"

# 3. ClientSim（テスト用）
vpm add package com.vrchat.clientsim -p .
echo "ClientSim追加完了"

# 依存関係解決
vpm resolve project .

# 追加されたパッケージ確認
vpm list packages -p .
```

#### 3.3 互換性問題対処法
```bash
# パッケージ互換性エラーが発生した場合

# エラー例:
# Couldn't add com.vrchat.udonsharp@1.1.9 to target project. It is incompatible

# 解決方法1: 特定バージョン指定
vpm add package com.vrchat.udonsharp@1.1.8 -p .
vpm add package com.vrchat.clientsim@1.2.6 -p .

# 解決方法2: テンプレート更新
vpm install templates
vpm resolve project .

# 解決方法3: 段階的追加
vpm add package com.vrchat.worlds -p .
vpm resolve project .
vpm add package com.vrchat.udonsharp -p .
vpm resolve project .
vpm add package com.vrchat.clientsim -p .
vpm resolve project .
```

### Phase 4: Unity統合

#### 4.1 Unity Hubでプロジェクト開く
```bash
# Unity Hubをバックグラウンド起動
/usr/bin/unityhub -- --projectPath ~/projects/VirtualTokyoMatching &

# プロセス確認
ps aux | grep unityhub
```

#### 4.2 手動パッケージ追加（フォールバック）
Unity Editor内で以下を実行：

```csharp
// Window → Package Manager → + → Add package from git URL

// 1. VRChat Worlds SDK
https://github.com/vrchat/packages.git?path=/packages/com.vrchat.worlds

// 2. UdonSharp  
https://github.com/vrchat/packages.git?path=/packages/com.vrchat.udonsharp

// 3. ClientSim
https://github.com/vrchat/packages.git?path=/packages/com.vrchat.clientsim
```

## VirtualTokyoMatchingプロジェクト構造

### 完全なフォルダ構成
```
Assets/VirtualTokyoMatching/
├── Scripts/
│   ├── Core/                    # VTMController, PlayerDataManager
│   │   ├── PlayerDataManager.cs
│   │   ├── VTMController.cs
│   │   └── EventSystem.cs
│   ├── Assessment/              # 112問診断システム
│   │   ├── DiagnosisController.cs
│   │   ├── QuestionManager.cs
│   │   └── ProgressTracker.cs
│   ├── Vector/                  # 30D→6D変換・類似度計算
│   │   ├── VectorBuilder.cs
│   │   ├── DimensionReducer.cs
│   │   └── SimilarityCalculator.cs
│   ├── Matching/                # リアルタイム推薦
│   │   ├── CompatibilityCalculator.cs
│   │   ├── RecommendationEngine.cs
│   │   └── MatchingAlgorithm.cs
│   ├── UI/                      # ユーザーインターフェース
│   │   ├── MainUIController.cs
│   │   ├── RecommenderUI.cs
│   │   └── AssessmentUI.cs
│   ├── Safety/                  # プライバシー保護
│   │   ├── SafetyController.cs
│   │   ├── PrivacyManager.cs
│   │   └── DataProtection.cs
│   ├── Session/                 # 1on1個室管理
│   │   ├── SessionRoomManager.cs
│   │   ├── RoomController.cs
│   │   └── TimerSystem.cs
│   ├── Sync/                    # ネットワーク同期
│   │   ├── PublicProfilePublisher.cs
│   │   ├── NetworkSync.cs
│   │   └── DataSynchronizer.cs
│   └── Performance/             # パフォーマンス最適化
│       ├── PerfGuard.cs
│       ├── FrameRateOptimizer.cs
│       └── MemoryManager.cs
├── ScriptableObjects/
│   ├── QuestionDatabase.asset   # 112問・5択・軸マッピング
│   ├── VectorConfiguration.asset # 30D→6D変換行列
│   ├── SummaryTemplates.asset   # 性格要約テンプレート
│   └── PerformanceSettings.asset # 最適化設定
├── Scenes/
│   └── VirtualTokyoMatching.unity
├── Prefabs/
│   ├── UI/                      # UIプレファブ
│   │   ├── MainLobbyCanvas.prefab
│   │   ├── AssessmentUI.prefab
│   │   ├── RecommenderCards.prefab
│   │   └── SafetyPanel.prefab
│   ├── SessionRooms/            # 個室プレファブ
│   │   ├── PrivateRoom01.prefab
│   │   ├── PrivateRoom02.prefab
│   │   └── PrivateRoom03.prefab
│   └── Systems/                 # システムプレファブ
│       ├── VTMController.prefab
│       └── NetworkedProfiles.prefab
├── Materials/                   # UI・環境マテリアル
├── Textures/                    # テクスチャアセット
├── Audio/                       # 音響効果
└── Resources/                   # 設定アセット（ランタイムロード）
    ├── DefaultPerformanceSettings.asset
    ├── QuestionDatabase.asset
    ├── VectorConfig.asset
    └── SummaryTemplates.asset
```

## 高度なトラブルシューティング

### ケース1: 完全復旧手順
```bash
# VPM環境の完全リセット
#!/bin/bash
echo "=== VPM環境完全リセット開始 ==="

# 1. VPM CLI削除
dotnet tool uninstall --global vrchat.vpm.cli
rm -rf ~/.dotnet/tools/.store/vrchat.vpm.cli

# 2. 設定フォルダバックアップ・削除
if [ -d ~/.local/share/VRChatCreatorCompanion ]; then
    cp -r ~/.local/share/VRChatCreatorCompanion ~/.local/share/VRChatCreatorCompanion.backup.$(date +%Y%m%d_%H%M%S)
    rm -rf ~/.local/share/VRChatCreatorCompanion
fi

# 3. 新規インストール
dotnet tool install --global vrchat.vpm.cli
vpm install templates

# 4. 設定確認
vpm --version
vpm list repos

echo "=== リセット完了 ==="
```

### ケース2: ネットワーク問題診断
```bash
# VPMネットワーク接続確認
#!/bin/bash
echo "=== ネットワーク診断 ==="

# 1. 基本接続確認
ping -c 3 api.vrchat.cloud
ping -c 3 packages.vrchat.com

# 2. VRChatリポジトリアクセス確認
curl -I https://packages.vrchat.com/

# 3. VPMリポジトリ状態確認
vpm list repos

# 4. パッケージリスト取得テスト
timeout 30 vpm list packages --all

echo "=== 診断完了 ==="
```

### ケース3: パフォーマンス問題対処
```bash
# システムリソース確認
#!/bin/bash
echo "=== システムリソース確認 ==="

# メモリ使用量
free -h

# CPU使用率
top -bn1 | grep "Cpu(s)"

# ディスク容量
df -h ~/.local/share/VRChatCreatorCompanion
df -h ~/projects

# .NET プロセス確認
ps aux | grep dotnet

echo "=== 確認完了 ==="
```

## 具体的エラーパターンと解決コード

### エラーパターン1: JSON破損
```bash
# エラー出力例
Failed to load settings! Unexpected character encountered while parsing value: {. Path 'unityEditors', line 8, position 5.

# 診断コマンド
cat ~/.local/share/VRChatCreatorCompanion/settings.json | head -20
python3 -c "import json; json.load(open('/home/$USER/.local/share/VRChatCreatorCompanion/settings.json'))"

# 解決コード
cat > ~/.local/share/VRChatCreatorCompanion/settings.json << 'JSON_EOF'
{
  "pathToUnityHub": "/usr/bin/unityhub",
  "pathToUnityExe": "/home/kafka/Unity/Hub/Editor/2022.3.22f1/Editor/Unity",
  "userProjects": [],
  "unityEditors": [
    {
      "version": "2022.3.22f1",
      "path": "/home/kafka/Unity/Hub/Editor/2022.3.22f1/Editor/Unity"
    }
  ],
  "preferredUnityEditors": {},
  "defaultProjectPath": "/home/kafka/projects",
  "lastUIState": 0,
  "skipUnityAutoFind": false,
  "userPackageFolders": [],
  "windowSizeData": { "width": 0, "height": 0, "x": 0, "y": 0 },
  "skipRequirements": false,
  "lastNewsUpdate": "2025-08-25T11:58:00.000Z",
  "allowPii": false,
  "projectBackupPath": "/home/kafka/.local/share/VRChatCreatorCompanion/ProjectBackups",
  "showPrereleasePackages": false,
  "trackCommunityRepos": true,
  "selectedProviders": 3,
  "userRepos": []
}
JSON_EOF
```

### エラーパターン2: パッケージ追加失敗
```bash
# エラー出力例
Couldn't add com.vrchat.udonsharp@1.1.9 to target project. It is incompatible

# 診断コマンド
cd ~/projects/VirtualTokyoMatching
vpm check project .
vpm list packages -p .

# 解決コード（段階的追加）
vpm add package com.vrchat.worlds -p .
vpm resolve project .

# 互換バージョンで再試行
vpm add package com.vrchat.udonsharp@1.1.8 -p .
vpm resolve project .

vpm add package com.vrchat.clientsim@1.2.6 -p .
vpm resolve project .
```

### エラーパターン3: Unity Hub認識失敗
```bash
# エラー出力例
Can only get Hub version on Windows and Mac so far...
Found Unity Hub version  at /usr/bin/unityhub

# これは正常（Linux制限）- 機能に支障なし

# 確認コマンド
which unityhub
/usr/bin/unityhub --version

# Unity起動テスト
/usr/bin/unityhub -- --projectPath ~/projects/VirtualTokyoMatching
```

## 最適化とベストプラクティス

### VPM操作のベストプラクティス
```bash
# 1. 常にプロジェクトルートで実行
cd ~/projects/VirtualTokyoMatching
pwd  # 確認

# 2. -p . オプションを必ず付ける
vpm add package com.vrchat.worlds -p .
vpm check project .
vpm resolve project .

# 3. 段階的にパッケージ追加
vpm add package com.vrchat.worlds -p .
vpm resolve project .
vpm add package com.vrchat.udonsharp -p .
vpm resolve project .

# 4. 定期的な状態確認
vpm list packages -p .
vpm check project .
```

### Unity開発環境最適化
```bash
# Unity Hub メモリ最適化
echo 'export UNITY_HUB_DISABLE_CRASH_REPORTING=1' >> ~/.bashrc
echo 'export UNITY_HUB_DISABLE_ANALYTICS=1' >> ~/.bashrc

# Unity Editor設定最適化
mkdir -p ~/Unity/Hub/Editor/2022.3.22f1/Editor/Data/Resources
cat > ~/Unity/Hub/Editor/2022.3.22f1/Editor/Data/Resources/performance.cfg << 'EOF'
gc-max-time-slice=5
gc-incremental=true
EOF
```

## 代替ソリューション

### vrc-get（オープンソース代替）
```bash
# Rustベースの代替ツール
curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh
source ~/.cargo/env
cargo install vrc-get

# vrc-get使用例
vrc-get new VirtualTokyoMatching --template world
cd VirtualTokyoMatching
vrc-get add com.vrchat.udonsharp
vrc-get add com.vrchat.clientsim
```

### 完全手動セットアップ
```bash
# Unity Hubのみでのプロジェクト作成
/usr/bin/unityhub &

# GUIでの操作：
# 1. Projects → New project
# 2. 3D Core template
# 3. Project name: VirtualTokyoMatching
# 4. Location: ~/projects
# 5. Create project

# Unity Editor内でVRChat SDK手動追加
# Window → Package Manager → + → Add package from git URL
# https://github.com/vrchat/packages.git?path=/packages/com.vrchat.worlds
```

## まとめ：成功への道筋

### ✅ 確実に動作する環境
- **OS**: Ubuntu 22.04 LTS
- **Unity**: 2022.3.22f1（確認済み）
- **VPM CLI**: 0.1.28（基本機能動作）
- **Unity Hub**: 正常起動（プロジェクト管理）

### 📋 推奨ワークフロー
1. **VPM CLI**: プロジェクト作成・パッケージ管理
2. **Unity Hub**: 開発・ビルド・テスト
3. **手動設定**: Linux制限事項への対応
4. **代替手段**: 問題発生時のフォールバック

### 🎯 VirtualTokyoMatching実装目標
- **112問性格診断システム**: 進捗保存・再開対応
- **30D→6D変換**: プライバシー保護縮約
- **リアルタイムマッチング**: コサイン類似度ベース推薦
- **1on1個室システム**: 双方同意・15分タイマー
- **パフォーマンス最適化**: PC 72FPS / Quest 60FPS
- **プライバシー保護**: 公開は縮約データのみ

**結論**: Ubuntu 22.04でのVRChatワールド開発は完全に実現可能です。この包括的なガイドにより、Linux環境でのプロフェッショナルなVRChatワールド開発が実現でき、**VirtualTokyoMatching**プロジェクトを成功に導くことができます。