以下の README は、Virtual Tokyo Matching（VTM）プロジェクトをクローンしてから VRChat へのワールド公開までを **最短経路**で案内する公式手引きです。  
（※Ubuntu22.04＋Unity 2022.3 LTS／VRChat SDK3 Worlds／UdonSharp 前提）

# Virtual Tokyo Matching – README

## 1. プロジェクト概要
Virtual Tokyo Matching は、VRChat ワールド内で 112 問のパーソナリティ診断を行い、6 次元ベクトルに圧縮した上でユーザー同士を 1 対 1 にマッチングする **完全オンワールド型**マッチングシステムです。  
プライバシー保護のため、未圧縮データはローカル保存のみ、公開同期は 6D ベクトルと自動生成プロフィールに限定しています。

## 2. 必要環境
- OS: Ubuntu 22.04 LTS  
- Unity: 2022.3 LTS（Hub 経由推奨）  
- VRChat SDK3 Worlds ＋ UdonSharp（VPM 経由で自動取得）  
- VRChat Creator Companion (VCC) / VPM CLI  
- Git, Bash, Python 3 系（ビルドスクリプト用）

## 3. クイックセットアップ

```bash
# 1. リポジトリ取得
git clone https://github.com/your-org/VirtualTokyoMatching.git
cd VirtualTokyoMatching

# 2. VPM で依存パッケージ解決
vpm resolve project .

# 3. Unity プロジェクト確認 & UdonSharp インストール
ls Assets/VirtualTokyoMatching/Scripts/  # 9つのコアコンポーネント確認
vpm list repos                           # VPM リポジトリ状況確認

# 4. Unity Editor 起動（UdonSharp インストール後）
~/Unity/Hub/Editor/2022.3.22f1/Editor/Unity -projectPath ~/projects/VirtualTokyoMatching

# 5. シーン自動生成
Unity -batchmode -quit -projectPath . \
  -executeMethod VirtualTokyoMatching.Editor.VTMSceneSetupTool.CreateSceneSetup \
  -logFile /tmp/unity_scene_setup.log
```

## 4. プロジェクト現在状況と対処済み問題

### ✅ 解決済みコンパイルエラー（2025年8月）
- **CS0592/CS1109/CS0246 エラー**: 全て修正済み
- **TestRunner 依存問題**: Unity Test Framework ファイルを `Tests_Backup/` に移動済み
- **VRChat SDKパッケージ**: 3.7.6 が組み込み済み（base, worlds）

### ⚠️ 現在の既知問題
| 問題 | 状況と対策 |
|------|-----------|
| UdonSharp パッケージ | VPM リポジトリアクセス不可のため手動インストール必要 |
| Unity Editor 起動 | UdonSharp 不足のため一部機能制限あり |

### 🔧 推奨セットアップ手順
```bash
# UdonSharp 手動インストール後に実行
vpm install com.vrchat.udonsharp
# または Unity Package Manager から手動インストール
```

## 5. ディレクトリ構成（抜粋）

```
Assets/VirtualTokyoMatching/
├─ Scripts/        # 9 主要コンポーネント
│  ├─ Core/PlayerDataManager.cs
│  ├─ Assessment/DiagnosisController.cs
│  ├─ Vector/VectorBuilder.cs
│  ├─ Matching/CompatibilityCalculator.cs
│  └─ …（略）
├─ Editor/         # 自動シーン生成ツール
├─ Resources/      # テキスト・UI・マテリアル
└─ ScriptableObjects/QuestionDatabase.asset
```

## 6. 開発ワークフロー

1. **トピックブランチ作成**
   ```bash
   git checkout -b feat/xxx
   ```
2. **コード実装**（`UdonSharpBehaviour` を継承・命名規則を遵守）
3. **自動テスト & ヘッドレスコンパイル**
   ```bash
   sh/vtm_headless_build.sh
   ```
4. **Unity Editor で動作確認 & ClientSim テスト**
5. **プルリクエスト → コードレビュー → main へマージ**

## 7. 性能・安全ガイドライン

- **PC 72 FPS / Quest 60 FPS** を維持できるよう `PerfGuard` で計算分散。  
- **ネットワーク同期は 6D ベクトルのみ**。112 問回答や 30D ベクトルはローカル保存。  
- **ユーザー安全**: 緊急非表示・データリセット・ブロック連携を実装済み。  

## 8. VRChat への公開手順

```bash
# 1. 最終ビルド（PC/Quest）
sh/vtm_headless_build.sh

# 2. Unity Editor → VRChat SDK → Build & Test (Private)
# 3. 問題なければ Friends+ β公開 → 1 週間フィードバック
# 4. Public Release
```

## 9. FAQ

- **Q. Unity Editor でコンパイルエラーが出る**  
  A. UdonSharp パッケージがインストールされているかを確認。`Window → UdonSharp → Settings` から状況確認可能。  

- **Q. Quest で FPS が落ちる**  
  A. `PerfGuard` の `budgetMsPerFrame` を調整。デフォルトは PC:5ms, Quest:8ms に設定済み。  

- **Q. VPM で UdonSharp がインストールできない**  
  A. 現在既知問題です。Unity Package Manager から手動で Git URL インストールを試してください。

- **Q. テストコードはどこにある？**  
  A. Unity Test Framework テストは `Tests_Backup/` に移動済み。実際のテストは `VTMSystemValidator.cs` を使用してください。  

***

開発上の詳細ポリシーやコード規約は `code_style_conventions.md` を、タスク完了基準は `task_completion_checklist.md` を参照してください。
