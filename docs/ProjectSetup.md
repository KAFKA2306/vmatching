はい、依頼どおり ProjectSetup.md を**VPMベース**に書き直した内容を下記に示します。Ubuntu 22.04 での実用運用を前提に、プロジェクト作成から依存解決、シーン統合までをCLI中心で完結するフローに再構成しています。[1][2]

## 目的
この文書は、Virtual Tokyo Matching のUnityプロジェクトを、GUIのVCCではなくCLIのVPMを中心に構築・管理するための手順書として再編成したものです。Linux(Ubuntu 22.04)での実用運用を前提に、テンプレート作成、設定、依存解決、シーン統合、テストまでの要点を最短経路で記載します。[2][1]

## 前提条件
- OS: Ubuntu 22.04 LTS、ネットワーク接続、端末操作環境。[1]
- Unity Hub/Unity Editor 2022.3 LTS 系のローカル配置（手動パス指定想定）。[1]
- .NET SDK（VPMを実行するため、一般的に最新版を推奨）。[1]

## 1. VPM CLI の導入と初期化
- VPM CLI をグローバル導入（dotnetツール経由）し、テンプレートを展開します。[1]
  - 実行例: `dotnet tool install --global vrchat.vpm.cli` → `vpm install templates` → `vpm --version` で導入確認。[1]
- 初回作成直後に自動生成される設定フォルダ（~/.local/share/VRChatCreatorCompanion/）にテンプレートが配置されることを確認します。[1]

## 2. Ubuntu向け VPM 設定（settings.json）
- settings.json（~/.local/share/VRChatCreatorCompanion/settings.json）に Unity Hub/Editor のパスを手動で記載し、VPMがエディタを認識できるようにします。[1]
- 記述例（実環境のパスに置き換えて使用）。[1]
  - `pathToUnityHub`: 例 `/opt/unityhub/unityhub`。[1]
  - `pathToUnityExe`: 例 `/home/USERNAME/Unity/Hub/Editor/2022.3.6f1/Editor/Unity`。[1]
  - `unityEditors`: `version` と `path` のセットを配列で記載（Unity 2022.3系）。[1]
- 反映確認: `vpm check hub` と `vpm check unity` を実行し、認識結果を確認します。[1]

## 3. プロジェクトの新規作成（テンプレート）
- テンプレートからVRChat Worldsプロジェクトを作成します。[1]
  - 実行例: `vpm new VirtualTokyoMatching World -p ~/projects` → `cd ~/projects/VirtualTokyoMatching`。[1]
- 作成直後の状態を検証・解決します。[1]
  - 実行例: `vpm check project .` → `vpm resolve project .` → `vpm list packages -p .`。[1]
- 以降のパッケージ追加は `vpm add package <ID> -p .` を基本とし、解決が必要な場合は再度 `vpm resolve project .` を実行します。[1]

## 4. プロジェクト構成（Unity側ディレクトリ）
- 既存の実装を次の構成で配置します（Assets/VirtualTokyoMatching 以下）。[1]
  - Scripts（Analysis, Assessment, Core, Matching, Performance, Safety, Session, Sync, UI, Vector）。[1]
  - ScriptableObjects（QuestionDatabase, VectorConfiguration, SummaryTemplates, PerformanceSettings）。[1]
  - Prefabs（UI, Rooms, Systems）、Materials、Textures、Audio を用途別に配置します。[1]

## 5. ScriptableObject の作成と設定
- Resources 配下に以下4種のアセットを作成し、最小構成から投入します。[1]
  - QuestionDatabase: 112問・5択・軸(0–29)・選択肢ウェイトのスキーマに沿って作成。[1]
  - VectorConfiguration: 112→30DのW、30→6DのP、軸名/ラベルを設定。[1]
  - SummaryTemplates: 30軸の記述テンプレート、タグ、見出しテンプレートを設定。[1]
  - PerformanceSettings: フレーム予算、目標FPS、計算間隔などの実行パラメータを設定。[1]

## 6. シーン統合（最短ガイド）
- シーン階層は VirtualTokyoMatchingWorld をルートに、Environment/Systems/UI/Testing で整理します。[2]
- Systems/VTMController に各UdonBehaviour（PlayerDataManager, DiagnosisController, VectorBuilder, CompatibilityCalculator, PerfGuard, ValuesSummaryGenerator, MainUIController, SafetyController, SessionRoomManager）を配置します。[2]
- NetworkedProfiles を想定最大人数分設置し、PublicProfilePublisher ごとにユニークなNetwork IDを割り当てます。[2]
- UIは MainLobby（Screen Space）と Assessment/Recommendations/Safety（World Space）を設置し、TextMeshProや各Button/Slider参照をInspectorで結線します。[2]
- VRC Scene Descriptor を設定し、ロビーのSpawnPoints、Reference Camera、Respawn Height を指定します。[2]

## 7. 依存参照とイベント配線
- PlayerDataManager の onDataLoadedTargets に DiagnosisController、VectorBuilder、MainUIController、SafetyController を割り当てます。[2]
- DiagnosisController の onQuestionAnsweredTargets に VectorBuilder と PublicProfilePublisher を割り当てます。[2]
- VectorBuilder の onVectorUpdatedTargets と onVectorFinalizedTargets に PublicProfilePublisher と CompatibilityCalculator を割り当てます。[2]
- CompatibilityCalculator の完了イベントを RecommenderUI に接続し、UI更新のトリガにします。[2]

## 8. パフォーマンスとビルド
- PC目標: 72fps/200MB以下/再計算≤5s、Quest目標: 60fps/100MB以下/再計算≤10s を目安に最適化します。[1]
- テクスチャ上限（PC:2048、Quest:1024）、ベイク照明、最小限のリアルタイムライト、軽量シェーダ運用を徹底します。[1]
- ビルド設定は PC(Standalone Windows 64-bit) と Quest(Android) を用意し、サイズ・品質・圧縮方式（ASTC等）を目標に合わせて調整します。[1]

## 9. テストワークフロー
- エディタ内で UI 遷移、保存/再開、公開/非公開、レコメンド更新、1on1招待までを一通り検証します。[1]
- 複数インスタンスや実機VRChatクライアントでの挙動確認を行い、同期・帯域・フレームタイムをログと合わせて点検します。[1]
- 目標を満たしたら Friends+ で1週間の安定検証を行い、その後 Public 化します。[1]

## 10. セキュリティ/プライバシー要点
- 112問の生回答と30Dベクトルは非公開、公開は6D縮約＋タグ/ヘッドラインのみで、公開は明示同意ベースです。[1]
- 緊急非表示やセッション退出、データリセットをSafety UIから常時操作可能にします。[1]

## 付録A: よく使うVPMコマンド（例）
- 初期化: `dotnet tool install --global vrchat.vpm.cli` → `vpm install templates`。[1]
- 新規作成: `vpm new VirtualTokyoMatching World -p ~/projects` → `cd ~/projects/VirtualTokyoMatching`。[1]
- 検証/解決: `vpm check project .` → `vpm resolve project .` → `vpm list packages -p .`。[1]
- 追加/削除: `vpm add package <ID> -p .`、`vpm remove package <ID> -p .`（解決が必要なら resolve を再実行）。[1]

## 付録B: 次に行う作業（チェックリスト）
- Resourcesに4種のScriptableObjectを作成し、最小データを投入。[1]
- Systems/UI/NetworkedProfilesを配置し、Inspector参照とイベント配線を完了。[2]
- 目標パフォーマンスを満たすまで品質と負荷を調整し、Friends+→Public へ段階的に公開。[1]

以上を新しい ProjectSetup.md（VPM版）として保存すれば、Linux中心の実運用に適した、再現性の高いセットアップ手順として利用できます。[2][1]
