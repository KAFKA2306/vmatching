VCCの機能中核であるVPMのCLI版はLinuxで利用できるためUbuntuでもプロジェクト作成やパッケージ管理は可能です。
Ubuntuでの実用的な方法
推奨はVPM CLIの利用です。.NET 8 SDKを入れた上で「dotnet tool install --global vrchat.vpm.cli」「vpm install templates」で環境を整え、「vpm new <Name> World」や「vpm check project .」「vpm resolve project .」でプロジェクト管理を行います。
LinuxではUnity Hub/Editorの自動検出が弱いため、~/.local/share/VRChatCreatorCompanion/settings.json に pathToUnityHub と pathToUnityExe を手動記入して「vpm check hub」「vpm check unity」で認識させるのが安定です。
run unityhub
test scene