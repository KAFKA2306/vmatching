## 位置同期とTransform運用
- 個室テレポート時の座標/向き、安全な着地（コライダー/段差）、再入室時の位置復元、外周逸脱時のリスポーンなど、[`VRCPlayerApi.TeleportTo()`](https://docs.vrchat.com/docs/vrcplayerapi#teleportto) / [`Respawn()`](https://docs.vrchat.com/docs/vrcplayerapi#respawn) を前提とした導線定義が未記述で、トリガー検出や無限ループ対策（連続発火抑止）を含む実装規約が必要である。[2][4]
- 「ロビー⇄個室⇄帰還」の各動線で軌跡/向きの整合を担保するため、テレポート先アンカーとStationの併用、個室占有中の他者侵入防止（ドア/コライダー層）を位置レベルで仕様化するのが安全である。[1][2]

## テレポート/リスポーンの具体
- UdonSharpでは[`OnPlayerTriggerEnter()`](https://docs.vrchat.com/docs/udonsharp#onplayertriggerenter) などのイベントから[`Networking.LocalPlayer.TeleportTo()`](https://docs.vrchat.com/docs/networking#localplayerteleportto) で安全に移動させられるため、招待同意→個室割当→テレポの一連を例外処理付きで標準化するべきである。[5][2]
- 迷子/落下・個室リーク検出時の自動Respawnと、スクリプト由来の誤テレポ抑止（クールダウン/一回限りトリガー）をガイドライン化してテスト項目に含めると復旧性が高まる。[4][1]

## 所有権と同期レート設計
- UdonはOwnerのみが同期変数を送信でき、[`RequestSerialization()`](https://docs.vrchat.com/docs/udonsharp#requestserialization) は呼び過ぎるとレート制限と待機が入るため、イベント駆動再計算と併せて「所有権の保持/移譲」と「送信頻度（デバウンス/バッチ化）」の数値基準が必要である。[6][1]
- 実効帯域は理論値より小さい環境も観測されており（フォーラム事例では数KB/sで頭打ち）、red/tags/headline等の同期ペイロードと招待イベントの合計を帯域予算内に収める設計が望ましい。[7][2]

## Late-joiner整合と初期化
- Manual同期の初期化はStart直後では未同期で[`OnPlayerJoined()`](https://docs.vrchat.com/docs/udonsharp#onplayerjoined) 時に揃う前提があるため、公開データ/ランキング/UIの初期描画タイミングを「OnPlayerJoined後」に寄せる設計指針を明記する必要がある。[6][1]
- 所有権移譲は一時的に両側で[`IsOwner()`](https://docs.vrchat.com/docs/udonsharp#isowner) が真に見える瞬間があり得るため、[`SetOwner()`](https://docs.vrchat.com/docs/networking#setowner) 直後のフレームで副作用処理を避け、確定後イベントでUI/状態遷移させる保護が必要である。[1][6]

## 個室の占有と導線衝突
- 個室の占有テーブル、同時招待の衝突解決（優先順位/再試行/再掲示）、満室時の退避座標/待機場所、セッション終了時の残骸清掃（タイムアウトで強制解放）をネットワークオブジェクトと座標で規定する必要がある。[2][1]
- 招待拒否・ブロック時は誘導先（ロビー固定点）とクールダウンを合わせて、連打・押し込みを抑止するUI/ネットワーク設計を加えるのが望ましい。[3][2]

## 音声/視覚の空間分離
- 1on1のプライバシー担保には、個室の遮音（フォールオフ距離/減衰）と壁・視界遮蔽の併用が必要で、ベル/終了音の聴取範囲もロビーに被らないよう音源の配置と層設計を追加したい。[2][1]
- 個室エリアのレンダラー/ライト/パーティクルをエリア単位でオン/オフし、ロビーと分離して描画コストを一定化する設計を補うべきである。[8][2]

## Quest向け容量/描画予算
- Quest側の容量上限は実運用で100MB目安が知られており、サイズ最適化（圧縮/テクスチャ上限/Crunch/MipmapStreaming）とMobile系シェーダでの削減をCIで自動検査する規約があると安定する。[9][3]
- ワールドサイズだけでなく、ドローコール/ライト/シャドウ/パーティクル/マテリアル種別などの上限目安を「ロビー/個室」別に明示し、K上限（計算）との合算フレーム予算を表で管理したい。[8][2]

## プレイヤーデータと位置情報の扱い
- [`VRCPlayerApi()`](https://docs.vrchat.com/docs/vrcplayerapi) で他者の位置/トラッキング情報は参照可能だが、配列管理・入退室に同期した再構築・Late-joiner同期が必要であり、ストレージ（PlayerData）には保存しない運用を明記すると混同が避けられる。[4][1]
- 位置可視化や近接ベースのUI（例：招待ボタンの近接有効化）を行うなら、[`GetPlayers()`](https://docs.vrchat.com/docs/vrcplayerapi#getplayers) / [`OnPlayerJoined()`](https://docs.vrchat.com/docs/udonsharp#onplayerjoined) / [`OnPlayerLeft()`](https://docs.vrchat.com/docs/udonsharp#onplayerleft) での配列管理規約と更新頻度（ポーリング/イベント駆動）を定義する必要がある。[4][2]

## ネットワーク・復旧・テスト
- 同期頻度/RequestSerializationのクールダウン、[`OnDeserialization()`](https://docs.vrchat.com/docs/udonsharp#ondeserialization) での差分適用、帯域「苦境」時の挙動（優先度付きキュー/送信間引き）のルールをPerfGuardと統合して数値管理する設計が必要である。[6][1]
- 位置系シナリオ（テレポ連鎖/Station乗降/遮音ゾーン/Late-joiner復元/個室満室）をPC/Questで再現テストし、違反時はビルドを止める品質ゲート（容量/フレーム/ネットワーク送出量）をCIに組み込みたい。[5][8]

## 具体的な追記提案（設計への組み込み）
- 位置・導線: テレポート先アンカー命名規約、Station使用指針、落下復帰（Respawn/安全床）と無限トリガー対策（フラグ＋クールダウン）を[`design.md`](docs/design.md)に追記。[5][2]
- 所有権/同期: Owner保持方針、SetOwnerの禁止/許可条件、RequestSerializationの最小間隔・バッチ化・帯域上限（KB/s）と優先度キューを[`architecture.md`](docs/archtecture.md)に明記。[1][6]
- 個室/分離: 占有テーブルの状態遷移図（空→予約→占有→解放）、遮音/遮蔽の配置・層設計、終了時の強制解放タイムアウトを設計に追加。[2][1]
- Quest最適化/CI: プラットフォーム別Importerプリセット、テクスチャ上限・Crunch既定、ワールド容量・ドローコール・ライト数の上限と自動検査を[`devenv.md`](docs/devenv.md)に追加。[9][8]