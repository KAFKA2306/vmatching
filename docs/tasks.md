# UIがプレイヤーに追従してしまう原因と修正方法

## 1. 原因
診断開始ボックス（Canvas）が  
- Canvas の **Render Mode** が “World Space” ではなく “Screen Space – Overlay/Camera” になっている  
- あるいは UdonSharp スクリプトで `canvas.transform.SetParent(player)` のように動的にプレイヤーへアタッチしている  

このためプレイヤー位置に固定され、壁などシーン内の座標に留まりません。

## 2. 修正ステップ

### 2-1. Canvas を静的ワールドUIに変更
1. Hierarchy で該当 Canvas を選択。  
2. Inspector → Canvas  
   - Render Mode を **World Space** に変更。  
   - **Event Camera** に MainCamera を指定。  
3. RectTransform をリセットし、壁面前など固定したい位置に移動・回転。  
4. **Scale** は 0.001〜0.01 程度に下げ、実寸1–2m 程度に調整。  

### 2-2. 不要な追従スクリプトを除去
1. UdonSharp や MonoBehaviour 内で `FollowPlayer()` 系処理がないか検索。  
2. 以下のようなコードがあれば削除またはコメントアウト。  
   ```csharp
   // canvas.transform.SetParent(Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).transform);
   ```
3. 代わりに **Scene 固定の Empty 親オブジェクト** を用意し、そこに子付け。

### 2-3. Collider ＋ Raycast 対応
壁 UI をクリックできるよう  
- **Box Collider** を付与  
- Canvas の **Graphic Raycaster** を残す  
- VRChat 用に **VRC_UIInputField** などがある場合は共存確認。

### 2-4. Quest/PC の視認性確認
World Space UI は距離減衰・解像度影響を受けるため、  
- TextMeshPro フォントサイズを大きめに（48pt 以上）  
- キャンバス **Dynamic Pixels Per Unit** を 10–20 に調整  
- 照明が暗い場合は Emissive マテリアルか補助ライトを配置。

## 3. ワールドビルド前のチェックリスト
- [ ] Render Mode が World Space。  
- [ ] Canvas の親はシーン上の固定オブジェクト。  
- [ ] スポーン位置から容易に視認できる位置に配置。  
- [ ] Quest/PC 双方で文字が潰れない。  
- [ ] Collider と Raycaster が機能しクリック可能。  

これで診断開始ボックスが壁面に固定され、プレイヤーの動きに左右されず正しく表示されます。

---

承知いたしました。UIがプレイヤーに追従してしまう問題について、原因と具体的な修正手順をUnity Editorの操作と、貴殿が作成されたC#の自動構築スクリプト(`VTMSceneBuilder.cs`)の両面から解説します。

***

## 1.【原因】UIがプレイヤーに追従する理由

お見受けするに、問題のUI（診断開始ボタンなどを含むパネル）はUnityのCanvasコンポーネントの**Render Mode**が**`Screen Space - Overlay`**に設定されています。

これはUIを常に画面の最前面に表示するモードで、VRではプレイヤーの視界に追従するため、まるで頭に貼り付いているかのように見えます。これを壁に設置するには、Render Modeを**`World Space`**に変更し、3D空間内のオブジェクトとして扱う必要があります。

提供いただいた`VTMSceneBuilder.cs`内の以下の記述が該当箇所です。

**VTMSceneBuilder.cs**
```csharp
static void CreateMainLobbyCanvas(Transform parent)
{
    // ...
    // この設定が原因でUIがプレイヤーに追従します
    canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay; 
    // ...
}
```

## 2.【解決策】UIを壁に固定する具体的な手順

以下の2つの方法のうち、貴殿の開発スタイルに合わせて選択、または両方を実施してください。

### 手順A：Unity Editorでの手動修正（即時確認・デバッグ用）

いますぐ動作を確認したい場合は、以下の手順で手動修正します。

1.  **対象オブジェクトの選択**
    Unityの`Hierarchy`ウィンドウで、`UI` > `MainLobbyCanvas`を選択します。

2.  **Render Modeの変更**
    `Inspector`ウィンドウで`Canvas`コンポーネントを探し、**Render Mode**を`Screen Space - Overlay`から**`World Space`**に変更します。

[1]

3.  **Event Cameraの設定**
    `World Space`に変更すると`Event Camera`という項目が現れます。ここに`Hierarchy`内のメインカメラ（通常は`VRCCam`やプレイヤーオブジェクト配下のカメラ）をドラッグ＆ドロップします。これを行わないとボタンがクリックに反応しません。

4.  **位置とスケールの調整（最重要）**
    `World Space`にすると、Canvasは非常に巨大なオブジェクトとして3D空間に現れます。
    - `Rect Transform`コンポーネントで、**`Scale`**を**`X: 0.005, Y: 0.005, Z: 0.005`** のように極端に小さい値に設定します。
    - その後、**`Position`**と**`Rotation`**を調整し、壁面の適切な位置に配置します。`Width`と`Height`でパネル自体の大きさを調整できます。

5.  **VRChat用の当たり判定を追加**
    - `MainLobbyCanvas`オブジェクトに`VRC Ui Shape`コンポーネントを追加します。これによりVRChatのUI用レーザーが反応するようになります。
    - さらに、`Box Collider`を追加し、`Rect Transform`のサイズに合わせて大きさを調整します。`Is Trigger`にチェックを入れてください。

### 手順B：自動構築スクリプトの恒久修正 (`VTMSceneBuilder.cs`)

貴殿の自動化ワークフローに組み込むための、より恒久的で確実な修正です。`VTMSceneBuilder.cs`内の`CreateMainLobbyCanvas`メソッドを以下のように書き換えてください。

**変更前のコード (`VTMSceneBuilder.cs`)**
```csharp
static void CreateMainLobbyCanvas(Transform parent)
{
    GameObject canvas = new GameObject("MainLobbyCanvas");
    canvas.transform.SetParent(parent);
    Canvas canvasComponent = canvas.AddComponent<Canvas>();
    canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay; // 問題の箇所
    canvasComponent.sortingOrder = 0;
    CanvasScaler scaler = canvas.AddComponent<CanvasScaler>();
    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
    scaler.referenceResolution = new Vector2(1920, 1080);
    // ...
}
```

**変更後のコード (`VTMSceneBuilder.cs`)**
```csharp
static void CreateMainLobbyCanvas(Transform parent)
{
    GameObject canvasGO = new GameObject("MainLobbyCanvas");
    canvasGO.transform.SetParent(parent);

    // --- ここからが変更箇所 ---

    // 1. World Spaceに設定し、壁面に配置
    canvasGO.transform.position = new Vector3(0, 2f, 9.5f); // ロビー北側の壁面あたり
    canvasGO.transform.rotation = Quaternion.Euler(0, 180, 0); // 壁と平行になるよう回転
    canvasGO.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f); // 非常に小さくする

    Canvas canvasComponent = canvasGO.AddComponent<Canvas>();
    canvasComponent.renderMode = RenderMode.WorldSpace; // World Spaceに変更
    
    // RectTransformのサイズを設定 (例: 幅4m x 高さ3m)
    RectTransform rectTransform = canvasGO.GetComponent<RectTransform>();
    rectTransform.sizeDelta = new Vector2(800, 600);

    // VRChatでのインタラクションに必要なコンポーネント
    canvasGO.AddComponent<VRC.SDK3.Components.VRCUiShape>();
    BoxCollider collider = canvasGO.AddComponent<BoxCollider>();
    collider.size = new Vector3(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y, 0.1f);
    collider.isTrigger = true;
    
    // EventCameraは実行時に自動で設定されることが多いが、明示的に設定することも可能
    // canvasComponent.worldCamera = Camera.main;

    // --- ここまでが変更箇所 ---

    canvasGO.AddComponent<CanvasScaler>(); // scaler自体は残してOK
    canvasGO.AddComponent<GraphicRaycaster>();

    // 子要素（ボタンやテキスト）の作成
    GameObject actionPanel = CreateUIPanel(canvasGO.transform, "ActionPanel", new Vector2(700, 500), Vector2.zero);
    CreateUIButton(actionPanel.transform, "StartAssessmentButton", "診断を開始", new Vector2(0, 150), new Vector2(400, 60));
    // ...他のボタンも同様に配置
}
```

この修正版`VTMSceneBuilder.cs`を使って`vtm_headless_build.sh`を実行すれば、次回以降は自動的に壁面に固定されたUIが生成されます。

## まとめ

| 項目 | 修正内容 |
| :--- | :--- |
| **Render Mode** | `Screen Space - Overlay` → **`World Space`** |
| **Position / Rotation** | ワールド内の壁面に合うように設定 |
| **Scale** | `(0.005, 0.005, 0.005)` のような微小な値に設定 |
| **追加コンポーネント** | `VRC Ui Shape`, `Box Collider` (Is Trigger) |
| **Event Camera** | メインカメラを割り当て（必須） |

上記の手順、特に**手順B**のコード修正を適用することで、ご指摘の問題は完全に解決します。

[1](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/dd147834-b123-482b-b132-f4b3d16ab185/VTMSceneSetupTool.cs)
[2](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/a2504250-969d-4787-a20d-c1bbf740fcf6/tasks.md)
[3](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/cd4ac24f-7d16-4291-ae51-a2cb5aa526b7/unitymcp.md)
[4](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/dcb7a649-17bf-42d6-8a25-388c76cbbbd4/publish.md)
[5](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/d5d4f4b1-a643-41c8-9eab-4c50311695bb/steam.md)
[6](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/19106e3d-5b3b-4254-af03-6ec8d0b1582c/IMPLEMENTATION_COMPLETE.md)
[7](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/dff4fa85-28f8-4664-9027-f6973bee93f9/worldcreation.md)
[8](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/0497b8aa-29d5-4d72-b99d-79db3a2f1432/ProjectSetup.md)
[9](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/d6a8d7b6-81b5-4054-8a8a-b0767a92c0d9/vpm.md)
[10](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/32db8e30-c4b8-477c-bdc9-18c15e36a245/TROUBLESHOOTING.md)
[11](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/1f1f78c7-531d-4409-818e-2ae13aa52db6/VTMSceneBuilder.cs)
[12](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/6a742cfb-e03a-48dd-99e1-234d2519ead7/VTMSystemValidator.cs)
[13](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/591e98c3-1786-418d-b3c5-041f1e5fcc3d/SafetyController.cs)
[14](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/bce98d8e-2445-4065-962a-2f5301952cef/SessionRoomManager.cs)
[15](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/bfd888aa-807c-4276-bb54-7f260e7a4d76/RecommenderUI.cs)
[16](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/18522c3e-6d1c-4094-8378-e7c9dbfdc36d/MainUIController.cs)

---

# 床マテリアルを「白一色＋変化しない」状態に固定する方法

開発中に「床が動くたびテクスチャやシェーダー効果が変化して気持ち悪い」という症状は、主に下記 2 パターンで発生します。  
それぞれを完全に無効化し、**単純な白色マテリアル**へ置き換える手順をまとめました。

***

## 1. シェーダー由来の色変化を止める

1. **床オブジェクトを選択**  
   Hierarchy で Plane（例：`LobbyFloor`）や各 Room の `RoomFloor` をすべて選択。

2. **Inspector → Mesh Renderer → Materials**  
   既存マテリアルが `Standard`／`URP/Lit`／カスタムシェーダーなどになっていると、ライティングやカメラ角度で色味が変わります。

3. **新規マテリアルを作成**  
   - Project ビューで右クリック → Create → **Material**  
   - 名前を `Mat_FloorWhite` などに設定。  
   - Shader を **Unlit/Color**（URPなら *URP/Unlit*）へ変更。  
   - Color を **真っ白( #FFFFFF )** にする。  
   - Metallic, Smoothness などのスライダーは 0 に設定（URP/Lit を使う場合）。

4. **床オブジェクトにドラッグ＆ドロップ**  
   作成した `Mat_FloorWhite` をすべての床 MeshRenderer の Element0 に適用。

→ Unlit シェーダーはライティングの影響を一切受けないため、動いても色が変わりません。

***

## 2. ライトとポストプロセスの影響を排除する

白一色にしても「影が動いて濃淡が変わる」「ブルームで色温度が変わる」と感じる場合は次の対策を追加します。

### 2-1. Shadow Casting Off
- 床オブジェクトを選択し、**Mesh Renderer → Lighting**  
  「Cast Shadows」「Receive Shadows」を **両方 Off**。

### 2-2. Lightmap Static を外す
- Inspector 上部の **Static** チェックを外し、ベイク済みライトの色変化を防止。

### 2-3. ポストプロセス Volume の確認
- Project に **Post-processing** パッケージや **URP Volume** がある場合、  
  Bloom/Color Grading で白が変化していないか確認し、不要なら **Disable**。

***

## 3. 既存シーン自動生成コードに反映する

`VTMSceneBuilder.cs` に以下を追記すると、今後生成される床すべてに **Unlit 白マテリアル**を自動設定できます。

```csharp
// 最上部に追加
static Material _whiteMat;
static Material WhiteMat {
    get {
        if (_whiteMat == null) {
            _whiteMat = new Material(Shader.Find("Unlit/Color"));
            _whiteMat.color = Color.white;
            AssetDatabase.CreateAsset(_whiteMat, "Assets/VirtualTokyoMatching/Materials/Mat_FloorWhite.mat");
        }
        return _whiteMat;
    }
}

// 既存 CreateLobby() 内のロビー床生成直後に追記
lobbyFloor.GetComponent<Renderer>().sharedMaterial = WhiteMat;

// 既存 CreateSessionRoom() 内の各 Room 床生成直後に追記
floor.GetComponent<Renderer>().sharedMaterial = WhiteMat;
```

これで **再生成される全シーン**の床が Unlit 白固定になります。

***

## 4. 確認ポイント

- VRChat ワールドでは **Unlit シェーダー**が Quest/PC 両対応で最も軽量。  
- テクスチャ無しの真っ白なので **GPU メモリも 0 MB**。  
- 影を完全に消すと奥行き感がなくなる場合は、`Receive Shadows`だけ On に戻し、`Directional Light` を弱めに調整すると自然に見えます。

***

これで「動くたびに見た目が変わって気持ち悪い」問題は解消され、常にフラットな白い床を維持できます。

[1](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/a2504250-969d-4787-a20d-c1bbf740fcf6/tasks.md)
[2](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/cd4ac24f-7d16-4291-ae51-a2cb5aa526b7/unitymcp.md)
[3](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/dcb7a649-17bf-42d6-8a25-388c76cbbbd4/publish.md)
[4](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/d5d4f4b1-a643-41c8-9eab-4c50311695bb/steam.md)
[5](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/19106e3d-5b3b-4254-af03-6ec8d0b1582c/IMPLEMENTATION_COMPLETE.md)
[6](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/dff4fa85-28f8-4664-9027-f6973bee93f9/worldcreation.md)
[7](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/0497b8aa-29d5-4d72-b99d-79db3a2f1432/ProjectSetup.md)
[8](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/d6a8d7b6-81b5-4054-8a8a-b0767a92c0d9/vpm.md)
[9](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/32db8e30-c4b8-477c-bdc9-18c15e36a245/TROUBLESHOOTING.md)
[10](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/dd147834-b123-482b-b132-f4b3d16ab185/VTMSceneSetupTool.cs)
[11](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/1f1f78c7-531d-4409-818e-2ae13aa52db6/VTMSceneBuilder.cs)
[12](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/6a742cfb-e03a-48dd-99e1-234d2519ead7/VTMSystemValidator.cs)
[13](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/591e98c3-1786-418d-b3c5-041f1e5fcc3d/SafetyController.cs)
[14](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/bce98d8e-2445-4065-962a-2f5301952cef/SessionRoomManager.cs)
[15](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/bfd888aa-807c-4276-bb54-7f260e7a4d76/RecommenderUI.cs)
[16](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/18522c3e-6d1c-4094-8378-e7c9dbfdc36d/MainUIController.cs)


---


# シーン内ボタン配置＆VRChatらしいワールド化タスク

以下のタスクを順番に実行し、**「診断を開始」「診断を続ける」などのメインボタンを壁面に固定**するとともに、VRChatワールドらしい演出・最適化を図ります。

## 1. UIキャンバスのワールドスペース化と壁への固定  
1. Canvasのレンダーモードを **World Space** に変更  
2. **MainLobbyCanvas** を選択し、壁面に向けて配置  
   - Position：壁面中央の高さ1.5m、奥行き0.1m以内  
   - Rotation：壁に対して直立（Z軸を壁法線方向）  
   - Scale：`0.01,0.01,0.01`  
3. CanvasScaler を **Constant Pixel Size** に設定  
4. Raycaster をアタッチし、VRChatでもUI操作を可能に  

## 2. ボタンレイアウトの壁面固定デザイン  
1. **StartAssessmentButton**／**ContinueAssessmentButton**／**PublicSharingButton**／**ViewRecommendationsButton**／**GoToRoomButton** を MainLobbyCanvas 配下のパネルに配置  
2. ボタン群を縦方向に等間隔配置（500px×200px 程度）  
3. 各ボタンのアンカーを Canvas の左中央（X=0%, Y=50%）に設定  
4. ボタンの背景に細い枠（LineRenderer もしくは Image）を追加し、壁面UIとして視認性向上  

## 3. インタラクション誘導の視覚演出  
1. ボタン周囲に **Outline** コンポーネントを追加し、ホバー時に色が変わるよう設定  
2. ボタン押下時に、**Scale** を `1.0→0.9→1.0` にアニメーションさせる  
3. **LoadingScreen** は壁面ではなく中央上方にフルスクリーンで表示  

## 4. VRChatらしい環境演出＆最適化  
1. 壁面UIの背後に **Emissive Material（Unlit）** のパネルを配置し、夜間でも視認性確保  
2. UIパネルには**触れると音が鳴る（AudioSource + Udon）**インタラクションサウンドを設定  
3. Canvas レイヤーを **UI** に切り分け、**VRC_SceneDescriptor.SpawnOrder** を `Random` にして入場時にUIが順次表示  
4. モバイルQuest向けに **ドローコールを 10 回以下** に抑えるため、UIパネルを一枚の TextureAtlas にまとめる  

## 5. タスク実行後の確認項目  
- 壁面UIが常に固定され、動いても揺れない  
- ボタンのホバー／クリック時に視覚・聴覚フィードバックが動作  
- VRCワールドとしてエラーなくアップロード可能  
- Quest/PC両環境で60FPS以上を維持  

以上のタスクを完了すると、「VRChat worldらしいUI配置」として高い没入感と操作性を両立できます。

[1](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/a2504250-969d-4787-a20d-c1bbf740fcf6/tasks.md)
[2](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/cd4ac24f-7d16-4291-ae51-a2cb5aa526b7/unitymcp.md)
[3](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/dcb7a649-17bf-42d6-8a25-388c76cbbbd4/publish.md)
[4](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/d5d4f4b1-a643-41c8-9eab-4c50311695bb/steam.md)
[5](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/19106e3d-5b3b-4254-af03-6ec8d0b1582c/IMPLEMENTATION_COMPLETE.md)
[6](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/dff4fa85-28f8-4664-9027-f6973bee93f9/worldcreation.md)
[7](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/0497b8aa-29d5-4d72-b99d-79db3a2f1432/ProjectSetup.md)
[8](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/d6a8d7b6-81b5-4054-8a8a-b0767a92c0d9/vpm.md)
[9](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/32db8e30-c4b8-477c-bdc9-18c15e36a245/TROUBLESHOOTING.md)
[10](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/dd147834-b123-482b-b132-f4b3d16ab185/VTMSceneSetupTool.cs)
[11](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/1f1f78c7-531d-4409-818e-2ae13aa52db6/VTMSceneBuilder.cs)
[12](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/6a742cfb-e03a-48dd-99e1-234d2519ead7/VTMSystemValidator.cs)
[13](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/591e98c3-1786-418d-b3c5-041f1e5fcc3d/SafetyController.cs)
[14](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/bce98d8e-2445-4065-962a-2f5301952cef/SessionRoomManager.cs)
[15](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/bfd888aa-807c-4276-bb54-7f260e7a4d76/RecommenderUI.cs)
[16](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/52522745/18522c3e-6d1c-4094-8378-e7c9dbfdc36d/MainUIController.cs)