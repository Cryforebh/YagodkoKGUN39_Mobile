using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace SampleProject
{
    public sealed class RecruitmentRallyPointPresenter : IInitializable, IDisposable
    {
        private const int CircleSegments = 32;
        private const float MarkerRadius = 0.7f;

        private readonly CompositeDisposable _subscriptions = new();
        private readonly IRecruitmentService _recruitment;
        private GameObject _marker;
        private LineRenderer _line;
        private Material _material;

        public RecruitmentRallyPointPresenter(IRecruitmentService recruitment)
        {
            _recruitment = recruitment;
        }

        public void Initialize()
        {
            CreateMarker();
            _recruitment.SelectedBuilding.Subscribe(_ => Refresh()).AddTo(_subscriptions);
            _recruitment.RallyPointChanged.Subscribe(_ => Refresh()).AddTo(_subscriptions);
            _recruitment.IsSettingRallyPoint.Subscribe(RefreshColor).AddTo(_subscriptions);
            Refresh();
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
            if (_material != null)
            {
                UnityEngine.Object.Destroy(_material);
            }

            if (_marker != null)
            {
                UnityEngine.Object.Destroy(_marker);
            }
        }

        private void CreateMarker()
        {
            _marker = new GameObject("Recruit Rally Point Marker");
            _line = _marker.AddComponent<LineRenderer>();
            _line.useWorldSpace = false;
            _line.loop = true;
            _line.positionCount = CircleSegments;
            _line.startWidth = 0.1f;
            _line.endWidth = 0.1f;
            _line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _line.receiveShadows = false;
            for (var index = 0; index < CircleSegments; index++)
            {
                var angle = index * Mathf.PI * 2f / CircleSegments;
                _line.SetPosition(index, new Vector3(Mathf.Cos(angle) * MarkerRadius, 0.06f, Mathf.Sin(angle) * MarkerRadius));
            }

            var shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                _material = new Material(shader);
                _line.material = _material;
            }

            _marker.SetActive(false);
        }

        private void Refresh()
        {
            var building = _recruitment.SelectedBuilding.Value;
            if (_marker == null || building == null || !building.isActiveAndEnabled || !building.TryGetRallyPoint(out var position))
            {
                if (_marker != null)
                {
                    _marker.SetActive(false);
                }

                return;
            }

            _marker.transform.position = position;
            _marker.SetActive(true);
            RefreshColor(_recruitment.IsSettingRallyPoint.Value);
        }

        private void RefreshColor(bool isSetting)
        {
            if (_line == null)
            {
                return;
            }

            var color = isSetting ? new Color(0.2f, 1f, 0.45f, 1f) : new Color(1f, 0.82f, 0.15f, 0.95f);
            _line.startColor = color;
            _line.endColor = color;
            if (_material != null)
            {
                _material.color = color;
            }
        }
    }
}
