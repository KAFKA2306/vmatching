# あそび募集 — Web prototype

`web/` は、Virtual Tokyo Matching の「人を探す」体験を、ユーザー向けの「あそび募集」UIとして検証するための依存ゼロ静的プロトタイプです。

## 目的

- ユーザーが数クリックで「今、一緒に遊べる募集」へ到達できること
- 遊びの種類 / プラットフォーム / 人数 / 時間を主要フィルタにすること
- 募集カード上で人数、開始時刻、言語、主催者を確認できること
- お気に入り、参加予定、募集作成までを1画面内で試せること

## 実装

- `index.html` — 画面構造とアクセシビリティ
- `styles.css` — レスポンシブUIとデザイントークン
- `app.js` — 検索、絞り込み、並べ替え、お気に入り、参加予定、募集作成

外部フレームワーク、CDN、外部画像への依存はありません。

## ローカル確認

```bash
cd web
python -m http.server 8000
```

ブラウザで `http://localhost:8000` を開きます。

## GitHub Pages

公開は `.github/workflows/pages.yml` から GitHub Actions / GitHub Pages を使用します。`web/` を Pages artifact としてアップロードするため、公開用ファイルを repository root や `docs/` へ複製しません。

初回のみ repository の **Settings → Pages → Build and deployment → Source** で **GitHub Actions** を選択して Pages site を有効化します。その後は `main` の `web/` 配下（READMEを除く）が更新されるたびに自動デプロイされます。手動実行も `workflow_dispatch` から可能です。

想定公開URL:

`https://kafka2306.github.io/vmatching/`

## 注意

この画面は BasisVR 公式サービスではありません。BasisVR を題材にした非公式UIコンセプトから派生した、Virtual Tokyo Matching 側のプロトタイプです。
