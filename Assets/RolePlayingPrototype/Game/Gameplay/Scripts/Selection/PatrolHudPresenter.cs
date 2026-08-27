using System;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using SampleProject.Base;

namespace SampleProject
{
    public sealed class PatrolHudPresenter : IInitializable, IDisposable
    {
        private readonly List<GameObject> _markers = new();
        private readonly CompositeDisposable _subscriptions = new();
        private readonly IPatrolRouteEditor _routeEditor;
        private readonly IUnitSelectionService _selection;
        private readonly GameplayHudView _view;
        private TextMeshProUGUI _undoLabel;
        private GameObject _routeLineObject;
        private LineRenderer _routeLine;
        private Material _routeMaterial;
        private Material _markerMaterial;
        private bool _isDisposed;

        public PatrolHudPresenter(IPatrolRouteEditor routeEditor, IUnitSelectionService selection, GameplayHudView view)
        {
            _routeEditor = routeEditor;
            _selection = selection;
            _view = view;
        }

        public void Initialize()
        {
            _undoLabel = _view.UndoButton.GetComponentInChildren<TextMeshProUGUI>();
            _view.PatrolButton.onClick.AddListener(_routeEditor.Begin);
            _view.UndoButton.onClick.AddListener(_routeEditor.UndoOrExit);
            _view.ApplyButton.onClick.AddListener(_routeEditor.Apply);
            _view.ClearButton.onClick.AddListener(_routeEditor.ClearPoints);
            CreateRouteLine();
            _routeEditor.Changed += Refresh;
            _selection.Selected.ObserveCountChanged().Subscribe(_ => Refresh()).AddTo(_subscriptions);
            Refresh();
        }

        public void Dispose()
        {
            _isDisposed = true;
            _routeEditor.Changed -= Refresh;
            _subscriptions.Dispose();
            if (_view != null)
            {
                _view.PatrolButton.onClick.RemoveListener(_routeEditor.Begin);
                _view.UndoButton.onClick.RemoveListener(_routeEditor.UndoOrExit);
                _view.ApplyButton.onClick.RemoveListener(_routeEditor.Apply);
                _view.ClearButton.onClick.RemoveListener(_routeEditor.ClearPoints);
            }
            ClearMarkers();
            if (_routeMaterial != null)
            {
                UnityEngine.Object.Destroy(_routeMaterial);
            }

            if (_markerMaterial != null)
            {
                UnityEngine.Object.Destroy(_markerMaterial);
            }

            if (_routeLineObject != null)
            {
                UnityEngine.Object.Destroy(_routeLineObject);
            }
        }

        private void CreateRouteLine()
        {
            _routeLineObject = new GameObject("Patrol Route");
            _routeLine = _routeLineObject.AddComponent<LineRenderer>();
            _routeLine.useWorldSpace = true;
            _routeLine.startWidth = 0.08f;
            _routeLine.endWidth = 0.08f;
            _routeLine.startColor = new Color(0.15f, 0.8f, 1f, 0.9f);
            _routeLine.endColor = new Color(0.15f, 0.8f, 1f, 0.9f);
            var shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                _routeMaterial = new Material(shader);
                _routeLine.material = _routeMaterial;
                _markerMaterial = new Material(shader) { color = new Color(0.15f, 0.8f, 1f, 0.9f) };
            }

            _routeLine.positionCount = 0;
        }

        private void Refresh()
        {
            if (_isDisposed || _view == null)
            {
                return;
            }

            _view.PatrolButton.gameObject.SetActive(!_routeEditor.IsEditing && _selection.Selected.Count > 0);
            _view.EditButtons.SetActive(_routeEditor.IsEditing);
            _view.UndoButton.interactable = true;
            _undoLabel.text = _routeEditor.CanUndoPoint ? "Отмена" : "Выйти";
            _view.ClearButton.interactable = _routeEditor.PointCount > 0;
            RefreshRouteVisualization();
        }

        private void RefreshRouteVisualization()
        {
            ClearMarkers();
            if (_routeLine == null)
            {
                return;
            }

            if (!_routeEditor.IsEditing)
            {
                _routeLine.positionCount = 0;
                return;
            }

            for (var i = 0; i < _routeEditor.Points.Count; i++)
            {
                CreateMarker(_routeEditor.Points[i], i + 1);
            }

            var pointCount = _routeEditor.Points.Count;
            _routeLine.positionCount = pointCount > 1 ? pointCount + 1 : pointCount;
            for (var i = 0; i < pointCount; i++)
            {
                _routeLine.SetPosition(i, _routeEditor.Points[i] + Vector3.up * 0.08f);
            }

            if (pointCount > 1)
            {
                _routeLine.SetPosition(pointCount, _routeEditor.Points[0] + Vector3.up * 0.08f);
            }
        }

        private void CreateMarker(Vector3 position, int number)
        {
            var marker = new GameObject("Patrol Point " + number);
            marker.name = "Patrol Point " + number;
            marker.transform.position = position;

            var cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.name = "Marker";
            cylinder.transform.SetParent(marker.transform, false);
            cylinder.transform.localPosition = Vector3.up * 0.03f;
            cylinder.transform.localScale = new Vector3(0.24f, 0.03f, 0.24f);
            var collider = cylinder.GetComponent<Collider>();
            collider.enabled = false;
            UnityEngine.Object.Destroy(collider);
            if (_markerMaterial != null)
            {
                cylinder.GetComponent<Renderer>().sharedMaterial = _markerMaterial;
            }

            var labelObject = new GameObject("Number", typeof(TextMeshPro));
            labelObject.transform.SetParent(marker.transform, false);
            labelObject.transform.localPosition = new Vector3(0f, 0.08f, 0f);
            labelObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var label = labelObject.GetComponent<TextMeshPro>();
            label.text = number.ToString();
            label.fontSize = 3f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            _markers.Add(marker);
        }

        private void ClearMarkers()
        {
            for (var i = 0; i < _markers.Count; i++)
            {
                if (_markers[i] != null)
                {
                    UnityEngine.Object.Destroy(_markers[i]);
                }
            }

            _markers.Clear();
        }

    }
}
