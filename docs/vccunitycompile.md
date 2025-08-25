結論: エラーは「vpm add」に必要なサブコマンド不足と、VPMの設定ファイル settings.json の破損、およびUnityプロジェクト外で実行したことが主因です。settings.json を再生成し、Unityプロジェクト配下で「vpm add package」を使うか、テンプレートから「vpm new」で新規作成してから追加してください。以後は添付ガイドのとおりにAssets構成とScriptableObject作成、シーン配線、テストへ進みます。[1][2][3]

## 何が起きているか
- vpmの正しい形式は「vpm add package <ID>」で、サブコマンド省略により「Required command was not provided」となっていました。[3]
- さらに settings.json が壊れており、「Please fix or delete your settings file … Unexpected character … Path 'unityEditors'」でVPM内部のRepo/設定が読み込めず、NullReferenceExceptionが発生しています。[3]
- 「Project version not found」「Can't load project manifest ./Packages/manifest.json」は、現在のカレントディレクトリがUnityプロジェクト（Packages/manifest.jsonがある）ではなかった可能性が高い状況です。[3]

## 速攻対処コマンド
- 1) 破損した設定をバックアップして再生成  
  ```
  mv ~/.local/share/VRChatCreatorCompanion/settings.json ~/.local/share/VRChatCreatorCompanion/settings.json.bak
  vpm install templates
  vpm list repos
  ```
  これでVPMテンプレートと既定のレポジトリ定義が入り直し、以降のコマンドが安定します。[3]
- 2) プロジェクトをテンプレートからCLI新規作成（推奨）  
  ```
  vpm new VirtualTokyoMatching World -p ~/projects
  cd ~/projects/VirtualTokyoMatching
  vpm add package com.vrchat.udonsharp -p .
  vpm add package com.vrchat.clientsim -p .
  ```
  「vpm new」でVRChat WorldsテンプレートのUnityプロジェクトを作り、UdonSharp/ClientSimをID指定で追加します（ID例は公式ドキュメントの例示どおり）。[3]
- 3) 既存Unityプロジェクトに追加する場合  
  ```
  cd /path/to/YourUnityProject   # Packages/manifest.json が存在するルート
  vpm check project .
  vpm add package com.vrchat.worlds -p .
  vpm add package com.vrchat.udonsharp -p .
  vpm add package com.vrchat.clientsim -p .
  ```
  「-p .」を付け、プロジェクトルートで実行します（com.vrchat.worlds/udonsharp/clientsimは公式/コミュニティの代表的パッケージID）。[3]

## Linux/WSLでの注意
- VPMはWindowsが正式対応で、Linux/WSLは「未検証扱い」なため、Unity Hub/Editorの自動検出が失敗することがあります。[3]
- その際は「vpm open settingsFolder」で設定フォルダを開き、settings.json の pathToUnityHub などを手動で指定、または「vpm check hub」「vpm check unity」「vpm list unity」で検出と保存を試します（WSLでWindows側のHub/Editorを使う場合はWindows側パスを参照する設計が前提）。[3]

## 追加後にやること（最短ルート）
- Assets/VirtualTokyoMatching 配下に Scripts/Prefabs/Resources/Scenes などの構成を用意し、添付の手順どおりに配置します。[2][1]
- Resources に4種のScriptableObject（QuestionDatabase / VectorConfiguration / SummaryTemplates / PerformanceSettings）を作成し、テンプレート記載の最低限データで開始します。[4][1]
- シーンにSystemsとUIを配置し、Inspectorで依存関係とイベント配列（onDataLoadedTargets / onQuestionAnsweredTargets / onVectorUpdatedTargets など）を配線して、Build & Testで起動確認します。[1][2]