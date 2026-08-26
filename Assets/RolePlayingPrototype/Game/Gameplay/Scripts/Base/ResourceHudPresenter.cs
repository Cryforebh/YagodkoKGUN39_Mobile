using System;
using Game.GameEngine.Ecs;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SampleProject.Base
{
    public sealed class ResourceHudPresenter : IInitializable, IDisposable
    {
        private readonly CompositeDisposable _subscriptions = new();
        private readonly IResourceStorage _storage;
        private GameObject _root;

        public ResourceHudPresenter(IResourceStorage storage)
        {
            _storage = storage;
        }

        public void Initialize()
        {
            _root = new GameObject("Resource HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = _root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = _root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);

            var panel = new GameObject("Resources", typeof(RectTransform), typeof(VerticalLayoutGroup));
            panel.transform.SetParent(_root.transform, false);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(24, -24);
            rect.sizeDelta = new Vector2(360, 180);
            var layout = panel.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 4;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;

            CreateLabel(panel.transform, ResourceType.Minerals);
            CreateLabel(panel.transform, ResourceType.Wood);
            CreateLabel(panel.transform, ResourceType.Crystals);
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
            if (_root != null)
            {
                UnityEngine.Object.Destroy(_root);
            }
        }

        private void CreateLabel(Transform parent, ResourceType type)
        {
            var labelObject = new GameObject(type.ToString(), typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            labelObject.transform.SetParent(parent, false);
            var label = labelObject.GetComponent<TextMeshProUGUI>();
            label.fontSize = 32;
            label.color = Color.white;
            label.alignment = TextAlignmentOptions.Left;
            labelObject.GetComponent<LayoutElement>().preferredHeight = 48;
            _storage.Get(type).Subscribe(value => label.text = $"{type}: {value}").AddTo(_subscriptions);
        }
    }
}
