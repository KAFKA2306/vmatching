はい、承知いたしました。これまでの議論と調査結果をすべて統合し、コードスニペットやコマンド例を豊富に含んだ、完全なガイド記事を作成します。

***

# Unity MCP Pythonサーバー設定 完全ガイド：`pyproject.toml`エラーからの脱出 (2025年版)

## 1. はじめに
Unity MCP (Model Context Protocol) は、ClaudeやCursorといったAIアシスタントとUnity Editorを直接連携させ、自然言語の指示でシーン操作やアセット管理を可能にする画期的なツールです。しかし、その強力な機能の裏で、多くの開発者がPythonサーバーのセットアップ段階、特に`pyproject.toml not found`というエラーでつまずいています。

この記事では、これまでの試行錯誤と調査結果をすべてまとめ、**現在最も確実かつ簡単なセットアップ手順**を、具体的なコマンドやコード設定例と共に網羅的に解説します。このガイドを読めば、環境構築の迷路から抜け出し、すぐにAIとの共同開発を始めることができるでしょう。

## 2. なぜエラーが起きるのか？古い情報との決別
このセットアップで混乱が生じる最大の原因は、**リポジトリの仕様と推奨手順が大きく変更された**ことにあります。[5]

*   **古い情報**: `git clone`したリポジトリ内の`Python/`ディレクトリに移動し、`pip install -e .`を実行するという手動セットアップ手順。この方法は現在では機能しません。[6]
*   **現在の仕様**: Unityパッケージに統合された**`Auto-Setup`機能**が、必要なPythonサーバーをユーザーのローカル環境へ自動的にインストール・設定する方式。[3][5]

現在、公式の`CoplayDev/unity-mcp`リポジトリには、手動インストールを前提とした`Python/`ディレクトリは存在しません。そのため、古い情報に基づいた手動セットアップは必ず失敗します。このガイドでは、現在の公式手順に完全準拠した方法を解説します。[5]

## 3. 前提条件
セットアップを始める前に、お使いの環境に以下のツールがインストールされていることを確認してください。

*   **Unity Hub & Unity Editor**: バージョン 2021.3 LTS 以降[5]
*   **Python**: バージョン 3.12 以降[5]
*   **uv (Pythonパッケージマネージャー)**: 最新のPython環境では標準搭載されていることが多いですが、なければ以下のコマンドでインストールします。[3][5]

ターミナル（WindowsならPowerShell）で以下のコマンドを実行します。

**Windowsの場合:**
```powershell
powershell -ExecutionPolicy ByPass -c "irm https://astral.sh/uv/install.ps1 | iex"
```

**macOS / Linux の場合:**
```bash
curl -LsSf https://astral.sh/uv/install.sh | sh
```
インストール後、ターミナルを再起動してください。

## 4. 最速セットアップ手順：Unityによる自動設定
これが公式推奨の最も簡単で確実な方法です。コマンドラインでの複雑な作業はほとんど必要ありません。

**Step 1: Unityパッケージをインストール**
1.  Unityで対象のプロジェクトを開きます。
2.  メニューバーから `Window > Package Manager` を選択します。
3.  Package Managerウィンドウの左上にある `+` ボタンをクリックし、`Add package from git URL...` を選択します。[5]
4.  以下のURLを正確に入力し、`Add` ボタンをクリックします。
    ```
    https://github.com/CoplayDev/unity-mcp.git?path=/UnityMcpBridge
    ```
    > **ポイント**: URLの末尾にある `?path=/UnityMcpBridge` は、リポジトリの中からUnityパッケージとして必要な部分だけを正しくインストールするための重要な指定です。[5]

**Step 2: Pythonサーバーを自動セットアップ**
1.  パッケージのインストールが完了すると、メニューバーに `Window > MCP for Unity` という項目が追加されます。これをクリックしてウィンドウを開きます。
2.  開いたウィンドウの中央にある `Auto-Setup` ボタンをクリックします。[5]
3.  `Auto-Setup`が実行されると、あなたのPCの適切な場所にPythonサーバーが自動でインストール・設定されます。処理が完了すると、ウィンドウ内の `Python Server Status` が緑色の **`Connected ✓`** に変わります。これでサーバーのセットアップは完了です。[5]

## 5. MCPクライアントとの連携設定
`Auto-Setup`機能の優れた点は、サーバーのセットアップだけでなく、主要なAIクライアント（Claude, Cursor, Visual Studio Codeなど）の設定ファイルも自動で更新してくれることです[5]。

### 手動設定が必要な場合
もし接続がうまくいかない場合や、対応外のクライアントを使用する場合は、設定ファイルを手動で編集する必要があります。

**1. 設定ファイルの場所**
*   **Claude (macOS)**: `~/Library/Application Support/Claude/claude_desktop_config.json`[]
*   **Claude (Windows)**: `%APPDATA%\Claude\claude_desktop_config.json`[5]
*   **VSCode**: ユーザー設定内の `mcp.json` ファイル[5]

**2. 設定スニペットの例**
`Auto-Setup`によってローカルに作成された`UnityMcpServer`のパスを調べ、以下のように設定ファイルに記述します。パスは**ご自身の環境に合わせて**正確に書き換えてください。

#### **VSCode の設定例 (`mcp.json`)**
```json
{
  "servers": {
    "unityMCP": {
      "command": "uv",
      "args": [
        "--directory",
        "C:\\Users\\YOUR_USERNAME\\AppData\\Local\\Programs\\UnityMCP\\UnityMcpServer\\src",
        "run",
        "server.py"
      ],
      "type": "stdio"
    }
  }
}
```

#### **Claude / Cursor の設定例 (`claude_desktop_config.json`)**

**Windowsの場合:**
```json
{
  "mcpServers": {
    "UnityMCP": {
      "command": "uv",
      "args": [
        "run",
        "--directory",
        "C:\\Users\\YOUR_USERNAME\\AppData\\Local\\Programs\\UnityMCP\\UnityMcpServer\\src",
        "server.py"
      ]
    }
  }
}
```

**macOSの場合:**
```json
{
  "mcpServers": {
    "UnityMCP": {
      "command": "uv",
      "args": [
        "run",
        "--directory",
        "/usr/local/bin/UnityMCP/UnityMcpServer/src",
        "server.py"
      ]
    }
  }
}
```

> **重要**: `YOUR_USERNAME`の部分やパスは、あなたの環境に`Auto-Setup`で生成されたディレクトリの**絶対パス**に正確に置き換える必要があります。Windowsではパスの区切り文字`\`を二重（`\\`）にするのを忘れないでください。[5]

## 6. トラブルシューティング
### Q1: `pyproject.toml not found` や `No such file or directory: Python` というエラーが出る
**A1:** あなたは古い手動セットアップ手順を試みています。このガイドの「最速セットアップ手順」に従い、Unityの`Auto-Setup`機能を使用してください。`git clone`して`pip install`や`cd Python`を実行する必要はもうありません。

### Q2: `MCP for Unity`ウィンドウに`uv Not Found`と表示される
**A2:** Pythonパッケージマネージャー`uv`がインストールされていないか、システムのPATHが通っていません。
1.  このガイドの「前提条件」セクションに戻り、`uv`のインストールコマンドを再度実行してください。
2.  それでも表示が消えない場合は、ウィンドウ内に表示される `Choose uv Install Location` ボタンを使い、`uv`の実行ファイル（`uv.exe`など）の場所を直接指定してください。[5]

### Q3: `Python Server Status`が緑色の `Connected ✓` にならない
**A3:** いくつかの原因が考えられます。以下の点を確認してください。
1.  **`Auto-Setup`の再実行**: もう一度`MCP for Unity`ウィンドウの`Auto-Setup`ボタンを押して、処理が正常に完了するか確認します。
2.  **クライアント設定ファイルの確認**: 手動設定セクションを参考に、設定ファイル内のパスが正しいか、特に絶対パスが間違っていないかを確認してください。
3.  **UnityとAIクライアントの再起動**: 両方のアプリケーションを完全に終了し、Unityを先に起動してからAIクライアントを起動してみてください。[5]

## 7. まとめ
Unity MCPのPythonサーバーセットアップは、公式の`Auto-Setup`機能の登場により、劇的に簡単かつ確実になりました。コマンドラインでの複雑な操作や`pyproject.toml`エラーに悩む必要はもうありません。

このガイドで紹介した**「Unity Package Managerでパッケージを追加し、MCP for UnityウィンドウでAuto-Setupボタンを押す」**という手順こそが、現在の最も正しく、そして最も効率的な方法です。環境構築でつまずいていた方も、ぜひこの方法で再挑戦し、AIとの未来的なゲーム開発を体験してください。

[1](https://ai-kenkyujo.com/software/mcp-nyuumon/)
[2](https://note.com/npaka/n/n5a5135ac395a)
[3](https://soft-rime.com/post-23819/)
[4](https://note.com/npaka/n/n8d4ab42ace71)
[5](https://github.com/CoplayDev/unity-mcp)
[6](https://apidog.com/jp/blog/unity-mcp-server-jp/)
[7](https://ai-kenkyujo.com/programming/language/python/mcp-python/)
[8](https://sihikipon-yakujob.com/ai-game-dev-guide-claude-unity-mcp/)
[9](https://qiita.com/taka_yayoi/items/f3f3e034d712f204fba5)
[10](https://www.docswell.com/s/SiroSuzume/ZW4QWJ-2025-07-26-224936)