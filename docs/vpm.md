# Ubuntu 22.04での VPM CLI完全ガイド - VRChatワールド開発環境構築

## 概要

Ubuntu 22.04環境での**VRChat Package Manager (VPM) CLI**を使用したVRChatワールド開発環境の完全構築ガイドです。**VirtualTokyoMatching**プロジェクト（112問性格診断ベースのリアルタイムマッチングシステム）の実装を通じて、実用的な開発環境を構築します。

## 前提条件

- **OS**: Ubuntu 22.04 LTS (WSL2も対応)
- **.NET SDK**: 8.0以上
- **Unity Hub**: インストール済み
- **Unity Editor**: 2022.3 LTS系 (VRChat推奨版)
- **ネットワーク**: インターネット接続 (パッケージダウンロード用)

## Linux環境での重要な制限事項

### ✅ 動作する機能
- VPM CLI基本操作 (`vpm --version`, `vpm list repos`)
- Unity Hub認識 (`vpm check hub`)
- VRChatリポジトリアクセス (公式・キュレーテッド)
- プロジェクト作成・パッケージ管理
- 依存関係解決

### ❌ 制限のある機能
- **Unity Editor自動検出**: Linux未対応（手動設定必須）
- **GUI VCC**: Windows専用
- **完全自動セットアップ**: 一部手動設定が必要

## Phase 1: 基本環境構築

### 1.1 .NET SDK確認・インストール

```bash
# .NET SDKのバージョン確認
dotnet --version
dotnet --list-sdks

# .NET 8.0がない場合の最新インストール
sudo apt update
sudo apt install -y dotnet-sdk-8.0
```

### 1.2 VPM CLI初期導入

```bash
# VPM CLIをグローバルインストール
dotnet tool install --global vrchat.vpm.cli

# バージョン確認
vpm --version
# 期待する出力: 0.1.28+ハッシュ

# テンプレートとリポジトリの初期化
vpm install templates

# リポジトリアクセス確認
vpm list repos
# 期待する出力:
# com.vrchat.repos.official | Official (VRChat)
# com.vrchat.repos.curated | Curated (VRChat)
```

### 1.3 Unity環境の確認

```bash
# Unity Hubの場所確認
which unityhub
find /opt -name "unityhub" 2>/dev/null

# Unity Editorの場所確認
find ~/Unity/Hub/Editor -name "Unity" -type f 2>/dev/null

# 実行可能性テスト
~/Unity/Hub/Editor/2022.3.22f1/Editor/Unity --version
```

**期待する出力例**:
```
/usr/bin/unityhub
/home/username/Unity/Hub/Editor/2022.3.22f1/Editor/Unity
2022.3.22f1
```

## Phase 2: VPM設定の完全構築

### 2.1 settings.json設定（重要）

Linux環境では`settings.json`の手動設定が**必須**です：

```bash
# 設定ファイルの場所確認
ls -la ~/.local/share/VRChatCreatorCompanion/settings.json

# 正しいsettings.jsonの作成
cat > ~/.local/share/VRChatCreatorCompanion/settings.json << 'EOF'
{
  "pathToUnityHub": "/usr/bin/unityhub",
  "pathToUnityExe": "/home/USERNAME/Unity/Hub/Editor/2022.3.22f1/Editor/Unity",
  "userProjects": [],
  "unityEditors": [
    {
      "version": "2022.3.22f1",
      "path": "/home/USERNAME/Unity/Hub/Editor/2022.3.22f1/Editor/Unity"
    }
  ],
  "preferredUnityEditors": {
    "2022.3": "2022.3.22f1"
  },
  "defaultProjectPath": "/home/USERNAME/projects",
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
  "lastNewsUpdate": "2025-08-25T11:56:00.000Z",
  "allowPii": false,
  "projectBackupPath": "/home/USERNAME/.local/share/VRChatCreatorCompanion/ProjectBackups",
  "showPrereleasePackages": false,
  "trackCommunityRepos": true,
  "selectedProviders": 3,
  "userRepos": []
}
EOF

# USERNAMEを実際のユーザー名に置換
sed -i "s/USERNAME/$USER/g" ~/.local/share/VRChatCreatorCompanion/settings.json

# ファイル権限設定
chmod 644 ~/.local/share/VRChatCreatorCompanion/settings.json
```

### 2.2 設定の検証

```bash
# JSON構文の確認
python3 -m json.tool ~/.local/share/VRChatCreatorCompanion/settings.json

# VPM動作確認
vpm --version
vpm check hub
vpm list repos

# Unity認識確認（Linuxでは制限あり）
vpm list unity
vpm check unity
```

**期待する結果**:
- `vpm check hub`: Unity Hubパスが表示される
- `vpm list repos`: 公式・キュレーテッドリポジトリが表示される
- `vpm list unity`: Unity Editorが認識される（または空のテーブル）

## Phase 3: VirtualTokyoMatchingプロジェクト作成

### 3.1 新規プロジェクト作成

```bash
# プロジェクト作成ディレクトリの準備
mkdir -p ~/projects

# VRChatワールドプロジェクトの新規作成
vpm new VirtualTokyoMatching World -p ~/projects

# プロジェクトディレクトリに移動
cd ~/projects/VirtualTokyoMatching

# プロジェクト状態の確認
vpm check project .
```

**期待する出力**:
```
Project is WorldVPM
```

### 3.2 必須パッケージの追加

```bash
# VRChat Worlds SDK（基本パッケージ）
vpm add package com.vrchat.worlds -p .

# UdonSharp（スクリプティング）
vpm add package com.vrchat.udonsharp -p .

# ClientSim（テスト用）
vpm add package com.vrchat.clientsim -p .

# 依存関係の解決
vpm resolve project .

# 追加されたパッケージの確認
vpm list packages -p .
```

### 3.3 パッケージ互換性問題の対処

```bash
# 互換性エラーが発生した場合の対処法

# 1. 特定バージョンでの追加
vpm add package com.vrchat.udonsharp@1.1.8 -p .
vpm add package com.vrchat.clientsim@1.2.6 -p .

# 2. プロジェクトの再解決
vpm resolve project .

# 3. キャッシュクリア（必要に応じて）
rm -rf ~/.local/share/VRChatCreatorCompanion/Packages
vpm install templates
```

## Phase 4: Unity統合・開発環境完成

### 4.1 Unity Hubでプロジェクトを開く

```bash
# Unity Hubを起動（GUIモード）
/usr/bin/unityhub -- --projectPath ~/projects/VirtualTokyoMatching &

# または、Unity Hubを起動してGUIで追加：
# 1. Unity Hub → Projects → Add → ~/projects/VirtualTokyoMatching
# 2. Unity 2022.3.22f1で開く
```

### 4.2 手動パッケージ追加（VPMが失敗する場合）

Unity Editor内で以下を実行：

```csharp
// Window → Package Manager → + → Add package from git URL

// VRChat Worlds SDK
https://github.com/vrchat/packages.git?path=/packages/com.vrchat.worlds

// UdonSharp
https://github.com/vrchat/packages.git?path=/packages/com.vrchat.udonsharp

// ClientSim
https://github.com/vrchat/packages.git?path=/packages/com.vrchat.clientsim
```

### 4.3 VirtualTokyoMatchingプロジェクト構造

```
Assets/VirtualTokyoMatching/
├── Scripts/
│   ├── Core/                    # VTMController, PlayerDataManager
│   ├── Assessment/              # 112問診断システム
│   ├── Vector/                  # 30D→6D変換・類似度計算
│   ├── Matching/                # リアルタイム推薦
│   ├── UI/                      # ユーザーインターフェース
│   ├── Safety/                  # プライバシー保護
│   ├── Session/                 # 1on1個室管理
│   ├── Sync/                    # ネットワーク同期
│   └── Performance/             # パフォーマンス最適化
├── ScriptableObjects/
│   ├── QuestionDatabase.asset   # 112問・5択・軸マッピング
│   ├── VectorConfiguration.asset # 30D→6D変換行列
│   ├── SummaryTemplates.asset   # 性格要約テンプレート
│   └── PerformanceSettings.asset # 最適化設定
├── Scenes/
│   └── VirtualTokyoMatching.unity
├── Prefabs/
│   ├── UI/                      # UIプレファブ
│   ├── SessionRooms/            # 個室プレファブ
│   └── NetworkedProfiles/       # 同期プロファイル
└── Resources/                   # 設定アセット
```

## Phase 5: トラブルシューティング

### 5.1 よくある問題と解決方法

#### 問題1: VPM CLIが設定ファイルを読み込めない

**エラー例**:
```
Failed to load settings! Please fix or delete your settings file
Unexpected character encountered while parsing value
```

**解決方法**:
```bash
# 設定ファイルを削除して再生成
rm -f ~/.local/share/VRChatCreatorCompanion/settings.json
vpm install templates

# 正しい設定ファイルを再作成（Phase 2.1を参照）
```

#### 問題2: Unity Editorが認識されない

**エラー例**:
```
Found No Supported Editors
Unity is not installed
```

**解決方法**:
```bash
# settings.jsonのunityEditorsセクションを確認
cat ~/.local/share/VRChatCreatorCompanion/settings.json | grep -A 10 "unityEditors"

# パスが正しいか確認
ls -la /home/$USER/Unity/Hub/Editor/2022.3.22f1/Editor/Unity

# 設定ファイルを修正（Phase 2.1参照）
```

#### 問題3: パッケージ追加時の互換性エラー

**エラー例**:
```
Couldn't add com.vrchat.udonsharp@1.1.9 to target project. It is incompatible
```

**解決方法**:
```bash
# 互換性のあるバージョンを指定
vpm add package com.vrchat.udonsharp@1.1.8 -p .

# または Unity Editor内で手動追加
# Window → Package Manager → Add package from git URL
```

### 5.2 設定ファイル破損時の完全復旧

```bash
# VPM設定の完全リセット
rm -rf ~/.local/share/VRChatCreatorCompanion

# VPM CLIの再インストール
dotnet tool uninstall --global vrchat.vpm.cli
dotnet tool install --global vrchat.vpm.cli

# 新しい設定の作成
vpm install templates

# 正しいsettings.jsonの設定（Phase 2.1参照）
```

### 5.3 デバッグ・診断コマンド

```bash
# VPM状態の完全診断
echo "=== VPM CLI バージョン ==="
vpm --version

echo "=== .NET SDK 状態 ==="
dotnet --version
dotnet --list-sdks

echo "=== Unity環境 ==="
vpm check hub
vpm check unity
vpm list unity

echo "=== プロジェクト状態 ==="
cd ~/projects/VirtualTokyoMatching
vpm check project .
vpm list packages -p .

echo "=== 設定ファイル確認 ==="
ls -la ~/.local/share/VRChatCreatorCompanion/
head -20 ~/.local/share/VRChatCreatorCompanion/settings.json
```

## Phase 6: 代替アプローチ

### 6.1 vrc-get（オープンソース代替）

VPM CLIに問題がある場合の代替ツール：

```bash
# Rustがインストールされている場合
cargo install vrc-get

# vrc-getでプロジェクト管理
vrc-get new VirtualTokyoMatching --template world
vrc-get add com.vrchat.udonsharp
vrc-get add com.vrchat.clientsim
```

### 6.2 Unity Hub直接使用

最も安定したアプローチ：

```bash
# Unity Hubでプロジェクト作成
/usr/bin/unityhub &

# GUIで実行：
# 1. Projects → New project → 3D Core
# 2. Project name: VirtualTokyoMatching
# 3. Location: ~/projects
# 4. Create project

# Unity Editor内でVRChat SDKを手動追加
# Window → Package Manager → + → Add package from git URL
```

## VirtualTokyoMatching実装ロードマップ

### Phase A: 中核システム実装
1. **112問性格診断システム**
   - QuestionDatabase (ScriptableObject)
   - 進捗保存・再開機能
   - 暫定ベクトル更新

2. **30軸→6軸変換システム**
   - ベクトル変換行列
   - プライバシー保護縮約
   - リアルタイム計算

3. **マッチングアルゴリズム**
   - コサイン類似度計算
   - 上位3名推薦表示
   - 進捗ベース暫定マッチング

### Phase B: VRChatワールド機能
1. **Udon/UdonSharpスクリプト**
   - PlayerDataManager（個人データ管理）
   - PublicProfilePublisher（縮約データ同期）
   - SessionRoomManager（1on1個室システム）

2. **UI/UXシステム**
   - 診断進捗表示
   - 推薦カード表示UI
   - 安全機能（緊急非表示・データリセット）

3. **ネットワーク同期**
   - 公開データのみ同期
   - 双方同意→個室移動
   - 15分タイマー→自動帰還

### Phase C: 最適化・公開
1. **パフォーマンス最適化**
   - 目標：PC 72FPS / Quest 60FPS
   - PerfGuard実装
   - 分散計算システム

2. **テスト・公開**
   - エディタ／ClientSimテスト
   - Friends+での安定性確認
   - Public公開

## 結論

Ubuntu 22.04でのVRChatワールド開発は**完全に実現可能**です：

### ✅ 構築完了した環境
- **Unity 2022.3.22f1**: 動作確認済み
- **Unity Hub**: 正常起動
- **VPM CLI**: 基本機能動作（Linux制限考慮）
- **VRChatプロジェクト**: 作成済み・SDK追加済み

### 📋 推奨開発フロー
1. **VPM CLI**: プロジェクト管理・パッケージ追加
2. **Unity Hub**: プロジェクト開発・ビルド
3. **手動設定**: Linux特有の制限への対応
4. **代替ツール**: 問題発生時のフォールバック

**VirtualTokyoMatching**は112問性格診断ベースのリアルタイムマッチングシステムとして、Ubuntu環境で完全に開発・公開可能です。この包括的なガイドにより、Linux環境でのプロフェッショナルなVRChatワールド開発が実現できます。
