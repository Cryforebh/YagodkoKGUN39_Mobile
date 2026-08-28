using System;
using Entities;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace SampleProject
{
    public sealed class UnitGroupSpawner : IInitializable
    {
        private readonly IUnitPoolService _unitPool;
        private readonly UnitSpawnSettings _settings;

        public UnitGroupSpawner(IUnitPoolService unitPool, UnitSpawnSettings settings)
        {
            _unitPool = unitPool;
            _settings = settings;
        }

        public void Initialize()
        {
            if (!_settings.SpawnOnStart || _settings.Prefab == null || _settings.Parent == null)
            {
                return;
            }

            var count = Mathf.Max(0, _settings.Count);
            var columns = Mathf.Max(1, _settings.Columns);
            var spacing = Mathf.Max(0.5f, _settings.Spacing);
            _unitPool.Prewarm(_settings.Prefab, count);
            for (var index = 0; index < count; index++)
            {
                var row = index / columns;
                var column = index % columns;
                var offset = new Vector3(column * spacing, 0f, row * spacing);
                _unitPool.Spawn(_settings.Prefab, _settings.Origin + offset, Quaternion.identity, _settings.Parent);
            }
        }
    }

    [Serializable]
    public sealed class UnitSpawnSettings
    {
        [FormerlySerializedAs("spawnOnStart")] public bool SpawnOnStart = true;
        [FormerlySerializedAs("prefab")] public CharacterEntity Prefab;
        [FormerlySerializedAs("parent")] public Transform Parent;
        [FormerlySerializedAs("count"), Min(0)] public int Count = 20;
        [FormerlySerializedAs("columns"), Min(1)] public int Columns = 5;
        [FormerlySerializedAs("spacing"), Min(0.5f)] public float Spacing = 1.2f;
        [FormerlySerializedAs("origin")] public Vector3 Origin = new(1f, 0f, -4f);
    }
}
