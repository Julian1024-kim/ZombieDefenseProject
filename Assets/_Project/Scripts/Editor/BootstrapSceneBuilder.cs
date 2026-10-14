// 메뉴 ZD/Build Bootstrap Scene: 시작 화면 씬을 코드로 만든다.
// 처음 화면(닉네임, 모드 버튼 2개), 방 목록 화면(방 줄, 방 만들기, 방 코드로 참가, 뒤로), 방 만들기 창, 비밀번호 창.
// 시작 화면 전체(카메라, 이벤트 시스템, 캔버스)는 Menu 오브젝트 하나 아래에 두어 게임 중에 한 번에 끈다.

using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ZD.Bootstrap;
using ZD.Presentation.Menu;

namespace ZD.Editor
{
    internal static class BootstrapSceneBuilder
    {
        public const string SCENE_PATH = "Assets/_Project/Scenes/Bootstrap.unity";

        private const int ROOM_ROW_COUNT = 8;
        private const float ROOM_ROW_HEIGHT = 66f;

        private static readonly Color BackgroundColor = new(0.06f, 0.07f, 0.08f);
        private static readonly Color ButtonColor = new(0.18f, 0.2f, 0.17f);
        private static readonly Color RowColor = new(0.12f, 0.13f, 0.12f);
        private static readonly Color InputColor = new(0.1f, 0.1f, 0.1f);
        private static readonly Color DimColor = new(0f, 0f, 0f, 0.75f);
        private static readonly Color DialogColor = new(0.13f, 0.14f, 0.13f);
        private static readonly Color AccentColor = new(0.95f, 0.75f, 0.2f);
        private static readonly Color PlaceholderColor = new(1f, 1f, 1f, 0.35f);

        [MenuItem("ZD/Build Bootstrap Scene")]
        public static void Build()
        {
            FontSetup.Apply();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var menuRoot = new GameObject("Menu");
            CreateCamera(menuRoot.transform);
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystem.transform.SetParent(menuRoot.transform, false);
            Canvas canvas = CreateCanvas(menuRoot.transform);

            var scopeObject = new GameObject("RootLifetimeScope");
            scopeObject.AddComponent<RootLifetimeScope>();
            CreateMainScreen(canvas.transform, scopeObject, menuRoot, eventSystem);
            CreateLobbyScreen(canvas.transform, scopeObject);
            CreateRoomDialog(canvas.transform, scopeObject);
            CreatePasswordDialog(canvas.transform, scopeObject);

            EditorSceneManager.SaveScene(scene, SCENE_PATH);
        }

        private static void CreateMainScreen(Transform canvas, GameObject viewHost, GameObject menuRoot, GameObject eventSystem)
        {
            CanvasGroup panel = CreateScreen(canvas, "MainScreen");
            CreateText(panel.transform, "Title", new Vector2(0f, 300f), new Vector2(1200f, 140f), "ZOMBIE DEFENSE", 96, AccentColor);
            CreateText(panel.transform, "NicknameLabel", new Vector2(-300f, 170f), new Vector2(220f, 60f), "NICKNAME", 30, Color.white);
            TMP_InputField nickname = CreateInput(panel.transform, "NicknameInput", new Vector2(40f, 170f), new Vector2(440f, 60f), "Player", false);
            CreateText(panel.transform, "Subtitle", new Vector2(0f, 80f), new Vector2(1200f, 60f), "SELECT A MODE", 32, Color.white);
            Button baseDefense = CreateModeButton(panel.transform, "BaseDefenseButton", new Vector2(-260f, -100f), "BASE DEFENSE", "Protect the generator.\nLose if it is destroyed or everyone is down.");
            Button survival = CreateModeButton(panel.transform, "SurvivalButton", new Vector2(260f, -100f), "SURVIVAL", "Survive endless waves.\nLose if everyone is down.");
            TMP_Text status = CreateText(panel.transform, "Status", new Vector2(0f, -320f), new Vector2(1200f, 60f), string.Empty, 30, Color.white);

            ModeSelectView view = viewHost.AddComponent<ModeSelectView>();
            MvpAssetFactory.SetObjectField(view, "_menuRoot", menuRoot);
            MvpAssetFactory.SetObjectField(view, "_eventSystem", eventSystem);
            MvpAssetFactory.SetObjectField(view, "_panel", panel);
            MvpAssetFactory.SetObjectField(view, "_nicknameInput", nickname);
            MvpAssetFactory.SetObjectField(view, "_baseDefenseButton", baseDefense);
            MvpAssetFactory.SetObjectField(view, "_survivalButton", survival);
            MvpAssetFactory.SetObjectField(view, "_statusText", status);
        }

        private static void CreateLobbyScreen(Transform canvas, GameObject viewHost)
        {
            CanvasGroup panel = CreateScreen(canvas, "LobbyScreen");
            TMP_Text title = CreateText(panel.transform, "Title", new Vector2(0f, 430f), new Vector2(1200f, 90f), "ROOMS", 60, AccentColor);

            var rowRoots = new Object[ROOM_ROW_COUNT];
            var rowNames = new Object[ROOM_ROW_COUNT];
            var rowInfos = new Object[ROOM_ROW_COUNT];
            var rowButtons = new Object[ROOM_ROW_COUNT];
            for (int rowIndex = 0; rowIndex < ROOM_ROW_COUNT; rowIndex++)
            {
                float y = 330f - rowIndex * (ROOM_ROW_HEIGHT + 8f);
                GameObject row = CreateBox(panel.transform, $"Room_{rowIndex}", new Vector2(0f, y), new Vector2(1100f, ROOM_ROW_HEIGHT), RowColor);
                TMP_Text name = CreateText(row.transform, "Name", new Vector2(-260f, 0f), new Vector2(520f, ROOM_ROW_HEIGHT), string.Empty, 30, Color.white);
                name.alignment = TextAlignmentOptions.MidlineLeft;
                TMP_Text info = CreateText(row.transform, "Info", new Vector2(220f, 0f), new Vector2(420f, ROOM_ROW_HEIGHT), string.Empty, 24, new Color(1f, 1f, 1f, 0.7f));
                info.alignment = TextAlignmentOptions.MidlineLeft;
                Button join = CreateButton(row.transform, "Join", new Vector2(480f, 0f), new Vector2(120f, 48f), "JOIN", 26);
                row.SetActive(false);
                rowRoots[rowIndex] = row;
                rowNames[rowIndex] = name;
                rowInfos[rowIndex] = info;
                rowButtons[rowIndex] = join;
            }

            TMP_Text status = CreateText(panel.transform, "Status", new Vector2(0f, -280f), new Vector2(1200f, 50f), string.Empty, 28, AccentColor);
            Button create = CreateButton(panel.transform, "CreateRoom", new Vector2(-420f, -370f), new Vector2(260f, 64f), "CREATE ROOM", 28);
            CreateText(panel.transform, "CodeLabel", new Vector2(-150f, -370f), new Vector2(140f, 64f), "CODE", 26, Color.white);
            TMP_InputField code = CreateInput(panel.transform, "CodeInput", new Vector2(0f, -370f), new Vector2(180f, 64f), "ABC123", false);
            TMP_InputField codePassword = CreateInput(panel.transform, "CodePasswordInput", new Vector2(220f, -370f), new Vector2(240f, 64f), "PASSWORD", true);
            Button joinCode = CreateButton(panel.transform, "JoinCode", new Vector2(430f, -370f), new Vector2(160f, 64f), "JOIN", 28);
            Button back = CreateButton(panel.transform, "Back", new Vector2(-800f, 440f), new Vector2(180f, 60f), "BACK", 28);

            LobbyView view = viewHost.AddComponent<LobbyView>();
            MvpAssetFactory.SetObjectField(view, "_panel", panel);
            MvpAssetFactory.SetObjectField(view, "_titleText", title);
            MvpAssetFactory.SetObjectField(view, "_statusText", status);
            MvpAssetFactory.SetObjectArray(view, "_rowRoots", rowRoots);
            MvpAssetFactory.SetObjectArray(view, "_rowNames", rowNames);
            MvpAssetFactory.SetObjectArray(view, "_rowInfos", rowInfos);
            MvpAssetFactory.SetObjectArray(view, "_rowJoinButtons", rowButtons);
            MvpAssetFactory.SetObjectField(view, "_createButton", create);
            MvpAssetFactory.SetObjectField(view, "_backButton", back);
            MvpAssetFactory.SetObjectField(view, "_codeInput", code);
            MvpAssetFactory.SetObjectField(view, "_codePasswordInput", codePassword);
            MvpAssetFactory.SetObjectField(view, "_joinCodeButton", joinCode);
        }

        private static void CreateRoomDialog(Transform canvas, GameObject viewHost)
        {
            CanvasGroup panel = CreateScreen(canvas, "CreateRoomDialog");
            panel.GetComponent<Image>().color = DimColor;
            GameObject box = CreateBox(panel.transform, "Box", Vector2.zero, new Vector2(760f, 520f), DialogColor);
            CreateText(box.transform, "Title", new Vector2(0f, 200f), new Vector2(700f, 70f), "CREATE ROOM", 44, AccentColor);
            CreateText(box.transform, "NameLabel", new Vector2(-250f, 90f), new Vector2(200f, 60f), "ROOM NAME", 26, Color.white);
            TMP_InputField name = CreateInput(box.transform, "NameInput", new Vector2(90f, 90f), new Vector2(460f, 60f), string.Empty, false);
            CreateText(box.transform, "PasswordLabel", new Vector2(-250f, 0f), new Vector2(200f, 60f), "PASSWORD", 26, Color.white);
            TMP_InputField password = CreateInput(box.transform, "PasswordInput", new Vector2(90f, 0f), new Vector2(460f, 60f), "(optional)", true);
            Toggle privateToggle = CreateToggle(box.transform, "PrivateToggle", new Vector2(-140f, -90f), "PRIVATE (JOIN BY CODE ONLY)");
            Button create = CreateButton(box.transform, "Create", new Vector2(-130f, -200f), new Vector2(220f, 64f), "CREATE", 30);
            Button cancel = CreateButton(box.transform, "Cancel", new Vector2(130f, -200f), new Vector2(220f, 64f), "CANCEL", 30);

            CreateRoomView view = viewHost.AddComponent<CreateRoomView>();
            MvpAssetFactory.SetObjectField(view, "_panel", panel);
            MvpAssetFactory.SetObjectField(view, "_nameInput", name);
            MvpAssetFactory.SetObjectField(view, "_passwordInput", password);
            MvpAssetFactory.SetObjectField(view, "_privateToggle", privateToggle);
            MvpAssetFactory.SetObjectField(view, "_createButton", create);
            MvpAssetFactory.SetObjectField(view, "_cancelButton", cancel);
        }

        private static void CreatePasswordDialog(Transform canvas, GameObject viewHost)
        {
            CanvasGroup panel = CreateScreen(canvas, "PasswordDialog");
            panel.GetComponent<Image>().color = DimColor;
            GameObject box = CreateBox(panel.transform, "Box", Vector2.zero, new Vector2(640f, 380f), DialogColor);
            CreateText(box.transform, "Title", new Vector2(0f, 130f), new Vector2(600f, 60f), "PASSWORD REQUIRED", 40, AccentColor);
            TMP_Text room = CreateText(box.transform, "Room", new Vector2(0f, 60f), new Vector2(600f, 50f), string.Empty, 30, Color.white);
            TMP_InputField password = CreateInput(box.transform, "PasswordInput", new Vector2(0f, -20f), new Vector2(420f, 60f), "PASSWORD", true);
            Button join = CreateButton(box.transform, "Join", new Vector2(-120f, -120f), new Vector2(200f, 64f), "JOIN", 30);
            Button cancel = CreateButton(box.transform, "Cancel", new Vector2(120f, -120f), new Vector2(200f, 64f), "CANCEL", 30);

            PasswordPromptView view = viewHost.AddComponent<PasswordPromptView>();
            MvpAssetFactory.SetObjectField(view, "_panel", panel);
            MvpAssetFactory.SetObjectField(view, "_roomText", room);
            MvpAssetFactory.SetObjectField(view, "_passwordInput", password);
            MvpAssetFactory.SetObjectField(view, "_joinButton", join);
            MvpAssetFactory.SetObjectField(view, "_cancelButton", cancel);
        }

        private static void CreateCamera(Transform parent)
        {
            var cameraObject = new GameObject("MenuCamera");
            cameraObject.transform.SetParent(parent, false);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = BackgroundColor;
            camera.cullingMask = 0;
            cameraObject.AddComponent<AudioListener>();
        }

        private static Canvas CreateCanvas(Transform parent)
        {
            var canvasObject = new GameObject("Canvas_Menu", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(parent, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static CanvasGroup CreateScreen(Transform parent, string name)
        {
            var screen = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            screen.transform.SetParent(parent, false);
            var rect = (RectTransform)screen.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image background = screen.GetComponent<Image>();
            background.color = Color.clear;
            CanvasGroup group = screen.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
            return group;
        }

        private static GameObject CreateBox(Transform parent, string name, Vector2 position, Vector2 size, Color color)
        {
            var box = new GameObject(name, typeof(RectTransform), typeof(Image));
            box.transform.SetParent(parent, false);
            var rect = (RectTransform)box.transform;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Image image = box.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return box;
        }

        private static Button CreateModeButton(Transform parent, string name, Vector2 position, string title, string description)
        {
            Button button = CreateButton(parent, name, position, new Vector2(440f, 240f), string.Empty, 1);
            CreateText(button.transform, "Title", new Vector2(0f, 50f), new Vector2(420f, 70f), title, 44, AccentColor);
            CreateText(button.transform, "Description", new Vector2(0f, -40f), new Vector2(400f, 100f), description, 22, Color.white);
            return button;
        }

        private static Button CreateButton(Transform parent, string name, Vector2 position, Vector2 size, string label, float fontSize)
        {
            var buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            var rect = (RectTransform)buttonObject.transform;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            buttonObject.GetComponent<Image>().color = ButtonColor;

            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(1.3f, 1.3f, 1.3f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            colors.disabledColor = new Color(0.45f, 0.45f, 0.45f);
            button.colors = colors;
            if (label.Length > 0)
            {
                CreateText(buttonObject.transform, "Label", Vector2.zero, size, label, fontSize, Color.white);
            }

            return button;
        }

        private static TMP_InputField CreateInput(Transform parent, string name, Vector2 position, Vector2 size, string placeholderText, bool isPassword)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
            root.transform.SetParent(parent, false);
            var rect = (RectTransform)root.transform;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            root.GetComponent<Image>().color = InputColor;

            var area = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
            area.transform.SetParent(root.transform, false);
            var areaRect = (RectTransform)area.transform;
            areaRect.anchorMin = Vector2.zero;
            areaRect.anchorMax = Vector2.one;
            areaRect.offsetMin = new Vector2(14f, 6f);
            areaRect.offsetMax = new Vector2(-14f, -6f);

            TMP_Text placeholder = CreateInputText(area.transform, "Placeholder", placeholderText, PlaceholderColor);
            placeholder.fontStyle = FontStyles.Italic;
            TMP_Text text = CreateInputText(area.transform, "Text", string.Empty, Color.white);

            TMP_InputField input = root.GetComponent<TMP_InputField>();
            input.textViewport = areaRect;
            input.textComponent = text;
            input.placeholder = placeholder;
            input.contentType = isPassword ? TMP_InputField.ContentType.Password : TMP_InputField.ContentType.Standard;
            input.lineType = TMP_InputField.LineType.SingleLine;
            return input;
        }

        private static TMP_Text CreateInputText(Transform parent, string name, string content, Color color)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            var rect = (RectTransform)textObject.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.text = content;
            text.fontSize = 28;
            text.color = color;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.raycastTarget = false;
            return text;
        }

        private static Toggle CreateToggle(Transform parent, string name, Vector2 position, string label)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(Toggle));
            root.transform.SetParent(parent, false);
            var rect = (RectTransform)root.transform;
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(460f, 50f);

            GameObject background = CreateBox(root.transform, "Background", new Vector2(-205f, 0f), new Vector2(40f, 40f), InputColor);
            background.GetComponent<Image>().raycastTarget = true;
            GameObject checkmark = CreateBox(background.transform, "Checkmark", Vector2.zero, new Vector2(26f, 26f), AccentColor);
            TMP_Text text = CreateText(root.transform, "Label", new Vector2(30f, 0f), new Vector2(400f, 50f), label, 24, Color.white);
            text.alignment = TextAlignmentOptions.MidlineLeft;

            Toggle toggle = root.GetComponent<Toggle>();
            toggle.targetGraphic = background.GetComponent<Image>();
            toggle.graphic = checkmark.GetComponent<Image>();
            toggle.isOn = false;
            return toggle;
        }

        private static TMP_Text CreateText(Transform parent, string name, Vector2 position, Vector2 size, string content, float fontSize, Color color)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            var rect = (RectTransform)textObject.transform;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            return text;
        }
    }
}
