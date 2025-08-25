VCCの機能中核であるVPMのCLI版はLinuxで利用できるためUbuntuでもプロジェクト作成やパッケージ管理は可能です。
Ubuntuでの実用的な方法
推奨はVPM CLIの利用です。.NET 8 SDKを入れた上で「dotnet tool install --global vrchat.vpm.cli」「vpm install templates」で環境を整え、「vpm new <Name> World」や「vpm check project .」「vpm resolve project .」でプロジェクト管理を行います。
LinuxではUnity Hub/Editorの自動検出が弱いため、~/.local/share/VRChatCreatorCompanion/settings.json に pathToUnityHub と pathToUnityExe を手動記入して「vpm check hub」「vpm check unity」で認識させるのが安定です。
最短手順（CLIの例）
手順概要: 1) .NET SDK導入 → 2) VPM導入 → 3) テンプレート導入 → 4) 新規プロジェクト作成 → 5) 必要パッケージ追加 → 6) settings.jsonでUnity Hub/Editorパス確認 → 7) Unityで開いてビルド/テスト。

コマンド例（概要）:

dotnet tool install --global vrchat.vpm.cli

vpm install templates

vpm new VirtualTokyoMatching World -p /path/to/projects

cd /path/to/projects/VirtualTokyoMatching

vpm add package com.vrchat.worlds -p .

vpm add package com.vrchat.udonsharp -p .

vpm add package com.vrchat.clientsim -p .
run unityhub
test scene