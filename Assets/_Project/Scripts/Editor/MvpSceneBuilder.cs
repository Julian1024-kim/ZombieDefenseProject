// 메뉴 ZD/Build MVP Scene: MVP 데이터, 프리팹, Bootstrap(시작 화면) 씬과 Game 씬을 코드로 다시 만든다.
// 환경은 기본 도형이고, 캐릭터·무기·방어막은 받은 모델이 있으면 그것을 쓴다.

using System.Collections.Generic;
using TMPro;
using Unity.AI.Navigation;
using Unity.AI.Navigation.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using ZD.Bootstrap;
using ZD.Gameplay.Building;
using ZD.Network.Session;
using ZD.Presentation.Building;
using ZD.Presentation.Cameras;
using ZD.Presentation.Diagnostics;
using ZD.Presentation.Hud;
using ZD.Presentation.Input;
using ZD.Presentation.Shop;
using ZD.Presentation.Weapons;

namespace ZD.Editor
{
    internal static class MvpSceneBuilder
    {
        private const string SCENE_FOLDER = "Assets/_Project/Scenes";
        private const string SCENE_PATH = SCENE_FOLDER + "/Game.unity";
        private const float ARENA_HALF_SIZE = 30f;
        private const string FUSION_REBUILD_PREFAB_TABLE_MENU = "Tools/Fusion/Rebuild Prefab Table";
        private const string FILL_SPRITE_PATH = "UI/Skin/UISprite.psd";

        // 진행·체력 막대의 채움 이미지에 쓰는 Unity 기본 UI 스프라이트.
        private static Sprite FillSprite => AssetDatabase.GetBuiltinExtraResource<Sprite>(FILL_SPRITE_PATH);

        [MenuItem("ZD/Build MVP Scene")]
        public static void Build()
        {
            // 레이어(Environment, Barricade)가 있어야 아래에서 마스크와 레이어를 정할 수 있다.
            ProjectSettingsSetup.Apply();

            // 시작 화면 씬을 먼저 만든다. Game 씬은 NavMesh 베이크가 끝난 뒤 '지금 열린 씬'으로 다시 저장하므로 마지막에 열려 있어야 한다.
            BootstrapSceneBuilder.Build();

            // NewScene은 사용하지 않는 에셋을 언로드하므로, 씬을 먼저 만들지 않으면 새로 만든 에셋 참조가 null이 된다.
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            MvpAssets assets = MvpAssetFactory.CreateAll();
            CreateLight();
            NavMeshSurface surface = CreateEnvironment();
            SpawnPointSet spawnPoints = CreateSpawnPoints();
            CreateCamera();
            var inputObject = new GameObject("Input");
            inputObject.AddComponent<InputView>();
            inputObject.AddComponent<DevInputBot>();
            new GameObject("Diagnostics").AddComponent<DevPerfProbe>();
            CreateWeaponEffects();
            CreateBuildPreview(assets.BuildCatalog);
            CreateHud();
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            CreateLifetimeScope(assets);

            EnsureFolder(SCENE_FOLDER);
            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            NavMeshAssetManager.instance.StartBakingSurfaces(new Object[] { surface });
            AddSceneToBuildSettings();
            Selection.activeObject = spawnPoints;
            FinishAfterBake(surface);
        }

        [MenuItem("ZD/Save Scene After NavMesh Bake")]
        public static void SaveAfterBake()
        {
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }

        private static void FinishAfterBake(NavMeshSurface surface)
        {
            EditorApplication.CallbackFunction waitForBake = null;
            waitForBake = () =>
            {
                if (surface != null && NavMeshAssetManager.instance.IsSurfaceBaking(surface))
                {
                    return;
                }

                EditorApplication.update -= waitForBake;
                SaveAfterBake();
                EditorApplication.ExecuteMenuItem(FUSION_REBUILD_PREFAB_TABLE_MENU);
                Debug.Log("ZD: 씬 빌드 완료(NavMesh 저장, 프리팹 테이블 갱신).");
            };
            EditorApplication.update += waitForBake;
        }

        private static void CreateLight()
        {
            var lightObject = new GameObject("Directional Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.shadows = LightShadows.Soft;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static NavMeshSurface CreateEnvironment()
        {
            var environment = new GameObject("Environment");
            NavMeshSurface surface = environment.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.Children;
            surface.layerMask = LayerMask.GetMask("Default", ProjectSettingsSetup.ENVIRONMENT_LAYER);

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(environment.transform, false);
            ground.transform.localScale = new Vector3(ARENA_HALF_SIZE / 5f, 1f, ARENA_HALF_SIZE / 5f);
            ground.GetComponent<Renderer>().sharedMaterial = MvpAssetFactory.GroundMaterial;

            float wallLength = ARENA_HALF_SIZE * 2f;
            CreateBlock(environment.transform, "Wall_N", new Vector3(0f, 1.5f, ARENA_HALF_SIZE), new Vector3(wallLength, 3f, 1f));
            CreateBlock(environment.transform, "Wall_S", new Vector3(0f, 1.5f, -ARENA_HALF_SIZE), new Vector3(wallLength, 3f, 1f));
            CreateBlock(environment.transform, "Wall_E", new Vector3(ARENA_HALF_SIZE, 1.5f, 0f), new Vector3(1f, 3f, wallLength));
            CreateBlock(environment.transform, "Wall_W", new Vector3(-ARENA_HALF_SIZE, 1.5f, 0f), new Vector3(1f, 3f, wallLength));

            CreateBlock(environment.transform, "Cover_1", new Vector3(6f, 0.75f, 4f), new Vector3(3f, 1.5f, 1f));
            CreateBlock(environment.transform, "Cover_2", new Vector3(-7f, 0.75f, -3f), new Vector3(1f, 1.5f, 4f));
            CreateBlock(environment.transform, "Cover_3", new Vector3(2f, 0.75f, -9f), new Vector3(4f, 1.5f, 1f));
            CreateBlock(environment.transform, "Cover_4", new Vector3(-4f, 1.25f, 10f), new Vector3(2f, 2.5f, 2f));
            CreateBlock(environment.transform, "Cover_5", new Vector3(12f, 1.25f, -12f), new Vector3(2f, 2.5f, 2f));
            CreateSupplyBoxes(environment.transform);

            int environmentLayer = LayerMask.NameToLayer(ProjectSettingsSetup.ENVIRONMENT_LAYER);
            environment.layer = environmentLayer;
            GameObjectUtility.SetStaticEditorFlags(environment, StaticEditorFlags.BatchingStatic);
            foreach (Transform child in environment.transform)
            {
                child.gameObject.layer = environmentLayer;
                GameObjectUtility.SetStaticEditorFlags(child.gameObject, StaticEditorFlags.BatchingStatic);
            }

            return surface;
        }

        private static void CreateSupplyBoxes(Transform environment)
        {
            Vector3[] positions = { new(5f, 0.5f, -5f), new(-5f, 0.5f, 5f) };
            var boxes = new List<Transform>();
            for (int boxIndex = 0; boxIndex < positions.Length; boxIndex++)
            {
                GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
                box.name = $"SupplyBox_{boxIndex}";
                box.transform.SetParent(environment, false);
                box.transform.localPosition = positions[boxIndex];
                box.transform.localScale = new Vector3(1.2f, 1f, 0.8f);
                box.GetComponent<Renderer>().sharedMaterial = MvpAssetFactory.CrateMaterial;
                boxes.Add(box.transform);
            }

            SupplyBoxSet set = new GameObject("SupplyBoxes").AddComponent<SupplyBoxSet>();
            var serialized = new SerializedObject(set);
            AssignArray(serialized.FindProperty("_boxes"), boxes);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateBlock(Transform parent, string name, Vector3 position, Vector3 size)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.SetParent(parent, false);
            block.transform.localPosition = position;
            block.transform.localScale = size;
            block.GetComponent<Renderer>().sharedMaterial = MvpAssetFactory.WallMaterial;
        }

        private static SpawnPointSet CreateSpawnPoints()
        {
            var root = new GameObject("SpawnPoints");
            SpawnPointSet set = root.AddComponent<SpawnPointSet>();

            var players = new List<Transform>();
            // 거점(3×3m)이 가운데에 서므로 플레이어는 그 둘레에 선다. 보급품 상자(대각선)와도 겹치지 않는다.
            Vector3[] playerPositions = { new(0f, 0f, -4.5f), new(4.5f, 0f, 0f), new(0f, 0f, 4.5f), new(-4.5f, 0f, 0f) };
            for (int pointIndex = 0; pointIndex < playerPositions.Length; pointIndex++)
            {
                players.Add(CreatePoint(root.transform, $"Player_{pointIndex}", playerPositions[pointIndex]));
            }

            var zombies = new List<Transform>();
            float edge = ARENA_HALF_SIZE - 3f;
            Vector3[] zombiePositions =
            {
                new(-edge, 0f, edge), new(0f, 0f, edge), new(edge, 0f, edge),
                new(-edge, 0f, -edge), new(0f, 0f, -edge), new(edge, 0f, -edge),
            };
            for (int pointIndex = 0; pointIndex < zombiePositions.Length; pointIndex++)
            {
                zombies.Add(CreatePoint(root.transform, $"Zombie_{pointIndex}", zombiePositions[pointIndex]));
            }

            Transform basePoint = CreatePoint(root.transform, "Base", Vector3.zero);
            basePoint.rotation = Quaternion.identity;

            var serialized = new SerializedObject(set);
            AssignArray(serialized.FindProperty("_playerSpawnPoints"), players);
            AssignArray(serialized.FindProperty("_zombieSpawnPoints"), zombies);
            serialized.FindProperty("_baseSpawnPoint").objectReferenceValue = basePoint;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return set;
        }

        private static Transform CreatePoint(Transform parent, string name, Vector3 position)
        {
            var point = new GameObject(name);
            point.transform.SetParent(parent, false);
            point.transform.localPosition = position;
            point.transform.rotation = Quaternion.LookRotation(new Vector3(-position.x, 0f, -position.z).normalized + Vector3.forward * 0.0001f);
            return point.transform;
        }

        private static void CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 65f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 150f;
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<CameraView>();
            cameraObject.transform.SetPositionAndRotation(new Vector3(0f, 6f, -8f), Quaternion.Euler(30f, 0f, 0f));
        }

        private static void CreateWeaponEffects()
        {
            WeaponEffectsView effects = new GameObject("WeaponEffects").AddComponent<WeaponEffectsView>();
            SetFields(effects, ("_tracerMaterial", MvpAssetFactory.TracerMaterial), ("_sparkMaterial", MvpAssetFactory.SparkMaterial));
            var serialized = new SerializedObject(effects);
            serialized.FindProperty("_visualHitMask").intValue = LayerMask.GetMask("Default", "Zombie", ProjectSettingsSetup.ENVIRONMENT_LAYER, ProjectSettingsSetup.BARRICADE_LAYER);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateHud()
        {
            // 갱신 빈도별로 나눠서, 숫자 하나가 바뀌어도 고정된 부분까지 다시 그리지 않게 한다.
            Canvas staticCanvas = CreateCanvas("Canvas_HudStatic", 0);
            var crosshair = new GameObject("Crosshair", typeof(RectTransform), typeof(Image));
            crosshair.transform.SetParent(staticCanvas.transform, false);
            crosshair.GetComponent<Image>().raycastTarget = false;
            ((RectTransform)crosshair.transform).sizeDelta = new Vector2(6f, 6f);

            Canvas dynamicCanvas = CreateCanvas("Canvas_HudDynamic", 1);
            TMP_Text health = CreateText(dynamicCanvas.transform, "Health", new Vector2(0f, 0f), new Vector2(40f, 40f), TextAlignmentOptions.BottomLeft, 36);
            TMP_Text ammo = CreateText(dynamicCanvas.transform, "Ammo", new Vector2(1f, 0f), new Vector2(-40f, 40f), TextAlignmentOptions.BottomRight, 36);
            TMP_Text wave = CreateText(dynamicCanvas.transform, "Wave", new Vector2(0.5f, 1f), new Vector2(0f, -30f), TextAlignmentOptions.Top, 40);
            TMP_Text status = CreateText(dynamicCanvas.transform, "Status", new Vector2(0.5f, 1f), new Vector2(0f, -80f), TextAlignmentOptions.Top, 28);
            TMP_Text downed = CreateText(dynamicCanvas.transform, "Downed", new Vector2(0.5f, 0.5f), new Vector2(0f, 80f), TextAlignmentOptions.Center, 48);
            downed.SetText("DOWNED - WAIT FOR A TEAMMATE");
            downed.color = new Color(1f, 0.35f, 0.3f);
            downed.gameObject.SetActive(false);

            TMP_Text points = CreateText(dynamicCanvas.transform, "Points", new Vector2(0f, 0f), new Vector2(40f, 90f), TextAlignmentOptions.BottomLeft, 30);
            points.color = new Color(1f, 0.85f, 0.35f);
            TMP_Text prompt = CreateText(dynamicCanvas.transform, "Prompt", new Vector2(0.5f, 0.3f), Vector2.zero, TextAlignmentOptions.Center, 30);

            PlayerHudView playerHud = dynamicCanvas.gameObject.AddComponent<PlayerHudView>();
            SetFields(playerHud, ("_healthText", health), ("_pointsText", points), ("_ammoText", ammo), ("_promptText", prompt), ("_downedBanner", downed.gameObject));

            var slotTexts = new TMP_Text[5];
            for (int slotIndex = 0; slotIndex < slotTexts.Length; slotIndex++)
            {
                slotTexts[slotIndex] = CreateText(dynamicCanvas.transform, $"Slot_{slotIndex + 1}", new Vector2(0.5f, 0f), new Vector2(-480f + slotIndex * 240f, 30f), TextAlignmentOptions.Bottom, 24);
                ((RectTransform)slotTexts[slotIndex].transform).sizeDelta = new Vector2(230f, 60f);
            }

            SlotBarView slotBar = dynamicCanvas.gameObject.AddComponent<SlotBarView>();
            SetFields(slotBar,
                ("_primaryText", slotTexts[0]),
                ("_secondaryText", slotTexts[1]),
                ("_meleeText", slotTexts[2]),
                ("_healText", slotTexts[3]),
                ("_throwableText", slotTexts[4]));

            CreateReviveBar(dynamicCanvas.transform);
            CreateBuildHud(dynamicCanvas.transform);
            CreateBaseHealthBar(dynamicCanvas.transform);

            Canvas menuCanvas = CreateCanvas("Canvas_Menu", 2);
            var panel = new GameObject("GameOverPanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            panel.transform.SetParent(menuCanvas.transform, false);
            var panelRect = (RectTransform)panel.transform;
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            Image panelImage = panel.GetComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.7f);
            panelImage.raycastTarget = false;
            // 게임 오버 창: 위에서부터 제목·진 이유, 점수 요약, 플레이어별 표, 조작 안내.
            TMP_Text gameOver = CreateText(panel.transform, "GameOverText", new Vector2(0.5f, 0.5f), new Vector2(0f, 300f), TextAlignmentOptions.Center, 64);
            ((RectTransform)gameOver.transform).sizeDelta = new Vector2(1100f, 160f);
            TMP_Text summary = CreateText(panel.transform, "ResultSummary", new Vector2(0.5f, 0.5f), new Vector2(0f, 130f), TextAlignmentOptions.Center, 34);
            ((RectTransform)summary.transform).sizeDelta = new Vector2(1100f, 150f);
            TMP_Text table = CreateText(panel.transform, "ResultTable", new Vector2(0.5f, 0.5f), new Vector2(0f, -90f), TextAlignmentOptions.TopLeft, 30);
            ((RectTransform)table.transform).sizeDelta = new Vector2(1200f, 230f);
            TMP_Text hint = CreateText(panel.transform, "ResultHint", new Vector2(0.5f, 0.5f), new Vector2(0f, -320f), TextAlignmentOptions.Center, 30);
            // 안내 문구는 호스트인지에 따라 MatchHudView가 채운다.
            ResultView resultView = panel.AddComponent<ResultView>();
            SetFields(resultView, ("_summaryText", summary), ("_tableText", table));

            CreateShopPanel(menuCanvas.transform);

            MatchHudView matchHud = dynamicCanvas.gameObject.AddComponent<MatchHudView>();
            SetFields(matchHud, ("_waveText", wave), ("_statusText", status), ("_gameOverPanel", panel.GetComponent<CanvasGroup>()), ("_gameOverText", gameOver), ("_gameOverHintText", hint));
        }

        private static void CreateReviveBar(Transform canvas)
        {
            var bar = new GameObject("ReviveBar", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            bar.transform.SetParent(canvas, false);
            var barRect = (RectTransform)bar.transform;
            barRect.anchorMin = new Vector2(0.5f, 0.32f);
            barRect.anchorMax = new Vector2(0.5f, 0.32f);
            barRect.sizeDelta = new Vector2(360f, 18f);
            Image background = bar.GetComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.6f);
            background.raycastTarget = false;

            var fillObject = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillObject.transform.SetParent(bar.transform, false);
            var fillRect = (RectTransform)fillObject.transform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(2f, 2f);
            fillRect.offsetMax = new Vector2(-2f, -2f);
            Image fill = fillObject.GetComponent<Image>();
            fill.color = new Color(0.45f, 0.85f, 0.45f);
            // 채움(Filled) 이미지는 스프라이트가 있어야 fillAmount가 먹는다. 없으면 늘 가득 찬 막대로 보인다.
            fill.sprite = FillSprite;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillAmount = 0f;
            fill.raycastTarget = false;

            TMP_Text label = CreateText(bar.transform, "Label", new Vector2(0.5f, 1f), new Vector2(0f, 30f), TextAlignmentOptions.Bottom, 24);
            ((RectTransform)label.transform).sizeDelta = new Vector2(360f, 30f);

            ReviveBarView view = bar.AddComponent<ReviveBarView>();
            SetFields(view, ("_group", bar.GetComponent<CanvasGroup>()), ("_fill", fill), ("_label", label));
        }

        private static void CreateBaseHealthBar(Transform canvas)
        {
            var bar = new GameObject("BaseHealthBar", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            bar.transform.SetParent(canvas, false);
            var barRect = (RectTransform)bar.transform;
            barRect.anchorMin = new Vector2(0.5f, 1f);
            barRect.anchorMax = new Vector2(0.5f, 1f);
            barRect.pivot = new Vector2(0.5f, 1f);
            barRect.anchoredPosition = new Vector2(0f, -170f);
            barRect.sizeDelta = new Vector2(480f, 22f);
            Image background = bar.GetComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.6f);
            background.raycastTarget = false;
            CanvasGroup group = bar.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;

            var fillObject = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillObject.transform.SetParent(bar.transform, false);
            var fillRect = (RectTransform)fillObject.transform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(2f, 2f);
            fillRect.offsetMax = new Vector2(-2f, -2f);
            Image fill = fillObject.GetComponent<Image>();
            fill.color = new Color(0.95f, 0.75f, 0.2f);
            // 채움(Filled) 이미지는 스프라이트가 있어야 fillAmount가 먹는다. 없으면 늘 가득 찬 막대로 보인다.
            fill.sprite = FillSprite;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillAmount = 1f;
            fill.raycastTarget = false;

            TMP_Text label = CreateText(bar.transform, "Label", new Vector2(0.5f, 0f), new Vector2(0f, -28f), TextAlignmentOptions.Top, 22);
            ((RectTransform)label.transform).sizeDelta = new Vector2(480f, 28f);

            BaseHudView view = bar.AddComponent<BaseHudView>();
            SetFields(view, ("_group", group), ("_fill", fill), ("_label", label));
        }

        private static void CreateShopPanel(Transform menuCanvas)
        {
            var panel = new GameObject("ShopPanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            panel.transform.SetParent(menuCanvas, false);
            var panelRect = (RectTransform)panel.transform;
            panelRect.sizeDelta = new Vector2(620f, 640f);
            panel.GetComponent<Image>().color = new Color(0.08f, 0.1f, 0.08f, 0.92f);

            TMP_Text title = CreateText(panel.transform, "Title", new Vector2(0.5f, 1f), new Vector2(0f, -20f), TextAlignmentOptions.Top, 36);
            title.SetText("SUPPLY BOX");
            ((RectTransform)title.transform).sizeDelta = new Vector2(580f, 50f);
            TMP_Text points = CreateText(panel.transform, "Points", new Vector2(0.5f, 1f), new Vector2(0f, -70f), TextAlignmentOptions.Top, 28);
            ((RectTransform)points.transform).sizeDelta = new Vector2(580f, 40f);
            points.color = new Color(1f, 0.85f, 0.35f);
            TMP_Text message = CreateText(panel.transform, "Message", new Vector2(0.5f, 0f), new Vector2(0f, 20f), TextAlignmentOptions.Bottom, 24);
            ((RectTransform)message.transform).sizeDelta = new Vector2(580f, 40f);

            var list = new GameObject("Items", typeof(RectTransform), typeof(VerticalLayoutGroup));
            list.transform.SetParent(panel.transform, false);
            var listRect = (RectTransform)list.transform;
            listRect.anchorMin = new Vector2(0f, 0f);
            listRect.anchorMax = new Vector2(1f, 1f);
            listRect.offsetMin = new Vector2(30f, 70f);
            listRect.offsetMax = new Vector2(-30f, -120f);
            VerticalLayoutGroup layout = list.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childControlHeight = false;
            layout.childForceExpandHeight = false;

            var template = new GameObject("ItemTemplate", typeof(RectTransform), typeof(Image), typeof(Button));
            template.transform.SetParent(list.transform, false);
            ((RectTransform)template.transform).sizeDelta = new Vector2(560f, 56f);
            template.GetComponent<Image>().color = new Color(0.2f, 0.3f, 0.2f, 1f);
            TMP_Text label = CreateText(template.transform, "Label", new Vector2(0.5f, 0.5f), Vector2.zero, TextAlignmentOptions.Center, 26);
            ((RectTransform)label.transform).sizeDelta = new Vector2(540f, 50f);

            ShopView shop = panel.AddComponent<ShopView>();
            SetFields(shop,
                ("_panel", panel.GetComponent<CanvasGroup>()),
                ("_itemTemplate", template.GetComponent<Button>()),
                ("_pointsText", points),
                ("_messageText", message));
        }

        private static Canvas CreateCanvas(string name, int sortingOrder)
        {
            var canvasObject = new GameObject(name, typeof(Canvas), typeof(CanvasScaler));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            // 가로와 세로를 절반씩 반영해 16:10이나 21:9에서도 HUD가 넘치지 않게 한다.
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static void CreateBuildPreview(BuildCatalog catalog)
        {
            var root = new GameObject("BuildPreview");
            var ghosts = new GameObject[catalog.Count];
            for (int itemIndex = 0; itemIndex < catalog.Count; itemIndex++)
            {
                var ghost = new GameObject($"Ghost_{itemIndex + 1}");
                ghost.transform.SetParent(root.transform, false);
                Renderer[] renderers = BarricadeAssetFactory.CreateVisual((BarricadeAssetFactory.Shape)itemIndex, catalog.Get(itemIndex), ghost.transform,
                    BarricadeAssetFactory.GhostValidMaterial, BarricadeAssetFactory.GhostValidMaterial);
                for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
                {
                    renderers[rendererIndex].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }

                ghosts[itemIndex] = ghost;
            }

            BuildPreviewView view = root.AddComponent<BuildPreviewView>();
            var serialized = new SerializedObject(view);
            SerializedProperty array = serialized.FindProperty("_ghosts");
            array.arraySize = ghosts.Length;
            for (int ghostIndex = 0; ghostIndex < ghosts.Length; ghostIndex++)
            {
                array.GetArrayElementAtIndex(ghostIndex).objectReferenceValue = ghosts[ghostIndex];
            }

            serialized.FindProperty("_validMaterial").objectReferenceValue = BarricadeAssetFactory.GhostValidMaterial;
            serialized.FindProperty("_invalidMaterial").objectReferenceValue = BarricadeAssetFactory.GhostInvalidMaterial;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateBuildHud(Transform canvas)
        {
            var panel = new GameObject("BuildHud", typeof(RectTransform), typeof(CanvasGroup));
            panel.transform.SetParent(canvas, false);
            var rect = (RectTransform)panel.transform;
            rect.anchorMin = new Vector2(1f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.anchoredPosition = new Vector2(-40f, 0f);
            rect.sizeDelta = new Vector2(460f, 320f);

            TMP_Text items = CreateText(panel.transform, "Items", new Vector2(1f, 1f), Vector2.zero, TextAlignmentOptions.TopRight, 28);
            ((RectTransform)items.transform).sizeDelta = new Vector2(460f, 240f);
            TMP_Text status = CreateText(panel.transform, "Status", new Vector2(1f, 0f), Vector2.zero, TextAlignmentOptions.BottomRight, 30);
            ((RectTransform)status.transform).sizeDelta = new Vector2(460f, 60f);

            BuildHudView view = panel.AddComponent<BuildHudView>();
            SetFields(view, ("_group", panel.GetComponent<CanvasGroup>()), ("_itemsText", items), ("_statusText", status));
        }

        private static TMP_Text CreateText(Transform parent, string name, Vector2 anchor, Vector2 position, TextAlignmentOptions alignment, float size)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            var rect = (RectTransform)textObject.transform;
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(900f, 120f);
            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.alignment = alignment;
            text.fontSize = size;
            text.raycastTarget = false;
            return text;
        }

        private static void CreateLifetimeScope(MvpAssets assets)
        {
            var scopeObject = new GameObject("GameLifetimeScope");
            GameLifetimeScope scope = scopeObject.AddComponent<GameLifetimeScope>();
            SetFields(scope,
                ("_playerDefinition", assets.PlayerDefinition),
                ("_waveProfile", assets.WaveProfile),
                ("_cameraSettings", assets.CameraSettings),
                ("_prefabCatalog", assets.PrefabCatalog),
                ("_hitQuerySettings", assets.HitQuerySettings),
                ("_weaponCatalog", assets.WeaponCatalog),
                ("_inventorySettings", assets.InventorySettings),
                ("_shopCatalog", assets.ShopCatalog),
                ("_buildCatalog", assets.BuildCatalog));
        }

        private static void SetFields(Object target, params (string field, Object value)[] fields)
        {
            var serialized = new SerializedObject(target);
            for (int fieldIndex = 0; fieldIndex < fields.Length; fieldIndex++)
            {
                serialized.FindProperty(fields[fieldIndex].field).objectReferenceValue = fields[fieldIndex].value;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignArray(SerializedProperty array, List<Transform> values)
        {
            array.arraySize = values.Count;
            for (int valueIndex = 0; valueIndex < values.Count; valueIndex++)
            {
                array.GetArrayElementAtIndex(valueIndex).objectReferenceValue = values[valueIndex];
            }
        }

        private static void AddSceneToBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            for (int sceneIndex = scenes.Count - 1; sceneIndex >= 0; sceneIndex--)
            {
                if (scenes[sceneIndex].path == SCENE_PATH || scenes[sceneIndex].path == BootstrapSceneBuilder.SCENE_PATH)
                {
                    scenes.RemoveAt(sceneIndex);
                }
            }

            scenes.Insert(0, new EditorBuildSettingsScene(BootstrapSceneBuilder.SCENE_PATH, true));
            scenes.Insert(1, new EditorBuildSettingsScene(SCENE_PATH, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Scenes");
            }
        }
    }
}
