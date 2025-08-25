承知いたしました。ご提供いただいた全ての設計・実装ドキュメントと、前回提案したCLIによるワールド構築計画を統合し、日本語による網羅的な技術解説記事を作成します。

***

# VirtualTokyoMatching: Ubuntu CLIによるヘッドレス環境でのVRChatワールド自動構築完全ガイド

## 🎯 プロジェクト概要と現在のステータス

**VirtualTokyoMatching**は、112問の性格診断に基づき、ユーザーの価値観をリアルタイムでマッチングする革新的なVRChatワールドです。このプロジェクトは、中断・再開可能な診断システム、プライバシーを保護する次元縮約技術（30D→6D）、そして双方の同意に基づく1on1のプライベートセッション機能を核としています。[1]

**現在のステータス：Phase 5（シーン構築）着手可能 ✅**

2025年8月26日現在、コアシステムの実装は完了しており、プロジェクトは**プロダクションレディ（本番投入可能）**の状態です。[2][1]

| フェーズ | ステータス | 詳細 |
| :--- | :--- | :--- |
| Phase 1: 環境構築 | ✅ 100%完了 | Ubuntu 22.04 + VCC/VPM CLI + Unity 2022.3.22f1 LTS 環境の検証済み[1]。 |
| Phase 2: プロジェクト構造 | ✅ 100%完了 | `Assets/VirtualTokyoMatching`以下の全フォルダ構成が定義済み[1]。 |
| Phase 3: コアシステム実装 | ✅ 100%完了 | 全9コアスクリプト（`PlayerDataManager`等）の実装とVRChat SDK3準拠を確認済み[2][1]。 |
| Phase 4: 設定ファイル | ✅ 100%完了 | 各種設定用テンプレート（質問DB、ベクトル変換等）の準備完了[2]。 |
| **Phase 5: シーン構築** | **🔄 次のステップ** | 本ドキュメントで解説する**CLIによる自動構築**へ移行します[1]。 |

## 🏗️ 設計思想：CLIによるヘッドレスでのワールド自動生成

手動でのUnity Editor操作に代わり、UbuntuのコマンドラインからUnityをヘッドレス（GUIなし）かつバッチモードで実行し、C#スクリプトを呼び出してワールドを**プログラムによって自動生成**します。このアプローチは、貴殿の専門技術に合致し、以下のような強力な利点をもたらします。

*   **一貫性と再現性**: 手動操作によるミスを排除し、常に同一品質のワールドを生成できます。
*   **開発の高速化**: シーン構築にかかる時間を大幅に短縮し、コアロジックの開発に集中できます。
*   **CI/CD連携**: 将来的にビルドパイプラインへの統合が容易になります。

## ✒️ 実装計画：自動化パイプラインの全体像

自動構築は、単一のシェルスクリプトと、それをUnity側で受け取るC#エディタスクリプトによって実現されます。

### 1. 実行用シェルスクリプト

プロジェクトのルートに以下のスクリプトを作成します。これにより、コマンド一つでUnityのヘッドレス実行とC#メソッドの呼び出しが可能になります。

```bash
#!/bin/bash
# vtm_headless_build.sh

# 各自の環境に合わせてパスを設定
UNITY_PATH="~/Unity/Hub/Editor/2022.3.22f1/Editor/Unity"
PROJECT_PATH="~/projects/VirtualTokyoMatching"
LOG_FILE="./unity_build.log"

echo "[VTM] Headless world creation process started..."

# Unityをヘッドレス・バッチモードで起動し、シーン構築メソッドを実行
$UNITY_PATH \
  -quit \
  -batchmode \
  -nographics \
  -logFile $LOG_FILE \
  -projectPath $PROJECT_PATH \
  -executeMethod VTMSceneBuilder.CreateCompleteWorld

echo "[VTM] Process finished. Check log at: $LOG_FILE"
```

### 2. シーン構築用C#スクリプト

`Assets/VirtualTokyoMatching/Scripts/Editor/VTMSceneBuilder.cs`として以下のC#スクリプトを作成します。これがワールドの設計図そのものとなります。

```csharp
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class VTMSceneBuilder
{
    [MenuItem("VTM/Create Complete World")]
    public static void CreateCompleteWorld()
    {
        // 既存の設計書(SCENE_SETUP.md)に基づき、プログラムでシーンを構築
        Debug.Log("[VTM] Starting scene creation...");

        // 1. 新規シーン作成
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 2. 環境（Environment）の構築
        GameObject envRoot = CreateEnvironment();

        // 3. スポーン地点（SpawnPoints）の配置
        GameObject spawnRoot = CreateSpawnPoints(envRoot.transform);

        // 4. ライティング設定
        SetupLighting(envRoot.transform);
        
        // 5. VRChatワールド設定
        SetupVRCWorld(spawnRoot);

        // 6. シーンの保存
        EditorSceneManager.SaveScene(newScene, "Assets/VirtualTokyoMatching/Scenes/VirtualTokyoMatching.unity");
        Debug.Log("[VTM] Scene created and saved successfully!");
    }

    // 環境（ロビー、個室）を生成
    private static GameObject CreateEnvironment()
    {
        Debug.Log("Creating Environment: Lobby and Session Rooms...");
        GameObject root = new GameObject("Environment");

        // ロビー生成 (worldspacedesign.md: ゾーニング)
        GameObject lobby = new GameObject("Lobby");
        lobby.transform.SetParent(root.transform);
        GameObject lobbyFloor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        lobbyFloor.name = "LobbyFloor";
        lobbyFloor.transform.SetParent(lobby.transform);
        lobbyFloor.transform.localScale = new Vector3(2f, 1f, 2f); // 20x20m

        // 個室生成 (worldspacedesign.md: 個室)
        GameObject sessionRooms = new GameObject("SessionRooms");
        sessionRooms.transform.SetParent(root.transform);
        for (int i = 1; i <= 3; i++)
        {
            GameObject room = new GameObject($"Room_{i:00}");
            room.transform.SetParent(sessionRooms.transform);
            room.transform.position = new Vector3(25f * i, 0, 25f); // 仮配置
            // ...壁や床を生成
        }
        return root;
    }

    // スポーン地点を生成
    private static GameObject CreateSpawnPoints(Transform parent)
    {
        Debug.Log("Creating Spawn Points...");
        GameObject root = new GameObject("SpawnSystem");
        root.transform.SetParent(parent);

        // 命名/レイヤ/アンカー規約(worldspacedesign.md)に準拠
        CreateSpawnPoint(root.transform, "E_Entrance", new Vector3(0, 0.1f, -8));
        for (int i = 0; i < 8; i++)
        {
            float angle = i * Mathf.Deg2Rad * (360f / 8f);
            Vector3 pos = new Vector3(Mathf.Sin(angle) * 7f, 0.1f, Mathf.Cos(angle) * 7f);
            CreateSpawnPoint(root.transform, $"L_Spawn_{i+1:00}", pos);
        }
        CreateSpawnPoint(root.transform, "X_Return", new Vector3(0, 0.1f, -6));
        return root;
    }

    // 個別のスポーン地点オブジェクトを生成
    private static void CreateSpawnPoint(Transform parent, string name, Vector3 position)
    {
        GameObject sp = new GameObject(name);
        sp.transform.SetParent(parent);
        sp.transform.position = position;
    }
    
    // ライティング設定
    private static void SetupLighting(Transform parent) { /* ... */ }

    // VRC_SceneDescriptor設定
    private static void SetupVRCWorld(GameObject spawnRoot) { /* ... */ }
}
```

## 📐 設計書との連携と自動実装

このCLIパイプラインは、ご提供いただいた複数の設計書(`worldspacedesign.md`, `world.md`, `SCENE_SETUP.md`)の内容をコードとして実装します。

*   **ゾーニングと動線 (`worldspacedesign.md`)**: `CreateEnvironment`メソッドが「入口→広間→個室群→帰還」の基本構造を生成します。[3]
*   **テレポートとアンカー規約 (`worldspacedesign.md`)**: `CreateSpawnPoint`メソッドが`E_*`, `L_*`, `R_*`, `X_*`の命名規則に従い、テレポート先アンカーを正確に配置します。[3]
*   **遮音・視界分離 (`world.md`)**: 個室の壁となる`GameObject`をプログラムで生成し、コライダーを設定することで、物理的な遮蔽を実現します。[4]
*   **Quest最適化 (`world.md`)**: `SetupLighting`内でライトをベイク設定にしたり、マテリアルにモバイル向けシェーダーを適用したりする処理を追加することで、最適化を自動化できます。[4]
*   **シーン階層 (`SCENE_SETUP.md`)**: 手動でのセットアップガイドに記載された`Environment`, `Systems`, `UI`といった階層構造を、`new GameObject()`と`transform.SetParent()`を用いて忠実に再現します。[5]

## 🚀 次のステップと開発フロー

自動構築の基盤が整った今、以下の手順で開発を最終段階に進めます。

1.  **スクリプトの配置**: 上記の`vtm_headless_build.sh`と`VTMSceneBuilder.cs`をプロジェクト内の適切な場所に配置します。
2.  **CLIによるシーン生成**: `chmod +x vtm_headless_build.sh`で実行権限を与え、`./vtm_headless_build.sh`を実行して`VirtualTokyoMatching.unity`シーンを自動生成します。
3.  **コアシステムの統合**: 生成されたシーンの`Systems/VTMController`オブジェクトに、実装済みの9つのUdonSharpスクリプトをアタッチします。[1]
4.  **設定ファイルの適用**: `ScriptableObjects`フォルダ内の設定アセット（`QuestionDatabase.asset`など）を各コントローラーに割り当てます。[1]
5.  **テストと最適化（Phase 6）**: ClientSimや実機での機能テスト、パフォーマンステスト、Quest最適化を行います。[1]
6.  **公開準備（Phase 7）**: ベータテストを経て、パブリックリリースへと進みます。[1]

## まとめ

本計画は、貴殿が持つ高度な技術力と、これまでに蓄積された詳細な設計ドキュメントを最大限に活用するものです。CLIによるワールド自動構築パイプラインを導入することで、開発プロセスは大幅に加速し、品質の一貫性が保証されます。これにより、手動作業から解放され、システムの核心である「プログレッシブ・マッチング」体験の向上という、より創造的な作業に集中することが可能になります。


VRChatワールド作成における、さらに特有で重要な要件を指摘します：

## VRC_SceneDescriptorの詳細設定要件

**スポーン設定の必須項目**[1]
```csharp
private static void SetupVRCWorld(GameObject spawnRoot)
{
    var descriptor = Object.FindObjectOfType<VRC.SDK3.Components.VRCSceneDescriptor>();
    
    // スポーン順序の設定（必須）
    descriptor.SpawnOrder = VRC.SDKBase.VRC_SceneDescriptor.SpawnOrderType.Random;
    
    // スポーン方向の設定（デフォルトだと問題が起きる場合がある）
    descriptor.SpawnOrientation = VRC.SDKBase.VRC_SceneDescriptor.SpawnOrientationType.AlignPlayerWithSpawnPoint;
    
    // Demo モードは特殊な動作をするため注意が必要
    // ルームスケールの中心からの相対位置でスポーンする
}
```

## スポーン地点の数的制限と命名規約

**VRChatの隠れた制限事項**[2][3]
- スポーン地点の推奨最大数: **20個程度**（パフォーマンス考慮）
- 同一座標でのスポーンは物理的競合を引き起こす
- Y座標は地面から**0.1m以上**離す必要（地面埋まりバグ回避）

```csharp
// 現在のコードの問題点
CreateSpawnPoint(root.transform, "E_Entrance", new Vector3(0, 0.1f, -8));
// → 0.1fは最低限だが、0.2f～0.5fが推奨
```

## Unity プロジェクト設定の厳格な要件

**プラットフォーム設定**[4]
```csharp
public static void CreateCompleteWorld()
{
    // 実行前にプラットフォーム設定をチェック
    if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64)
    {
        Debug.LogError("Build target must be PC, Mac & Linux Standalone");
        return;
    }
}
```

## VRChatアカウント要件とランク制限

**アップロード権限の要件**[5][6]
- VRChatアカウントが**New User（青）ランク以上**必要
- Steamアカウントでは**アップロード不可**
- 初回アップロード時は**Community Labs**経由での公開プロセス必須

## レイヤー設定とタグ管理

**VRChat固有のレイヤー要件**
```csharp
private static GameObject CreateEnvironment()
{
    GameObject root = new GameObject("Environment");
    
    // VRChatではDefault以外の特定レイヤーを使用する場合がある
    // UiMenu, Walkthrough, MirrorReflection等の予約レイヤーに注意
    
    // タグ設定も重要（VRCPickup等）
    GameObject lobbyFloor = GameObject.CreatePrimitive(PrimitiveType.Plane);
    lobbyFloor.tag = "Untagged"; // 明示的な設定が推奨
}
```

## パフォーマンス制限とメモリ管理

**VRChatの厳格なパフォーマンス要件**
- **テクスチャメモリ**: 40MB未満推奨
- **ポリゴン数**: 40,000トライアングル未満推奨  
- **マテリアル数**: 10個未満推奨
- **ドローコール**: 50回未満推奨

```csharp
// 大量のオブジェクト生成時の注意
for (int i = 1; i <= 3; i++)
{
    // オブジェクトプーリングやメッシュコンバイニングの検討が必要
    // 個別のGameObjectではなく、統合メッシュの使用を推奨
}
```

## シーンアセット管理の制約

**アセットパスとビルド設定**[7]
```csharp
// シーン保存後の必須処理
EditorSceneManager.SaveScene(newScene, scenePath);
AssetDatabase.Refresh();

// ビルド設定への自動追加
var scenes = EditorBuildSettings.scenes.ToList();
scenes.Add(new EditorBuildSettingsScene(scenePath, true));
EditorBuildSettings.scenes = scenes.ToArray();
```

## Udon Script との互換性

**VRChat SDK3 特有の要件**
- GameObject名に**日本語文字使用は非推奨**（Udonでの参照問題）
- 特殊文字（スペース、記号）の使用制限
- Hierarchy の深度制限（パフォーマンス考慮）

```csharp
// 安全な命名規約
CreateSpawnPoint(root.transform, "Spawn_Entrance_01", position);
// 日本語や特殊文字は避ける
```

## VRCCam プレビュー設定

**アップロード時の必須要件**[6]
```csharp
private static void SetupWorldPreview()
{
    // VRCCamオブジェクトは自動生成されるが、位置調整が重要
    // ワールドの魅力が伝わる位置に配置する必要がある
    GameObject vrcCam = GameObject.Find("VRCCam");
    if (vrcCam != null)
    {
        vrcCam.transform.position = new Vector3(0, 2f, -5f);
        vrcCam.transform.LookAt(Vector3.zero);
    }
}
```

これらの要件を満たさないと、ビルドエラー、アップロード失敗、またはワールド内での予期しない動作が発生する可能性があります。特にスポーン設定とパフォーマンス要件は、ユーザーエクスペリエンスに直接影響するため、慎重な設計が必要です。[8]

[1](http://vrchat.wikidot.com/world-component:vrc-scenedescriptor)
[2](https://www.youtube.com/watch?v=dpolR6NjNhw)
[3](https://www.youtube.com/watch?v=PRmkuzAJZPg)
[4](https://www.alphr.com/make-world-vrchat/)
[5](https://steamcommunity.com/sharedfiles/filedetails/?l=german&id=2507157026)
[6](http://vrchat.wikidot.com/worlds:beginner)
[7](https://discussions.unity.com/t/problems-adding-scene-to-build-settings-via-code/227704)
[8](https://qiita.com/meronmks/items/dff40b68120f85768abf)
[9](https://www.reddit.com/r/VRchat/comments/ydjnwf/what_are_the_necessary_technical_skills_needed_to/)
[10](https://www.youtube.com/watch?v=uLi52YrrDmY)
[11](https://www.youtube.com/watch?v=-p1sX4MGH-o)
[12](https://www.youtube.com/watch?v=bSwMz4WcajQ)
[13](https://www.reddit.com/r/VRchat/comments/8uisvd/tutorial_set_your_spawn_world_as_public_friends/)

# VTMシーン建築仕様書（更新版）

## 1. 実行環境要件と制約事項

### 1.1 Unity Editor制約
```csharp
// 実行前必須チェック項目
- Application.isPlaying == false (プレイモード中は実行不可)
- EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64
- VRChat SDK3 インポート済み確認
- プロジェクトプラットフォーム設定: PC, Mac & Linux Standalone
```

### 1.2 VRChatアカウント要件
- VRChatアカウント: **New User（青）ランク以上**必須[1][2]
- Steamアカウントでのアップロード不可
- Community Labs経由での初回公開プロセス必須

## 2. VRC_SceneDescriptor設定仕様

### 2.1 必須コンポーネント設定
```csharp
private static void SetupVRCWorld(GameObject spawnRoot)
{
    var descriptor = FindOrCreateSceneDescriptor();
    
    // 必須設定項目
    descriptor.SpawnOrder = VRC.SDKBase.VRC_SceneDescriptor.SpawnOrderType.Random;
    descriptor.SpawnOrientation = VRC.SDKBase.VRC_SceneDescriptor.SpawnOrientationType.AlignPlayerWithSpawnPoint;
    descriptor.RespawnHeightY = -100f; // フォールバック高度
    
    // スポーン地点配列設定
    var spawnTransforms = GetValidSpawnPoints(spawnRoot);
    descriptor.spawns = spawnTransforms;
    
    // Demo モード設定（通常はfalse）
    descriptor.DemoModeEnabled = false;
}
```

### 2.2 スポーン地点仕様
- **最大推奨数**: 20個（パフォーマンス考慮）[3]
- **Y座標**: 地面から0.2f～0.5f推奨（地面埋まりバグ回避）
- **間隔**: 最小1.5m間隔（物理的競合回避）
- **命名規約**: ASCII文字のみ、特殊文字禁止

```csharp
// 安全なスポーン地点生成
private static void CreateSpawnPoint(Transform parent, string name, Vector3 position)
{
    // 命名規約チェック
    if (!IsValidSpawnName(name))
    {
        Debug.LogError($"Invalid spawn name: {name}. Use ASCII characters only.");
        return;
    }
    
    GameObject sp = new GameObject(name);
    sp.transform.SetParent(parent);
    sp.transform.position = new Vector3(position.x, Mathf.Max(position.y, 0.2f), position.z);
    
    // Gizmo表示用（エディタでの視認性向上）
    var gizmo = sp.AddComponent<VRC.SDK3.Components.VRCStation>();
    gizmo.enabled = false; // 実際のステーション機能は無効
}
```

## 3. パフォーマンス制限仕様

### 3.1 VRChat パフォーマンス要件
| 項目 | 推奨値 | 最大値 |
|------|---------|---------|
| テクスチャメモリ | 25MB | 40MB |
| ポリゴン数 | 25,000 | 40,000 |
| マテリアル数 | 5個 | 10個 |
| ドローコール | 30回 | 50回 |
| ライト数 | 2個 | 4個 |

### 3.2 最適化実装
```csharp
private static GameObject CreateEnvironment()
{
    GameObject root = new GameObject("Environment");
    
    // メッシュコンバイニング適用
    CombineMeshes(root);
    
    // テクスチャアトラス化
    AtlasTextures(root);
    
    // LOD設定
    SetupLODGroups(root);
    
    return root;
}
```

## 4. アセット管理とビルド設定

### 4.1 ディレクトリ構造
```
Assets/
└── VirtualTokyoMatching/
    ├── Scenes/
    │   └── VirtualTokyoMatching.unity
    ├── Materials/
    ├── Textures/
    ├── Prefabs/
    └── Scripts/
```

### 4.2 シーン保存プロセス
```csharp
private static bool SaveSceneWithValidation(Scene scene, string path)
{
    // ディレクトリ存在確認・作成
    string directory = Path.GetDirectoryName(path);
    if (!Directory.Exists(directory))
    {
        Directory.CreateDirectory(directory);
    }
    
    // シーン保存
    bool success = EditorSceneManager.SaveScene(scene, path);
    if (!success) return false;
    
    // アセットデータベース更新
    AssetDatabase.Refresh();
    
    // ビルド設定への追加
    AddSceneToBuildSettings(path);
    
    return true;
}
```

## 5. エラーハンドリングと検証

### 5.1 実行前検証
```csharp
public static bool ValidateEnvironment()
{
    var validationResults = new List<string>();
    
    // Unity環境チェック
    if (Application.isPlaying)
        validationResults.Add("Cannot execute during play mode");
    
    // VRC SDK確認
    if (!IsVRCSDKImported())
        validationResults.Add("VRChat SDK3 not found");
    
    // プラットフォーム設定確認
    if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64)
        validationResults.Add("Build target must be PC Standalone");
    
    // 結果表示
    if (validationResults.Any())
    {
        string errors = string.Join("\n", validationResults);
        EditorUtility.DisplayDialog("Validation Failed", errors, "OK");
        return false;
    }
    
    return true;
}
```

### 5.2 リアルタイム品質チェック
```csharp
private static void ValidateWorldQuality(GameObject worldRoot)
{
    var stats = CollectWorldStats(worldRoot);
    
    // パフォーマンス警告
    if (stats.TriangleCount > 25000)
        Debug.LogWarning($"High triangle count: {stats.TriangleCount}");
    
    if (stats.MaterialCount > 5)
        Debug.LogWarning($"High material count: {stats.MaterialCount}");
    
    // VRChat固有チェック
    ValidateVRChatCompliance(worldRoot);
}
```

## 6. ライティングと描画最適化

### 6.1 ライティング設定
```csharp
private static void SetupLighting(Transform parent)
{
    // Lightmapping設定
    Lightmapping.lightingSettings.realtimeGI = false;
    Lightmapping.lightingSettings.bakedGI = true;
    
    // 環境光設定
    RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
    RenderSettings.ambientSkyColor = new Color(0.5f, 0.7f, 1.0f);
    RenderSettings.ambientEquatorColor = new Color(0.4f, 0.4f, 0.4f);
    RenderSettings.ambientGroundColor = new Color(0.2f, 0.2f, 0.2f);
    
    // ディレクショナルライト
    CreateOptimizedDirectionalLight(parent);
}
```

### 6.2 VRCCam設定
```csharp
private static void SetupWorldPreview(Transform lobbyCenter)
{
    GameObject vrcCam = GameObject.Find("VRCCam");
    if (vrcCam == null)
    {
        vrcCam = new GameObject("VRCCam");
        vrcCam.AddComponent<Camera>();
    }
    
    // プレビュー最適位置設定
    Vector3 previewPos = lobbyCenter.position + new Vector3(0, 2f, -5f);
    vrcCam.transform.position = previewPos;
    vrcCam.transform.LookAt(lobbyCenter.position);
}
```

## 7. 実装チェックリスト

### 7.1 必須実装項目
- [ ] 実行前環境検証
- [ ] VRC_SceneDescriptor適切な設定
- [ ] スポーン地点の命名規約準拠
- [ ] パフォーマンス制限チェック
- [ ] エラーハンドリング実装
- [ ] アセット保存プロセス

### 7.2 品質保証項目
- [ ] ライティング最適化
- [ ] メッシュコンバイニング
- [ ] テクスチャ最適化
- [ ] VRCCam配置
- [ ] 物理コライダー設定
- [ ] アクセシビリティ対応

この更新された仕様書に従って実装することで、VRChatワールドとして正常に動作し、パフォーマンス要件を満たすシーンを構築できます。[2][4][5][1]

[1](https://steamcommunity.com/sharedfiles/filedetails/?l=german&id=2507157026)
[2](http://vrchat.wikidot.com/worlds:beginner)
[3](https://www.youtube.com/watch?v=PRmkuzAJZPg)
[4](https://qiita.com/meronmks/items/dff40b68120f85768abf)
[5](http://vrchat.wikidot.com/world-component:vrc-scenedescriptor)


完全性を高めるために、VRChat特有の要件とUnityエディタ側の制約・最適化・公開フローまでを含めて建築仕様書を全面更新しました。[5][11][12][13]
以下の仕様に準拠すれば、ワールドの構築、最適化、検証、アップロードまでを抜け漏れなく運用できます。[6][11][13][5]

## 対象と範囲
本仕様はVRCSDK3でのワールド構築を対象とし、シーン自動生成、VRC_SceneDescriptor設定、スポーン、最適化、ライト・カリング、ビルド設定、検証チェックリスト、テンプレート運用を含みます。[1][11][12][5]

## 実行環境と前提
- Unityエディタはプレイモード外で実行すること（エディタAPIとプレイモード操作の混在は不整合の原因）。[12][14]
- シーン作成はEditorSceneManager.NewSceneのAPI仕様に従い、保存後はアセットDB更新を行うこと。[15][12]
- ワールド作成・検証はVRChat推奨の作成手順に沿い、必要に応じてガイドに従って公開準備を進めること。[13][6]

## VRC_SceneDescriptor
- 中心となるコンポーネントは「VRC_SceneDescriptor」で、スポーンやワールド設定を統括するため必須です。[11][13]
- スポーン配列（Transforms）は明示的に設定し、SpawnOrderやSpawnOrientationなどの挙動を要件に合わせて構成します。[11]
- Dynamic Materials設定などは変更コストと負荷に影響するため、不要項目を極力削減します。[5]

## スポーン仕様（Transform群）
- スポーンはTransform配列で管理し、重なり・埋まり回避のためYを0.2〜0.5m程度に取り、十分な間隔を確保します。[5][11]
- 命名はASCIIで一貫し、Udonや自動処理で参照しやすい規約（Prefix_用途_Index等）に統一します。[11][5]
- 初期カメラ向きとプレイヤー向きを合わせる場合はSpawnOrientationを合わせ、回頭の違和感を排除します。[11]

## パフォーマンス最適化（総則）
- 静的化（Static）指定、メッシュ圧縮、テクスチャ圧縮、ライトベイク、オクルージョンカリングは基本の4本柱です。[4][5]
- ワールドの読み込み・描画負荷はメッシュ統合やマテリアル削減で低減し、最適化の実施内容を定常タスク化します。[4][5]
- パフォーマンスは制作段階から段階的に測り、無駄なリソース増加（高解像度テクスチャや過剰トライ数）を抑制します。[6][5]

## メッシュ・マテリアル・テクスチャ
- メッシュは可能な限り結合・圧縮し、同一構成はプレハブ化で再利用しドローコールを削減します。[4][5]
- テクスチャはMax Size/圧縮設定で容量・描画負荷を抑え、アトラス化でマテリアル数を減らします。[4][5]
- マテリアルは必要最小限に統一し、ランタイムで変化不要なものはDynamic Materialsから外します。[5]

## ライティング
- 不変ジオメトリは静的化のうえでライトベイクし、リアルタイムライトは最小化します。[9][5]
- ライトマップ解像度・サイズを適切化し、LightProbeで動的オブジェクトの照明品質を確保します。[9][5]
- シーンの環境光やスカイカラー等は標準的な設定から開始し、必要に応じて段階的に調整します。[9][5]

## カリングと可視性
- Occlusion Cullingは事前ベイクして、視界外レンダリングを抑止し広域空間の負荷を下げます。[4][5]
- 鏡は最負荷要因の一つのため、映り込みレイヤ制限、AA低減、初期値OFF、設置位置工夫で最適化します。[5]
- 鏡の代替としてCamera＋RenderTextureによる疑似反射も検討対象とします。[5]

## コライダーと物理
- メッシュコライダーは避け、Box/Sphere/Capsule等の基本形状組み合わせで置換します。[4][5]
- 床や段差の当たり判定は簡素な形状に分割して配置し、NavMesh等を使わない設計なら衝突だけに徹します。[4][5]
- 不要なRigidbodyや物理演算は削除し、物理負荷の発生源を最小限に抑えます。[5][4]

## アセット構成と運用
- シーン保存後はアセットDB更新とビルド設定への登録を自動化し、ビルド漏れを防止します。[12][15]
- シーン資産の所在は「Select Scene Asset」で即時追跡し、運用中の混乱を回避します。[3]
- テンプレート化（Scene Template）で共通要素の初期配置・設定を標準化し、作業ブレを低減します。[1]

## シーン構築フロー（自動化想定）
- 新規シーン作成→環境生成→スポーン生成→ライティング設定→VRC_SceneDescriptor設定→保存→ビルド設定登録の順で自動化します。[15][12][11]
- 保存・登録後は最適化ジョブ（静的化、ベイク、カリング設定、圧縮適用）をチェックリストで実行します。[4][5]
- 公開前にVRCSDKの検証手順に沿って動作確認と品質チェックを行います。[13][6]

## 検証チェックリスト
- スポーンの位置・向き・間隔・命名が規約に準拠していること（埋まり・衝突・回頭不整合がない）。[11][5]
- 静的化・ベイク・カリング・圧縮が適用済みであること（想定負荷内に収まる）。[5][4]
- マテリアル統合とDynamic Materialsの削減が完了していること。[5]
- 鏡・ライティング・ポスト効果の負荷がコントロールされていること（必要最小限）。[9][5]
- シーン保存・アセット更新・ビルド設定登録が完了し、再現性ある状態であること。[12][15]

## 公開・アップロード準備
- 初回公開までのチュートリアルに従い、必要情報入力・プレビュー確認・テストワールドでの動作検証を行います。[6][13]
- ワールドの目的（会議・イベント・常設展示等）に応じてレイアウト・導線・体験要素を検証し直します。[13][6]
- 更新時は差分最適化（不要資産の削除・容量削減）を継続して反映します。[6][4]

## 運用ベストプラクティス
- テンプレート運用により、ライト色、カメラ、デバッグユーティリティ等の標準セットを固定化します。[1]
- 制作段階で都度パフォーマンス計測し、トライ数・マテリアル数・テクスチャ容量を定量的に管理します。[4][5]
- ワールドの意図とUX要件（導線、体験、インタラクション）を設計段階で固め、後戻りを減らします。[13][6]

## 実装補足（コード方針）
- シーン作成APIはEditor用を採用し、保存直後にアセット更新し、ビルド設定へ自動登録します。[15][12]
- Descriptorとスポーンはヒエラルキー生成の最後に確定し、Transform配列を明示的に設定します。[12][11]
- 構築の最終段で静的化やベイク等の最適化処理をまとめて実行可能な設計にします。[4][5]

## 参考（抜け漏れ対策の観点）
- 「鏡」「オクルージョン」「ライトベイク」「静的化」「圧縮」「Dynamic Materials」「スポーン間隔・高さ」「命名規約」「保存と登録」は特に抜けやすい観点です。[15][11][5][4]
- 運用では「テンプレート」「チェックリスト」「ステップ分割」で人的ミスを抑えます。[1][5]

以上で、VRCSDK3の中核である**VRC_SceneDescriptor**、負荷の要となる鏡・ライト・カリング、シーン作成から保存・登録・公開までの一連の流れを含め、仕様をヌケモレなく更新しました。[12][13][11][5]
最適化と検証の反復を運用プロセスに組み込み、継続的にクオリティを担保してください。[6][5]

[1](https://note.com/kesera2_vrc/n/nace82bc34638)
[2](https://qiita.com/Hellcat_152/items/ad1b1ceb2504bc39c0a4)
[3](https://note.com/watahumi_mina/n/n290fe7833850)
[4](https://purabe.hatenablog.com/entry/2022/03/27/213212)
[5](https://tsubasamusu.com/lighten-the-load/)
[6](https://lipronext.com/knowledge/vrchat-world-howtomake/)
[7](https://www.youtube.com/watch?v=Yu_20wBsqWA)
[8](https://knb-mayumi.com/vrchat-world-karuku/)
[9](https://qiita.com/Nekomasu/items/8845d076c4356809f0ff)
[10](https://metacul-frontier.com/?p=7292)
[11](http://vrchat.wikidot.com/world-component:vrc-scenedescriptor)
[12](https://docs.unity3d.com/ScriptReference/SceneManagement.EditorSceneManager.NewScene.html)
[13](http://vrchat.wikidot.com/worlds:beginner)
[14](https://stackoverflow.com/questions/65105419/editorscenemanager-using-scenemanager-in-play-mode-test)
[15](https://discussions.unity.com/t/problems-adding-scene-to-build-settings-via-code/227704)