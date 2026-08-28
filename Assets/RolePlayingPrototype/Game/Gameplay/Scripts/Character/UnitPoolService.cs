using System;
using System.Collections.Generic;
using Entities;
using UnityEngine;
using Zenject;

namespace SampleProject
{
    public interface IUnitPoolService
    {
        void Prewarm(CharacterEntity prefab, int count);
        CharacterEntity Spawn(CharacterEntity prefab, Vector3 position, Quaternion rotation, Transform parent);
    }

    public sealed class UnitPoolService : IUnitPoolService, IDisposable
    {
        private readonly Dictionary<CharacterEntity, string> _poolIds = new();
        private readonly DiContainer _container;
        private readonly IGameObjectPool _gameObjectPool;
        private readonly Transform _factoryRoot;

        public UnitPoolService(DiContainer container, IGameObjectPool gameObjectPool)
        {
            _container = container;
            _gameObjectPool = gameObjectPool;
            _factoryRoot = new GameObject("[UNIT POOL FACTORY]").transform;
            _factoryRoot.gameObject.SetActive(false);
        }

        public void Prewarm(CharacterEntity prefab, int count)
        {
            if (prefab == null || count <= 0)
            {
                return;
            }

            var poolId = GetPoolId(prefab);
            _gameObjectPool.Prewarm(poolId, count, () => CreateUnit(prefab, poolId));
        }

        public CharacterEntity Spawn(CharacterEntity prefab, Vector3 position, Quaternion rotation, Transform parent)
        {
            if (prefab == null)
            {
                return null;
            }

            var poolId = GetPoolId(prefab);
            var instance = _gameObjectPool.Get(
                poolId,
                () => CreateUnit(prefab, poolId),
                parent,
                gameObject => gameObject.GetComponent<PooledUnitHandle>().Prepare(position, rotation)
            );
            return instance.GetComponent<CharacterEntity>();
        }

        public void Dispose()
        {
            if (_factoryRoot != null)
            {
                UnityEngine.Object.Destroy(_factoryRoot.gameObject);
            }

            _poolIds.Clear();
        }

        private string GetPoolId(CharacterEntity prefab)
        {
            if (_poolIds.TryGetValue(prefab, out var poolId))
            {
                return poolId;
            }

            poolId = "Unit:" + prefab.GetInstanceID();
            _poolIds.Add(prefab, poolId);
            return poolId;
        }

        private GameObject CreateUnit(CharacterEntity prefab, string poolId)
        {
            var instance = _container.InstantiatePrefab(prefab.gameObject, Vector3.zero, Quaternion.identity, _factoryRoot);
            var handle = instance.GetComponent<PooledUnitHandle>();
            if (handle == null)
            {
                handle = instance.AddComponent<PooledUnitHandle>();
            }

            handle.Configure(_gameObjectPool, poolId);
            return instance;
        }
    }
}
