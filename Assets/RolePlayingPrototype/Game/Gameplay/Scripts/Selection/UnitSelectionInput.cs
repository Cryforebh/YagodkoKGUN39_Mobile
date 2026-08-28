using System.Collections.Generic;
using Game.GameEngine.Ecs;
using GameECS;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Zenject;

namespace SampleProject
{
    [RequireComponent(typeof(Camera))]
    public sealed class UnitSelectionInput : MonoBehaviour
    {
        private const float DragThreshold = 15.0f;

        [SerializeField]
        [FormerlySerializedAs("raycastMask")]
        private LayerMask _raycastMask = -1;

        private readonly InputAction _pointerPosition = new("Pointer Position", InputActionType.PassThrough, "<Pointer>/position");
        private readonly InputAction _primaryPress = new("Primary Press", InputActionType.Button, "<Pointer>/press");
        private readonly InputAction _commandPress = new("Command Press", InputActionType.Button, "<Mouse>/rightButton");
        private readonly InputAction _additiveSelection = new("Additive Selection", InputActionType.Button, "<Keyboard>/shift");

        private IUnitSelectionService _selection;
        private IGroupCommandService _commands;
        private IPatrolRouteEditor _patrolRouteEditor;
        private IContextCommandResolver _contextCommandResolver;
        private IRecruitmentService _recruitment;
        private EcsWorld _world;
        private Camera _worldCamera;
        private StrategyCameraController _cameraController;
        private readonly List<EntityHandle> _entityBuffer = new();
        private readonly List<RaycastResult> _uiRaycastResults = new();
        private Vector2 _dragStart;
        private bool _isDragging;

        [Inject]
        private void Construct(IUnitSelectionService unitSelection, IGroupCommandService groupCommands, IPatrolRouteEditor patrolRouteEditor, IContextCommandResolver contextCommandResolver, IRecruitmentService recruitment, EcsWorld ecsWorld)
        {
            _selection = unitSelection;
            _commands = groupCommands;
            _patrolRouteEditor = patrolRouteEditor;
            _contextCommandResolver = contextCommandResolver;
            _recruitment = recruitment;
            _world = ecsWorld;
        }

        private void Awake()
        {
            _worldCamera = GetComponent<Camera>();
            _cameraController = GetComponent<StrategyCameraController>();
        }

        private void OnEnable()
        {
            _pointerPosition.Enable();
            _primaryPress.Enable();
            _commandPress.Enable();
            _additiveSelection.Enable();
            _primaryPress.started += OnPrimaryStarted;
            _primaryPress.canceled += OnPrimaryCanceled;
            _commandPress.performed += OnCommandPress;
        }

        private void OnDisable()
        {
            _primaryPress.started -= OnPrimaryStarted;
            _primaryPress.canceled -= OnPrimaryCanceled;
            _commandPress.performed -= OnCommandPress;
            _pointerPosition.Disable();
            _primaryPress.Disable();
            _commandPress.Disable();
            _additiveSelection.Disable();
        }

        private void OnDestroy()
        {
            _pointerPosition.Dispose();
            _primaryPress.Dispose();
            _commandPress.Dispose();
            _additiveSelection.Dispose();
        }

        private void OnGUI()
        {
            if (!_isDragging || _cameraController != null && _cameraController.BlocksPointerInteraction || Vector2.Distance(_dragStart, _pointerPosition.ReadValue<Vector2>()) < DragThreshold)
            {
                return;
            }

            var rect = GetScreenRect(_dragStart, _pointerPosition.ReadValue<Vector2>());
            GUI.Box(new Rect(rect.xMin, Screen.height - rect.yMax, rect.width, rect.height), string.Empty);
        }

        private void OnPrimaryStarted(InputAction.CallbackContext context)
        {
            if (_cameraController != null && _cameraController.BlocksPointerInteraction)
            {
                _isDragging = false;
                return;
            }

            _dragStart = _pointerPosition.ReadValue<Vector2>();
            _isDragging = true;
        }

        private void OnPrimaryCanceled(InputAction.CallbackContext context)
        {
            if (_cameraController != null && _cameraController.BlocksPointerInteraction)
            {
                _isDragging = false;
                return;
            }

            if (!_isDragging)
            {
                return;
            }

            _isDragging = false;
            var pointer = _pointerPosition.ReadValue<Vector2>();
            var isTouch = context.control.device is Touchscreen;
            if (IsPointerOverUi(pointer))
            {
                return;
            }

            if (_patrolRouteEditor.IsEditing)
            {
                if (Vector2.Distance(_dragStart, pointer) < DragThreshold)
                {
                    TryAddPatrolPoint();
                }

                return;
            }

            if (_recruitment.IsSettingRallyPoint.Value)
            {
                if (Vector2.Distance(_dragStart, pointer) < DragThreshold)
                {
                    TrySetRallyPoint();
                }

                return;
            }

            if (Vector2.Distance(_dragStart, pointer) >= DragThreshold)
            {
                SelectInRect(GetScreenRect(_dragStart, pointer));
                return;
            }

            HandlePrimaryPress(isTouch);
        }

        private void HandlePrimaryPress(bool isTouch)
        {
            if (!TryGetPointerHit(out var hit, out var groundPoint))
            {
                return;
            }

            var recruitmentBuilding = hit.collider == null ? null : hit.collider.GetComponentInParent<RecruitmentBuilding>();
            if (recruitmentBuilding != null)
            {
                _selection.Clear();
                _recruitment.Select(recruitmentBuilding);
                return;
            }

            _recruitment.Close();

            var command = _contextCommandResolver.Resolve(hit, groundPoint);
            if (command.Type == ContextCommandType.Gather && _selection.Selected.Count > 0)
            {
                ExecuteContextCommand(command);
                return;
            }

            var entity = hit.collider == null ? null : hit.collider.GetComponentInParent<Entity>();
            if (entity != null)
            {
                if (_selection.Select(entity.Handle, _additiveSelection.IsPressed()))
                {
                    return;
                }

                if (isTouch && _selection.Selected.Count > 0)
                {
                    ExecuteContextCommand(_contextCommandResolver.Resolve(hit, groundPoint));
                }

                return;
            }

            if (isTouch && _selection.Selected.Count > 0 && command.Type == ContextCommandType.Move)
            {
                ExecuteContextCommand(command);
                return;
            }

            _selection.Clear();
        }

        private void SelectInRect(Rect rect)
        {
            _recruitment.Close();
            if (!_additiveSelection.IsPressed())
            {
                _selection.Clear();
            }

            _world.GetActiveEntities(_entityBuffer);
            for (var i = 0; i < _entityBuffer.Count; i++)
            {
                var entity = _entityBuffer[i];
                if (!_world.HasComponent<TransformComponent>(entity.Id))
                {
                    continue;
                }

                var position = _world.GetComponent<TransformComponent>(entity.Id).Value.position;
                var screenPosition = _worldCamera.WorldToScreenPoint(position);
                if (screenPosition.z > 0 && rect.Contains(screenPosition))
                {
                    _selection.Select(entity, true);
                }
            }
        }

        private void OnCommandPress(InputAction.CallbackContext context)
        {
            if (_patrolRouteEditor.IsEditing || IsPointerOverUi(_pointerPosition.ReadValue<Vector2>()) ||
                !TryGetPointerHit(out var hit, out var groundPoint))
            {
                return;
            }

            if (_recruitment.SelectedBuilding.Value != null)
            {
                TrySetRallyPoint(hit, groundPoint);
                return;
            }

            if (_selection.Selected.Count == 0)
            {
                return;
            }

            ExecuteContextCommand(_contextCommandResolver.Resolve(hit, groundPoint));
        }

        private void TrySetRallyPoint()
        {
            if (TryGetPointerHit(out var hit, out var groundPoint))
            {
                TrySetRallyPoint(hit, groundPoint);
            }
        }

        private void TrySetRallyPoint(RaycastHit hit, Vector3 groundPoint)
        {
            if ((hit.collider != null && hit.collider.GetComponentInParent<Entity>() != null) ||
                !_contextCommandResolver.TryResolveWalkablePosition(hit, groundPoint, out var walkablePosition))
            {
                return;
            }

            _recruitment.TrySetRallyPoint(walkablePosition);
        }

        private void TryAddPatrolPoint()
        {
            if (_patrolRouteEditor.PointCount >= PatrolRouteEditor.MaximumPointCount || !TryGetPointerHit(out var hit, out var groundPoint))
            {
                return;
            }

            if ((hit.collider != null && hit.collider.GetComponentInParent<Entity>() != null) || !_contextCommandResolver.TryResolveWalkablePosition(hit, groundPoint, out var walkablePosition))
            {
                return;
            }

            _patrolRouteEditor.AddPoint(walkablePosition);
        }

        private void ExecuteContextCommand(ContextCommand command)
        {
            switch (command.Type)
            {
                case ContextCommandType.Move:
                    _commands.Move(command.Position);
                    break;
                case ContextCommandType.Attack:
                    _commands.Attack(command.Target);
                    break;
                case ContextCommandType.Gather:
                    _commands.Gather(command.Target);
                    break;
            }
        }

        private bool IsPointerOverUi(Vector2 pointerPosition)
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            var eventData = new PointerEventData(EventSystem.current) { position = pointerPosition };
            _uiRaycastResults.Clear();
            EventSystem.current.RaycastAll(eventData, _uiRaycastResults);
            return _uiRaycastResults.Count > 0;
        }

        private bool TryGetPointerHit(out RaycastHit hit, out Vector3 groundPoint)
        {
            var ray = _worldCamera.ScreenPointToRay(_pointerPosition.ReadValue<Vector2>());
            if (Physics.Raycast(ray, out hit, float.MaxValue, _raycastMask))
            {
                groundPoint = hit.point;
                return true;
            }

            var groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(ray, out var distance))
            {
                groundPoint = ray.GetPoint(distance);
                return true;
            }

            groundPoint = default;
            return false;
        }

        private static Rect GetScreenRect(Vector2 start, Vector2 end)
        {
            var min = Vector2.Min(start, end);
            var max = Vector2.Max(start, end);
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }
    }
}
