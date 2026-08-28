using System;
using System.Collections.Generic;
using Game.GameEngine.Ecs;
using GameECS;
using UniRx;
using UnityEngine;

namespace SampleProject
{
    public sealed class UnitSelectionPresenter : IDisposable
    {
        private const string IndicatorPoolId = "Selection Indicator";
        private const int Segments = 32;
        private const float Width = 0.08f;

        private readonly Dictionary<EntityHandle, GameObject> _indicators = new();
        private readonly CompositeDisposable _subscriptions = new();
        private readonly IUnitSelectionService _selection;
        private readonly IGameObjectPool _gameObjectPool;
        private readonly EcsWorld _world;
        private Material _material;

        public UnitSelectionPresenter(IUnitSelectionService selection, IGameObjectPool gameObjectPool, EcsWorld world)
        {
            _selection = selection;
            _gameObjectPool = gameObjectPool;
            _world = world;
            _selection.Selected.ObserveAdd().Subscribe(item => AddIndicator(item.Value)).AddTo(_subscriptions);
            _selection.Selected.ObserveRemove().Subscribe(item => RemoveIndicator(item.Value)).AddTo(_subscriptions);
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
            foreach (var indicator in _indicators.Values)
            {
                _gameObjectPool.Release(IndicatorPoolId, indicator);
            }

            _indicators.Clear();
            if (_material != null)
            {
                UnityEngine.Object.Destroy(_material);
            }
        }

        private void AddIndicator(EntityHandle entity)
        {
            if (_indicators.ContainsKey(entity) || !_world.IsEntityExists(entity) || !_world.HasComponent<TransformComponent>(entity.Id))
            {
                return;
            }

            ref var transformComponent = ref _world.GetComponent<TransformComponent>(entity.Id);
            var indicator = _gameObjectPool.Get(IndicatorPoolId, CreateIndicator, transformComponent.Value);
            indicator.transform.localPosition = new Vector3(0, 0.05f, 0);
            var line = indicator.GetComponent<LineRenderer>();

            var radius = Mathf.Max(0.65f, transformComponent.Radius);
            for (var i = 0; i < Segments; i++)
            {
                var angle = i * Mathf.PI * 2 / Segments;
                line.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius));
            }

            _indicators.Add(entity, indicator);
        }

        private void RemoveIndicator(EntityHandle entity)
        {
            if (!_indicators.Remove(entity, out var indicator))
            {
                return;
            }

            _gameObjectPool.Release(IndicatorPoolId, indicator);
        }

        private GameObject CreateIndicator()
        {
            var indicator = new GameObject(IndicatorPoolId);
            var line = indicator.AddComponent<LineRenderer>();
            line.loop = true;
            line.useWorldSpace = false;
            line.positionCount = Segments;
            line.startWidth = Width;
            line.endWidth = Width;
            line.startColor = Color.green;
            line.endColor = Color.green;
            line.material = GetMaterial();
            return indicator;
        }

        private Material GetMaterial()
        {
            if (_material != null)
            {
                return _material;
            }

            var shader = Shader.Find("Sprites/Default");
            _material = new Material(shader) { name = "Selection Indicator Material" };
            return _material;
        }
    }
}
