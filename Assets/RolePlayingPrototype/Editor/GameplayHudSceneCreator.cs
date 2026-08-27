using SampleProject.Base;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RolePlayingPrototype.Editor
{
    [InitializeOnLoad]
    public static class GameplayHudSceneCreator
    {
        private const string GameScenePath = "Assets/RolePlayingPrototype/Game/Scenes/Game.unity";
        private static readonly Color PanelColor = new(0.05f, 0.08f, 0.12f, 0.72f);
        private static readonly Color ButtonColor = new(0.12f, 0.18f, 0.24f, 0.95f);

        static GameplayHudSceneCreator()
        {
            EditorApplication.delayCall += CreateForOpenGameScene;
            EditorSceneManager.sceneOpened += OnSceneOpened;
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (scene.path == GameScenePath)
            {
                EditorApplication.delayCall += CreateForOpenGameScene;
            }
        }

        [MenuItem("Tools/RolePlayingPrototype/Create Gameplay HUD")]
        private static void CreateForOpenGameScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().path != GameScenePath || Object.FindObjectOfType<GameplayHudView>(true) != null)
            {
                return;
            }

            var canvasObject = new GameObject("[UI] Gameplay HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(GameplayHudView));
            Undo.RegisterCreatedObjectUndo(canvasObject, "Create Gameplay HUD");
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var safeArea = CreateRect("Safe Area", canvasObject.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            safeArea.gameObject.AddComponent<SafeAreaPanel>();

            var resources = CreateRect("Resources", safeArea, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(32f, -32f), new Vector2(390f, 170f));
            resources.pivot = new Vector2(0f, 1f);
            resources.gameObject.AddComponent<Image>().color = PanelColor;
            var minerals = CreateLabel("Minerals", resources, "Minerals: 0", new Vector2(18f, -14f));
            var wood = CreateLabel("Wood", resources, "Wood: 0", new Vector2(18f, -62f));
            var crystals = CreateLabel("Crystals", resources, "Crystals: 0", new Vector2(18f, -110f));

            var controls = CreateRect("Patrol Controls", safeArea, Vector2.zero, Vector2.zero, new Vector2(32f, 32f), new Vector2(880f, 88f));
            controls.pivot = Vector2.zero;
            var patrolButton = CreateButton("Patrol", controls, "Патруль", Vector2.zero, new Vector2(220f, 80f));
            patrolButton.gameObject.SetActive(false);

            var editButtons = CreateRect("Edit Buttons", controls, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(860f, 80f));
            editButtons.pivot = Vector2.zero;
            editButtons.gameObject.SetActive(false);
            var undoButton = CreateButton("Exit Undo", editButtons, "Выйти", Vector2.zero, new Vector2(220f, 80f));
            var applyButton = CreateButton("Apply", editButtons, "Применить", new Vector2(230f, 0f), new Vector2(260f, 80f));
            var clearButton = CreateButton("Clear All", editButtons, "Удалить все", new Vector2(500f, 0f), new Vector2(300f, 80f));

            canvasObject.GetComponent<GameplayHudView>().Setup(minerals, wood, crystals, patrolButton, editButtons.gameObject, undoButton, applyButton, clearButton);
            EnsureEventSystem();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Selection.activeGameObject = canvasObject;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>(true) != null)
            {
                return;
            }

            var eventSystem = new GameObject("Event System", typeof(EventSystem));
            Undo.RegisterCreatedObjectUndo(eventSystem, "Create Event System");
            eventSystem.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        private static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            var rect = gameObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        private static TextMeshProUGUI CreateLabel(string name, Transform parent, string text, Vector2 position)
        {
            var rect = CreateRect(name, parent, Vector2.up, Vector2.up, position, new Vector2(350f, 44f));
            rect.pivot = Vector2.up;
            var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = 30f;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.color = Color.white;
            label.raycastTarget = false;
            return label;
        }

        private static Button CreateButton(string name, Transform parent, string text, Vector2 position, Vector2 size)
        {
            var rect = CreateRect(name, parent, Vector2.zero, Vector2.zero, position, size);
            rect.pivot = Vector2.zero;
            rect.gameObject.AddComponent<Image>().color = ButtonColor;
            var button = rect.gameObject.AddComponent<Button>();
            var labelRect = CreateRect("Label", rect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var label = labelRect.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = 30f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            label.raycastTarget = false;
            return button;
        }
    }
}
