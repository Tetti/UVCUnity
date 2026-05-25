#if UNITY_EDITOR
using System.IO;
using Tetti.UVCUnity.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Tetti.UVCUnity.Editor
{
    public static class UvcDemoSceneCreator
    {
        private const string ScenePath = "Assets/Scenes/UvcCameraDemo.unity";

        [MenuItem("Tools/UVC Unity/Create Demo Scene")]
        public static void CreateDemoScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var canvas = CreateCanvas();
            CreateEventSystem();

            var root = CreateRectTransform("LayoutRoot", canvas.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var rootLayout = root.gameObject.AddComponent<HorizontalLayoutGroup>();
            rootLayout.padding = new RectOffset(24, 24, 24, 24);
            rootLayout.spacing = 24f;
            rootLayout.childControlWidth = true;
            rootLayout.childControlHeight = true;
            rootLayout.childForceExpandWidth = true;
            rootLayout.childForceExpandHeight = true;

            var controlsPanel = CreatePanel("ControlsPanel", root, new Color(0.12f, 0.12f, 0.12f, 0.94f));
            var controlsLayout = controlsPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            controlsLayout.spacing = 12f;
            controlsLayout.padding = new RectOffset(18, 18, 18, 18);
            controlsLayout.childControlHeight = true;
            controlsLayout.childControlWidth = true;
            controlsLayout.childForceExpandWidth = true;
            controlsLayout.childForceExpandHeight = false;
            controlsPanel.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            LayoutElementFor(controlsPanel.gameObject, 360f, -1f, 0f, 0f);

            var previewPanel = CreatePanel("PreviewPanel", root, new Color(0.1f, 0.1f, 0.1f, 0.94f));
            var previewLayout = previewPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            previewLayout.spacing = 12f;
            previewLayout.padding = new RectOffset(18, 18, 18, 18);
            previewLayout.childControlWidth = true;
            previewLayout.childControlHeight = true;
            previewLayout.childForceExpandWidth = true;
            previewLayout.childForceExpandHeight = true;
            LayoutElementFor(previewPanel.gameObject, -1f, -1f, 1f, 1f);

            var title = CreateText("Title", controlsPanel, "UVC Android Camera Demo", 24, FontStyle.Bold);
            var refreshButton = CreateButton("RefreshButton", controlsPanel, "Refresh / Scan Devices");
            var cameraLabel = CreateText("CameraLabel", controlsPanel, "Camera", 16, FontStyle.Bold);
            var cameraDropdown = CreateDropdown("CameraDropdown", controlsPanel);
            var resolutionLabel = CreateText("ResolutionLabel", controlsPanel, "Resolution", 16, FontStyle.Bold);
            var resolutionDropdown = CreateDropdown("ResolutionDropdown", controlsPanel);
            var startButton = CreateButton("StartPreviewButton", controlsPanel, "Start Preview");
            var stopButton = CreateButton("StopPreviewButton", controlsPanel, "Stop Preview");
            var statusLabel = CreateText("StatusLabel", controlsPanel, "Status", 16, FontStyle.Bold);
            var statusText = CreateText("StatusText", controlsPanel, "Press Refresh / Scan Devices to enumerate built-in and USB/UVC cameras.", 14, FontStyle.Normal);
            statusText.alignment = TextAnchor.UpperLeft;
            statusText.horizontalOverflow = HorizontalWrapMode.Wrap;
            statusText.verticalOverflow = VerticalWrapMode.Overflow;
            LayoutElementFor(statusText.gameObject, -1f, 120f, 1f, 0f);

            var previewTitle = CreateText("PreviewTitle", previewPanel, "RawImage Preview", 24, FontStyle.Bold);
            var previewRawImage = CreateRawImage("PreviewRawImage", previewPanel);
            LayoutElementFor(previewRawImage.gameObject, -1f, -1f, 1f, 1f);
            previewRawImage.color = Color.white;

            var previewView = previewRawImage.gameObject.AddComponent<RawImagePreviewView>();
            var controllerObject = new GameObject("UvcCameraPanelController");
            controllerObject.transform.SetParent(controlsPanel, false);
            var controller = controllerObject.AddComponent<UvcCameraPanelController>();

            AssignSerializedField(controller, "refreshButton", refreshButton);
            AssignSerializedField(controller, "cameraDropdown", cameraDropdown);
            AssignSerializedField(controller, "resolutionDropdown", resolutionDropdown);
            AssignSerializedField(controller, "startPreviewButton", startButton);
            AssignSerializedField(controller, "stopPreviewButton", stopButton);
            AssignSerializedField(controller, "statusText", statusText);
            AssignSerializedField(controller, "previewView", previewView);

            AssignSerializedField(previewView, "rawImage", previewRawImage);
            previewView.Clear();

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath) ?? "Assets/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            EditorGUIUtility.PingObject(Selection.activeObject);
        }

        private static Canvas CreateCanvas()
        {
            var canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static void CreateEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private static RectTransform CreatePanel(string name, Transform parent, Color color)
        {
            var panel = CreateRectTransform(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var image = panel.gameObject.AddComponent<Image>();
            image.color = color;
            return panel;
        }

        private static Button CreateButton(string name, Transform parent, string label)
        {
            var root = CreateRectTransform(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var image = root.gameObject.AddComponent<Image>();
            image.color = new Color(0.19f, 0.4f, 0.82f, 1f);
            var button = root.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            LayoutElementFor(root.gameObject, -1f, 56f, 1f, 0f);
            var text = CreateText("Label", root, label, 16, FontStyle.Bold);
            text.alignment = TextAnchor.MiddleCenter;
            Stretch(text.rectTransform);
            return button;
        }

        private static Dropdown CreateDropdown(string name, Transform parent)
        {
            var root = CreateRectTransform(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var image = root.gameObject.AddComponent<Image>();
            image.color = new Color(0.94f, 0.94f, 0.94f, 1f);
            LayoutElementFor(root.gameObject, -1f, 56f, 1f, 0f);

            var dropdown = root.gameObject.AddComponent<Dropdown>();
            dropdown.targetGraphic = image;

            var label = CreateText("Label", root, "Option", 16, FontStyle.Normal);
            label.color = Color.black;
            label.alignment = TextAnchor.MiddleLeft;
            StretchWithOffsets(label.rectTransform, new Vector2(16f, 0f), new Vector2(-48f, 0f));

            var arrow = CreateText("Arrow", root, "▼", 16, FontStyle.Bold);
            arrow.color = Color.black;
            arrow.alignment = TextAnchor.MiddleRight;
            StretchWithOffsets(arrow.rectTransform, Vector2.zero, new Vector2(-16f, 0f));

            var templateRoot = CreateRectTransform("Template", root, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 180f));
            templateRoot.gameObject.SetActive(false);
            var templateImage = templateRoot.gameObject.AddComponent<Image>();
            templateImage.color = Color.white;
            templateRoot.gameObject.AddComponent<ScrollRect>();

            var viewport = CreateRectTransform("Viewport", templateRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var maskImage = viewport.gameObject.AddComponent<Image>();
            maskImage.color = Color.white;
            viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;

            var content = CreateRectTransform("Content", viewport, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            var contentLayout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            contentLayout.childControlHeight = true;
            contentLayout.childControlWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.childForceExpandWidth = true;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var item = CreateRectTransform("Item", content, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            var itemBackground = item.gameObject.AddComponent<Toggle>();
            var itemText = CreateText("Item Label", item, "Option", 16, FontStyle.Normal);
            itemText.color = Color.black;
            itemText.alignment = TextAnchor.MiddleLeft;
            Stretch(itemText.rectTransform);
            LayoutElementFor(item.gameObject, -1f, 40f, 1f, 0f);

            var checkmark = CreateText("Item Checkmark", item, "✓", 16, FontStyle.Bold);
            checkmark.color = Color.black;
            checkmark.alignment = TextAnchor.MiddleRight;
            StretchWithOffsets(checkmark.rectTransform, Vector2.zero, new Vector2(-12f, 0f));

            itemBackground.targetGraphic = item.gameObject.AddComponent<Image>();
            itemBackground.graphic = checkmark;
            itemBackground.isOn = true;

            dropdown.template = templateRoot;
            dropdown.captionText = label;
            dropdown.itemText = itemText;

            var scrollRect = templateRoot.GetComponent<ScrollRect>();
            scrollRect.content = content;
            scrollRect.viewport = viewport;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            return dropdown;
        }

        private static RawImage CreateRawImage(string name, Transform parent)
        {
            var preview = CreateRectTransform(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var rawImage = preview.gameObject.AddComponent<RawImage>();
            rawImage.color = new Color(0.18f, 0.18f, 0.18f, 1f);
            preview.gameObject.AddComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            preview.GetComponent<AspectRatioFitter>().aspectRatio = 16f / 9f;
            return rawImage;
        }

        private static Text CreateText(string name, Transform parent, string value, int fontSize, FontStyle fontStyle)
        {
            var textTransform = CreateRectTransform(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var text = textTransform.gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.text = value;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;
            LayoutElementFor(text.gameObject, -1f, -1f, 1f, 0f);
            return text;
        }

        private static RectTransform CreateRectTransform(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            var rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
            rectTransform.localScale = Vector3.one;
            return rectTransform;
        }

        private static void Stretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private static void StretchWithOffsets(RectTransform rectTransform, Vector2 offsetMin, Vector2 offsetMax)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
        }

        private static void LayoutElementFor(GameObject gameObject, float preferredWidth, float preferredHeight, float flexibleWidth, float flexibleHeight)
        {
            var layoutElement = gameObject.GetComponent<LayoutElement>() ?? gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = preferredWidth;
            layoutElement.preferredHeight = preferredHeight;
            layoutElement.flexibleWidth = flexibleWidth;
            layoutElement.flexibleHeight = flexibleHeight;
        }

        private static void AssignSerializedField(Object target, string fieldName, Object value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(fieldName).objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }
    }
}
#endif
