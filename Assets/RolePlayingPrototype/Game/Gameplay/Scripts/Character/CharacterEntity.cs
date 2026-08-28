using System.Collections.Generic;
using Game.GameEngine.Ecs;
using SampleProject;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Entities
{
    public sealed class CharacterEntity : Entity
    {
        [SerializeField]
        [FormerlySerializedAs("config")]
        private CharacterConfig _config;

        [SerializeField]
        [FormerlySerializedAs("team")]
        private TeamId _team = TeamId.Player;

        [SerializeField, Min(0), Tooltip("Set to 0 to use Hit Points from CharacterConfig.")]
        [FormerlySerializedAs("hitPoints")]
        private int _hitPoints;

        [SerializeField, Min(0.1f)]
        [FormerlySerializedAs("visionRange")]
        private float _visionRange = 6f;

        [SerializeField, Min(0.1f)]
        [FormerlySerializedAs("allyAssistRange")]
        private float _allyAssistRange = 10f;

        [SerializeField, Min(0.05f)]
        [FormerlySerializedAs("visionScanInterval")]
        private float _visionScanInterval = 0.25f;

        [SerializeField]
        [FormerlySerializedAs("disableEnemyDetection")]
        private bool _disableEnemyDetection;

        [SerializeField]
        [Tooltip("Optional world-space patrol route used when this scene unit is created. Maximum 6 points.")]
        private Vector3[] _initialPatrolPoints = System.Array.Empty<Vector3>();

        [SerializeField]
        [Tooltip("Optional shared scene patrol group. Its route takes priority over Initial Patrol Points.")]
        private PatrolGroupAuthoring _initialPatrolGroup;

        [SerializeField, Min(0.1f)]
        [FormerlySerializedAs("deathAnimationDuration")]
        private float _deathAnimationDuration = 1f;

        [Inject]
        private IUnitCollisionService _unitCollisionService;

        protected override void Init()
        {
            _unitCollisionService.Register(GetComponentsInChildren<Collider>());
            SetData(new SmoothRotationComponent());

            SetData(new TeamComponent { Value = _team });

            if (!_disableEnemyDetection)
            {
                SetData(new VisionComponent
                {
                    Range = _visionRange,
                    AssistRange = _allyAssistRange,
                    ScanInterval = _visionScanInterval,
                    NextScanTime = Random.Range(0f, _visionScanInterval)
                });
            }

            SetData(new DeathSettingsComponent { Duration = _deathAnimationDuration });

            SetData(new CombatComponent
            {
                Damage = _config.Damage,
                MinDistance = _config.MinDistance,
                AnimationTime = _config.AnimationTime,
                TimeBetweenAttack = _config.TimeBetweenAttack,
                DamageType = _config.DamageType
            });
            
            SetData(new AnimatorComponent
            {
                Value = GetComponentInChildren<AnimatorMachine>()
            });
            
            SetData(new HitPointsComponent
            {
                Max = _hitPoints > 0 ? _hitPoints : _config.HitPoints,
                Current = _hitPoints > 0 ? _hitPoints : _config.HitPoints
            });

            SetData(new MoveSpeedComponent
            {
                Value = _config.MoveSpeed
            });

            SetData(new TransformComponent
            {
                Value = transform,
                Radius = _config.Radius
            });

            SetData(new GameObjectComponent
            {
                Value = gameObject
            });

            SetData(new RigidbodyComponent
            {
                Value = GetComponent<Rigidbody>()
            });

            SetData(new RendererComponent
            {
                Value = GetComponentInChildren<Renderer>()
            });

            SetInitialPatrol();
        }

        private void OnValidate()
        {
            if (_initialPatrolPoints != null && _initialPatrolPoints.Length > PatrolRouteEditor.MaximumPointCount)
            {
                System.Array.Resize(ref _initialPatrolPoints, PatrolRouteEditor.MaximumPointCount);
            }
        }

        private void SetInitialPatrol()
        {
            if (_initialPatrolGroup != null && _initialPatrolGroup.TryJoin(Handle, out var sharedGroup, out var sharedPoints))
            {
                SetInitialPatrol(sharedPoints, sharedGroup);
                return;
            }

            if (_initialPatrolPoints == null || _initialPatrolPoints.Length == 0)
            {
                return;
            }

            var points = new List<Vector3>(_initialPatrolPoints.Length);
            for (var i = 0; i < _initialPatrolPoints.Length; i++)
            {
                points.Add(_initialPatrolPoints[i]);
            }

            var group = new PatrolGroupState(points);
            group.Add(Handle);
            SetInitialPatrol(points, group);
        }

        private void SetInitialPatrol(List<Vector3> points, PatrolGroupState group)
        {
            SetData(new PatrolRouteComponent { Points = points, Group = group });
            SetData(new CommandRequest { Type = CommandType.PATROL_BY_POINTS, Status = CommandStatus.IDLE, Args = points });
        }
    }
}
