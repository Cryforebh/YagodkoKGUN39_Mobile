using Game.GameEngine.Ecs;
using UnityEngine;

namespace SampleProject
{
    public sealed class PooledUnitHandle : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private IGameObjectPool _pool;
        private string _poolId;
        private Rigidbody _rigidbody;
        private Animator _animator;
        private AnimatorMachine _animatorMachine;
        private Renderer[] _renderers;
        private Color[] _initialColors;
        private bool _isConfigured;
        private bool _hasSpawned;

        public void Configure(IGameObjectPool pool, string poolId)
        {
            _pool = pool;
            _poolId = poolId;
            _rigidbody = GetComponent<Rigidbody>();
            _animator = GetComponentInChildren<Animator>(true);
            _animatorMachine = GetComponentInChildren<AnimatorMachine>(true);
            _renderers = GetComponentsInChildren<Renderer>(true);
            _initialColors = new Color[_renderers.Length];
            for (var i = 0; i < _renderers.Length; i++)
            {
                var material = _renderers[i].sharedMaterial;
                _initialColors[i] = material != null && material.HasProperty(BaseColorId) ? material.GetColor(BaseColorId) : Color.white;
            }

            _isConfigured = true;
        }

        public void Prepare(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
            var entity = GetComponent<Entities.CharacterEntity>();
            if (entity != null)
            {
                entity.StopAllCoroutines();
            }

            if (_rigidbody != null)
            {
                _rigidbody.isKinematic = false;
                _rigidbody.velocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
                _rigidbody.constraints = RigidbodyConstraints.FreezePositionY |
                                         RigidbodyConstraints.FreezeRotationX |
                                         RigidbodyConstraints.FreezeRotationZ;
            }

            if (!_hasSpawned)
            {
                _hasSpawned = true;
                return;
            }

            if (_animator != null)
            {
                _animator.Rebind();
            }

            if (_animatorMachine != null)
            {
                _animatorMachine.ChangeState(AnimatorStateId.IDLE);
            }

            for (var i = 0; i < _renderers.Length; i++)
            {
                var material = _renderers[i].material;
                if (material != null && material.HasProperty(BaseColorId))
                {
                    material.SetColor(BaseColorId, _initialColors[i]);
                }
            }
        }

        private void OnDisable()
        {
            if (_isConfigured)
            {
                _pool.Release(_poolId, gameObject, false);
            }
        }
    }
}
