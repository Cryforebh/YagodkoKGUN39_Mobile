using System;
using System.Collections.Generic;
using UnityEngine;

namespace SampleProject
{
    public interface IGameObjectPool
    {
        GameObject Get(string poolId, Func<GameObject> factory, Transform parent = null);
        void Release(string poolId, GameObject instance);
        void Prewarm(string poolId, int count, Func<GameObject> factory);
    }

    public sealed class GameObjectPool : IGameObjectPool, IDisposable
    {
        private readonly Dictionary<string, Stack<GameObject>> _pools = new();
        private readonly HashSet<GameObject> _instances = new();
        private readonly HashSet<GameObject> _inactiveInstances = new();
        private Transform _root;
        private bool _rootWasCreated;
        private bool _isDisposed;

        public GameObjectPool()
        {
            GetRoot();
        }

        public GameObject Get(string poolId, Func<GameObject> factory, Transform parent = null)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(GameObjectPool));
            }

            if (string.IsNullOrWhiteSpace(poolId))
            {
                throw new ArgumentException("Pool identifier must not be empty.", nameof(poolId));
            }

            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            var pool = GetPool(poolId);
            GameObject instance = null;
            while (pool.Count > 0 && instance == null)
            {
                instance = pool.Pop();
                _inactiveInstances.Remove(instance);
            }

            if (instance == null)
            {
                instance = factory();
                if (instance == null)
                {
                    throw new InvalidOperationException($"Factory for pool '{poolId}' returned null.");
                }

                _instances.Add(instance);
            }

            instance.transform.SetParent(parent, false);
            instance.SetActive(true);
            return instance;
        }

        public void Release(string poolId, GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            if (_isDisposed)
            {
                UnityEngine.Object.Destroy(instance);
                return;
            }

            if (!_instances.Contains(instance) || !_inactiveInstances.Add(instance))
            {
                return;
            }

            var currentParent = instance.transform.parent;
            var parentIsInactive = currentParent != null && !currentParent.gameObject.activeInHierarchy;
            instance.SetActive(false);
            var root = GetRoot();
            if (root == null)
            {
                return;
            }

            if (!parentIsInactive)
            {
                instance.transform.SetParent(root, false);
            }

            GetPool(poolId).Push(instance);
        }

        public void Prewarm(string poolId, int count, Func<GameObject> factory)
        {
            if (count <= 0)
            {
                return;
            }

            var pool = GetPool(poolId);
            while (pool.Count < count)
            {
                var instance = factory();
                if (instance == null)
                {
                    throw new InvalidOperationException($"Factory for pool '{poolId}' returned null.");
                }

                _instances.Add(instance);
                _inactiveInstances.Add(instance);
                instance.SetActive(false);
                var root = GetRoot();
                if (root == null)
                {
                    UnityEngine.Object.Destroy(instance);
                    return;
                }

                instance.transform.SetParent(root, false);
                pool.Push(instance);
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            foreach (var instance in _instances)
            {
                if (instance != null && !_inactiveInstances.Contains(instance))
                {
                    UnityEngine.Object.Destroy(instance);
                }
            }

            if (_root != null)
            {
                UnityEngine.Object.Destroy(_root.gameObject);
            }

            _pools.Clear();
            _instances.Clear();
            _inactiveInstances.Clear();
        }

        private Stack<GameObject> GetPool(string poolId)
        {
            if (_pools.TryGetValue(poolId, out var pool))
            {
                return pool;
            }

            pool = new Stack<GameObject>();
            _pools.Add(poolId, pool);
            return pool;
        }

        private Transform GetRoot()
        {
            if (_root != null)
            {
                return _root;
            }

            if (_rootWasCreated)
            {
                return null;
            }

            _rootWasCreated = true;
            _root = new GameObject("[GAME OBJECT POOL]").transform;
            return _root;
        }
    }
}
