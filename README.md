# Virtual Tokyo Matching – VRChat World

## 概要
Virtual Tokyo Matching は、112問の性格診断を基盤に、回答途中からでも暫定ベクトルで推薦が進む「即時対話到達」型のマッチング体験を提供するVRChatワールド実装である。診断は30軸ベクトルに集約し、公開時は6軸縮約＋タグ／見出しのみを同期するプライバシー設計を採用している。[3][4][1]

## 主な特徴
- 112問・5択の進捗再開対応診断、回答確定ごとの自動保存と暫定ベクトル更新により、途中でも推薦と1on1導線へ進める。[4][1]
- 30D→6D縮約を前提にコサイン類似度で上位3件を算出し、イベント駆動かつPerfGuardでフレーム負荷を抑えた分散計算を行う。[1][3]
- 公開は同意ベースで6軸縮約＋タグ/見出しのみ、緊急非表示・即時非公開・データリセットなどの安全UIを完備する。[4][1]

## アーキテクチャ
- コア構成: PlayerDataManager／DiagnosisController／VectorBuilder／PublicProfilePublisher／CompatibilityCalculator／RecommenderUI／SessionRoomManager／ValuesSummaryGenerator／PerfGuard／SafetyController でイベント駆動連携する。[3][1]
- データ流: 回答→30D→6D→類似度→上位カード→1on1へ遷移し、保存は自分のPlayerDataのみ、公開同期は縮約データのみに限定する。[1][3]

## 前提環境
- OS: Ubuntu 22.04 LTS（実用運用例、GUIのVCCはWindows正式対応、LinuxはVPM CLI運用が現実解）。[2][4]
- Unity: 2022.3 LTS 系（Hub管理推奨、プロジェクトはSDK互換版を前提）。[5][4]
- パッケージ: VRChat SDK3 Worlds、UdonSharp、ClientSim（VPMで導入）。[6][2]
- VPM CLI: .NET 8 SDKが必要、vpm new/add/resolve等でプロジェクト管理・依存解決を実施する。[7][2]

## クイックスタート（VPM CLI）
- 概要: .NET 8 SDK→VPM CLI導入→テンプレ展開→新規プロジェクト作成→パッケージ追加→Unity Hub/Editorパス設定→Unityで開いてテストの順に進める。[2][7]
- 代表コマンド:  
  - dotnet tool install --global vrchat.vpm.cli（導入）[7]
  - vpm install templates（テンプレ更新）[7]
  - vpm new VirtualTokyoMatching World -p ~/projects（新規作成）[2]
  - cd ~/projects/VirtualTokyoMatching && vpm check project . && vpm resolve project .（検証/解決）[2]
  - vpm add package com.vrchat.worlds -p . && vpm add package com.vrchat.udonsharp -p . && vpm add package com.vrchat.clientsim -p .（追加）。[2]
- Linuxの要点: settings.jsonにUnity Hub/Editorパスを手動記入し、vpm check hub/unityで認識させると安定する。[8][2]

## プロジェクト構成
- Assets/VirtualTokyoMatching 配下に Scripts（機能別サブフォルダ）、ScriptableObjects、Prefabs、Scenes、Resources、Materials、Audio を整理する。[9][6]
- 主要スクリプト群はAnalysis/Assessment/Core/Matching/Performance/Safety/Session/Sync/UI/Vector に分割管理する。[9][6]

## 設定アセット（ScriptableObject）
- QuestionDatabase: 112問・5択・軸(0–29)・選択肢ウェイトのスキーマで作成する。[10][6]
- VectorConfiguration: 112→30DのW行列、30→6DのP行列、軸名/ラベルを設定する。[10][6]
- SummaryTemplates: 30軸の正負/中立の記述・タグ・見出しテンプレと閾値を設定する。[10][1]
- PerformanceSettings: 計算予算、目標FPS、同期間隔、テクスチャ上限、ログ可視化等を設定する。[10][1]

## シーン統合
- 階層: Environment（Lobby/SessionRooms/Lighting/Audio）／Systems（VTMController/NetworkedProfiles/SpawnSystem/WorldSettings）／UI（Main/Assessment/Recommender/Safety）で構成する。[11][9]
- VTMController 配下に各UdonBehaviourを配置し、Inspectorで依存参照とイベント配列（onDataLoaded/onQuestionAnswered/onVectorUpdated 等）を結線する。[11][9]
- NetworkedProfiles は想定人数分用意し、PublicProfilePublisherごとにユニークなNetwork IDを割り当てる。[9][11]

## ビルドとパフォーマンス
- 目標: PC≥72FPS/サイズ<200MB/全再計算≤5s、Quest≥60FPS/サイズ<100MB/全再計算≤10s を満たす。[4][1]
- 最適化: ベイク照明、テクスチャ上限（PC:2048/Quest:1024）、モバイル系シェーダ、ミップ/ストリーミングを既定とする。[6][9]
- テスト: エディタ複窓/ClientSimで同期・帯域・フレームを検証し、Friends+で1週間安定後にPublic化する運用とする。[6][1]

## プライバシー/安全
- 非公開データ: 112問回答と30軸はPlayerData（自分のみ）で保持し他者に公開しない。[1][4]
- 公開データ: 6軸縮約＋タグ/見出し＋進捗％＋暫定フラグのみを同意時に同期し、OFFや退出で即時クリアする。[12][4]
- 安全UI: 緊急非表示・退出・リセットを常時操作可能にし、公開OFF時は露出ゼロを保証する。[13][1]

## 推薦/1on1設計
- 類似度: 6軸同士のコサイン類似度で上位3件を算出し、進捗イベントで増分再計算する。[12][3]
- 個室導線: 双方同意→個室割当→Teleport→15分タイマー→終了ベル→帰還、満室/同時招待は優先順位と再試行で解消する。[14][13]

## トラブルシューティング（VPM/Linux）
- vpm add の用法: 必ず「vpm add package <ID>」の形式で、プロジェクト直下（Packages/manifest.jsonがある場所）で実行する。[15][8]
- settings.json 破損時: バックアップ→再生成→vpm install templates→vpm list repos で復旧し、Hub/Editorパスを再設定する。[8][15]
- 互換性エラー: パッケージ追加失敗時はテンプレ更新や resolve、対応バージョン選定で再試行する。[8][2]

## 法令/ガイドライン
- VRChatの利用規約・コミュニティガイドラインに準拠し、ソーシャル/デーティング系表現は年齢適合・同意掲示・撮影/配信注意の設計を守る。[13][1]
- 外部API/DBや生データ公開、長文自由記述、在室外推薦等は採用せず、単一ワールド内完結と露出最小化を原則とする。[3][4]

## 今後の進め方
- Resourcesに4種のScriptableObjectを最小構成で作成し、UI Prefabとシーン配線を完了させてローカル/ClientSimテストに進む。[9][2]
- 目標性能を満たすまでPerfGuardとアセット予算を調整し、Friends+での安定確認後にPublicへ移行する。[6][1]

## 参考（VPM CLIドキュメント）
- vpmの導入/更新/テンプレ/新規作成/検証コマンドの詳細は公式ドキュメント・NuGetパッケージ説明に準拠する。[16][7]
