プロジェクト概要
バーチャル東京マッチングは、自動性格分析を利用したマッチング用のUnityHubベースVRChatワールドです。Unity 2022 LTS + VCC、SDK3 Worlds、およびUdonSharpを使用して構築されています。

基本アーキテクチャ
本システムは外部API/データベースに依存しないスタンドアロン型のVRChatワールドであり、段階的なマッチングを実現しています。ユーザーが質問票を完全回答していなくても推薦結果を表示できる点が特徴です。主要コンポーネントは以下の通りです：

PlayerDataManager: 各ユーザーの永続データを管理します（質問1～112、30次元ベクトル、フラグ情報）
診断コントローラ: 112問の性格診断アンケートを管理し、自動保存/再開機能を備えています
VectorBuilder: 各回答ごとにベクトルを段階的に更新します。途中回答時には暫定ベクトルを使用し、回答完了時に最終的な正規化を行います
PublicProfilePublisher: 公開状態の切り替え管理、6次元に圧縮したデータを進捗インジケーターと暫定フラグ付きでブロードキャストします
相性計算処理: イベント駆動型のコサイン類似度計算を実装。回答更新時にはキューに登録され、再計算が順次実行されます。
RecommenderUI: 暫定バッジと進捗率を表示しながら、上位3件の相性マッチング結果を表示します
SessionRoomManager: 1対1のプライベートルームへの転送処理と15分間のタイマー管理を担当
ValuesSummaryGenerator: ベクトルデータから性格特性サマリーを生成します（手動によるプロフィール設定は不要）
PerfGuard: フレーム制限付き分散処理と、段階的な再計算トリガー機能
データ設計
PlayerData（非公開、ユーザーごと）
diag_q_001 から diag_q_112: int型（0～5、0は未回答状態）
vv_0～vv_29: float型（-1.0～+1.0） - 回答中は暫定値を表示し、回答完了後に正規化されます
flags: int型（ビットフィールド：公開状態 ON=1、暫定公開許可=2など）
act_last_active: int型（アクティビティのタイムスタンプ）
公開同期データ（ユーザーが許可した場合、暫定データを含む）
red_0 から red_5: float型（固定投影行列 P による6次元圧縮ベクトル）
tags: 文字列（自動生成される性格タグ）
headline: 文字列（自動生成される要約テキスト）
display_name: 文字列（自由記述形式のプロフィールは許可されません）
partial_flag: bool型（暫定データ/不完全データであることを示すフラグ）
progress: int型（0～112）、または progress_pct: float型（0～1）で進捗率を表示
主な制約事項
パフォーマンス: PC ≥72FPS、Quest ≥60FPS、データサイズ PC<200MB/Quest<100MB
プライバシー保護: 回答内容や30次元ベクトルは他者に一切公開されず、6次元に圧縮されたデータのみが共有されます
外部依存なし：すべての機能はVRChatワールドの制約内で動作する必要があります
手動プロフィールの禁止: 性格データはすべて質問票の回答から自動生成されます
セッション限定画像: アクティブなセッション中のみ表示されるアバターのスナップショットで、保存はされません
段階的マッチングシステム
主な革新点: ユーザーは質問票が不完全な状態でも、おすすめの提案を確認でき、会話を開始することが可能です。

ベクトルの段階的更新: 各回答が即座に暫定的な30次元ベクトルを更新します（未回答項目はゼロで初期化）
イベント駆動型再計算: 回答確認が行われると、影響を受けるすべてのユーザーに対して互換性の再計算がキューに追加されます
暫定的なUI表示: おすすめカードには「暫定」バッジと進捗率が表示されます
回答の信頼性に応じた重み付け：初期の回答ほど重みが高く、暫定ベクトルのコサイン類似度におけるノルムは自然に小さくなります
パフォーマンスアーキテクチャ
フレーム予算: 分散処理を考慮した1フレームあたりK回の演算制限
漸進的更新方式: ベクトルが変更された場合にのみ相性を再計算します（回答イベント、ユーザーの参加/離脱、表示切り替え時など）
6次元圧縮処理: 公開マッチングでは圧縮ベクトルを使用します（30次元→6次元への固定射影行列Pによる変換）
メモリ制約: PC版は200MB未満、Quest版はワールド全体のサイズが100MB未満
実装状況（Implementation Status）
プロジェクトは設計完了・実装段階にあります：

✅ 主要スクリプト: 9/9完了 - 主要コンポーネントすべて実装済み
❌ Unityプロジェクト: 初期化されていません（VCCのセットアップが必要です）
❌ 3Dシーン: UIプレハブとワールドジオメトリが未実装
✅ アーキテクチャ: イベント駆動型で分散処理対応済み
実装済みコンポーネント
PlayerDataManager: PlayerData永続化・復元・リトライ機構
診断コントローラ: 112問のUI・中断/再開・自動進行機能
VectorBuilder: 30D暫定ベクトル・増分更新・正規化
PublicProfilePublisher: 30日間→6日間へのデータ縮約・同期配布・公開制御
CompatibilityCalculator: コサイン類似度の計算、分散分析、上位3件の選出
PerfGuard: FPS監視・計算予算管理・適応的スロットリング
RecommenderUI: 推薦カード・詳細UI・暫定バッジ・招待機能
SessionRoomManager: 1on1個室管理・招待システム・15分タイマー
ValuesSummaryGenerator: 30Dから自動要約生成
開発ワークフロー
開発は Unity 2022 LTS + VCC + SDK3 Worlds + UdonSharp 構成で進めます：

必須セットアップ手順
VCC Setup: VRChat Creator Companion で SDK3 Worlds プロジェクト作成
UdonSharp: UdonSharp パッケージ導入
Scripts Integration: Assets/VirtualTokyoMatching/Scripts/ を Unity Project に配置
Scene Creation: UI Prefab・3D空間・テレポートポイント配置
Multi-client Testing: エディタ複窓でのマルチプレイヤーテスト
テスト・ビルドコマンド
# Unity エディタでの複窓テスト（同期挙動検証）
Unity.exe -projectPath . -username player1 &
Unity.exe -projectPath . -username player2 &

# Quest向けビルド（Android）
Unity -buildTarget Android -projectPath .

# PC向けビルド（Windows64）
Unity -buildTarget StandaloneWindows64 -projectPath .
VRChat固有の制約・要件
同期変数および帯域制限
同期変数の上限: 約40個（現在red_0～5およびmeta変数9個を使用）
PlayerData容量: キー長制限・値型制限（現在112+30+meta=約150キー）
帯域制限: RequestSerializationの頻度制御およびLate-joiner対応が必須です。
UdonSharpの制限事項
System.Collections制限: Generic collections一部制限あり
非同期処理制限: コルーチン・Task非対応
Quest制限: 100MB容量・60FPS目標・モバイルGPU対応シェーダー
品質ゲート（Quality Gates）
PlayerData キー互換性テスト
30ユーザー同時接続負荷テスト
クエストの60FPS・PCの72FPS達成を確認
Public公開前 Friends+ 1週間検証
重要な開発指針
Claude Code利用時の制約
UdonSharp対応コード生成を最優先とする
同期変数の追加・変更は最小限に留める
PlayerDataキー管理の互換性を重視する
外部API/DB連携は完全禁止
VRChat SDK3 Worlds制約内での実装に限定
禁止事項
外部API/DB連携の追加
他ユーザーPlayerData書き込み
長文自由記述UIの導入
画像の永続保存
6D縮約を超える生データ公開
設定管理
すべてのデータをScriptableObjectとして外部化し、ビルドなしでランタイム更新可能：

112の質問と回答選択肢
重み行列 W（112次元→30次元変換）
投影行列 P（30次元→6次元への公開圧縮）
自動生成される性格説明文用のテンプレート
パフォーマンスパラメータ（フレームあたりの演算回数、互換性再計算のトリガー条件）