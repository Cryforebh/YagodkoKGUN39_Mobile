using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Entities;
using Game.GameEngine.Ecs;
using UnityEngine;
using Zenject;

namespace SampleProject
{
    public sealed class EnemyWaveSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterEntity _enemyPrefab;
        [SerializeField] private Transform[] _spawnPoints = Array.Empty<Transform>();
        [SerializeField] private Transform _assaultTarget;
        [SerializeField] private Transform _unitsParent;

        [Header("Waves")]
        [SerializeField] private bool _spawnOnStart = true;
        [SerializeField, Min(0f)] private float _initialDelay = 5f;
        [SerializeField, Min(0.1f)] private float _waveInterval = 15f;
        [SerializeField, Min(1)] private int _unitsPerWave = 5;
        [SerializeField, Min(0f)] private float _unitSpawnInterval = 0.2f;
        [SerializeField, Min(1)] private int _maximumAliveUnits = 30;

        [Header("Assault")]
        [SerializeField, Min(0.1f)] private float _arrivalDistance = 1.2f;

        private readonly List<CharacterEntity> _spawnedUnits = new();
        private IUnitPoolService _unitPool;
        private CancellationTokenSource _spawnCancellation;
        private bool _isInjected;
        private bool _hasStarted;
        private int _nextSpawnPoint;

        [Inject]
        private void Construct(IUnitPoolService unitPool)
        {
            _unitPool = unitPool;
            _isInjected = true;
            TryStartSpawning();
        }

        private void Start()
        {
            _hasStarted = true;
            TryStartSpawning();
        }

        private void OnEnable()
        {
            TryStartSpawning();
        }

        private void OnDisable()
        {
            StopSpawning();
        }

        private void OnDestroy()
        {
            StopSpawning();
        }

        private void TryStartSpawning()
        {
            if (!_spawnOnStart || !_hasStarted || !_isInjected || !isActiveAndEnabled ||
                _spawnCancellation != null || _enemyPrefab == null || _assaultTarget == null)
            {
                return;
            }

            _unitPool.Prewarm(_enemyPrefab, Mathf.Min(_unitsPerWave, _maximumAliveUnits));
            _spawnCancellation = new CancellationTokenSource();
            RunSpawnerAsync(_spawnCancellation.Token).Forget(exception => Debug.LogException(exception, this));
        }

        private async UniTask RunSpawnerAsync(CancellationToken cancellationToken)
        {
            if (await Delay(_initialDelay, cancellationToken))
            {
                return;
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                RemoveInactiveUnits();
                var availablePlaces = Mathf.Max(0, _maximumAliveUnits - _spawnedUnits.Count);
                var spawnCount = Mathf.Min(_unitsPerWave, availablePlaces);
                for (var index = 0; index < spawnCount; index++)
                {
                    SpawnEnemy();
                    if (index + 1 < spawnCount && await Delay(_unitSpawnInterval, cancellationToken))
                    {
                        return;
                    }
                }

                if (await Delay(_waveInterval, cancellationToken))
                {
                    return;
                }
            }
        }

        private void SpawnEnemy()
        {
            var spawnPoint = GetNextSpawnPoint();
            var parent = _unitsParent != null ? _unitsParent : transform.parent;
            var unit = _unitPool.Spawn(_enemyPrefab, spawnPoint.position, spawnPoint.rotation, parent);
            if (unit == null)
            {
                return;
            }

            unit.SetData(new AssaultOrderData
            {
                Destination = _assaultTarget.position,
                ArrivalDistance = _arrivalDistance
            });
            _spawnedUnits.Add(unit);
        }

        private Transform GetNextSpawnPoint()
        {
            if (_spawnPoints == null || _spawnPoints.Length == 0)
            {
                return transform;
            }

            for (var offset = 0; offset < _spawnPoints.Length; offset++)
            {
                var index = (_nextSpawnPoint + offset) % _spawnPoints.Length;
                if (_spawnPoints[index] == null)
                {
                    continue;
                }

                _nextSpawnPoint = (index + 1) % _spawnPoints.Length;
                return _spawnPoints[index];
            }

            return transform;
        }

        private void RemoveInactiveUnits()
        {
            for (var index = _spawnedUnits.Count - 1; index >= 0; index--)
            {
                var unit = _spawnedUnits[index];
                if (unit == null || !unit.gameObject.activeInHierarchy)
                {
                    _spawnedUnits.RemoveAt(index);
                }
            }
        }

        private static async UniTask<bool> Delay(float seconds, CancellationToken cancellationToken)
        {
            if (seconds <= 0f)
            {
                return cancellationToken.IsCancellationRequested;
            }

            return await UniTask.Delay(TimeSpan.FromSeconds(seconds), cancellationToken: cancellationToken)
                .SuppressCancellationThrow();
        }

        private void StopSpawning()
        {
            if (_spawnCancellation == null)
            {
                return;
            }

            _spawnCancellation.Cancel();
            _spawnCancellation.Dispose();
            _spawnCancellation = null;
        }

        private void OnValidate()
        {
            _initialDelay = Mathf.Max(0f, _initialDelay);
            _waveInterval = Mathf.Max(0.1f, _waveInterval);
            _unitsPerWave = Mathf.Max(1, _unitsPerWave);
            _unitSpawnInterval = Mathf.Max(0f, _unitSpawnInterval);
            _maximumAliveUnits = Mathf.Max(1, _maximumAliveUnits);
            _arrivalDistance = Mathf.Max(0.1f, _arrivalDistance);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            if (_spawnPoints == null || _spawnPoints.Length == 0)
            {
                Gizmos.DrawWireSphere(transform.position, 0.6f);
            }
            else
            {
                for (var index = 0; index < _spawnPoints.Length; index++)
                {
                    if (_spawnPoints[index] != null)
                    {
                        Gizmos.DrawWireSphere(_spawnPoints[index].position, 0.6f);
                    }
                }
            }

            if (_assaultTarget != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, _assaultTarget.position);
                Gizmos.DrawWireSphere(_assaultTarget.position, _arrivalDistance);
            }
        }
#endif
    }
}
