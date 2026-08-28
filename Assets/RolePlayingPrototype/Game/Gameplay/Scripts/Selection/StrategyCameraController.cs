using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace SampleProject
{
    [RequireComponent(typeof(Camera))]
    public sealed class StrategyCameraController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField, Min(0f)] private float _keyboardSpeed = 18f;
        [SerializeField, Min(0f)] private float _dragSpeed = 0.035f;
        [SerializeField, Min(0f)] private float _smoothTime = 0.08f;

        [Header("Zoom")]
        [SerializeField, Min(0f)] private float _mouseZoomSpeed = 0.025f;
        [SerializeField, Min(0f)] private float _pinchZoomSpeed = 0.035f;
        [SerializeField, Min(0f)] private float _minimumHeight = 12f;
        [SerializeField, Min(0f)] private float _maximumHeight = 45f;

        [Header("Bounds")]
        [SerializeField] private BoxCollider _movementBounds;

        public bool BlocksPointerInteraction => _touchGestureActive || _blockPointerUntilRelease;

        private Vector3 _targetPosition;
        private Vector3 _movementVelocity;
        private Vector2 _previousMousePosition;
        private Vector2 _previousFirstTouch;
        private Vector2 _previousSecondTouch;
        private bool _mouseDragging;
        private bool _touchGestureActive;
        private bool _touchGestureIgnored;
        private bool _blockPointerUntilRelease;

        private void Awake()
        {
            _targetPosition = transform.position;
            ClampTargetPosition();
        }

        private void Update()
        {
            HandleKeyboard();
            HandleMouse();
            HandleTouches();
            ClampTargetPosition();
            transform.position = Vector3.SmoothDamp(transform.position, _targetPosition, ref _movementVelocity, _smoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
        }

        private void HandleKeyboard()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            var horizontal = GetKeyAxis(keyboard.aKey, keyboard.dKey) + GetKeyAxis(keyboard.leftArrowKey, keyboard.rightArrowKey);
            var vertical = GetKeyAxis(keyboard.sKey, keyboard.wKey) + GetKeyAxis(keyboard.downArrowKey, keyboard.upArrowKey);
            var input = Vector2.ClampMagnitude(new Vector2(horizontal, vertical), 1f);
            if (input.sqrMagnitude <= 0f)
            {
                return;
            }

            var right = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
            var forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            _targetPosition += (right * input.x + forward * input.y) * (_keyboardSpeed * Time.unscaledDeltaTime);
        }

        private void HandleMouse()
        {
            var mouse = Mouse.current;
            if (mouse == null)
            {
                return;
            }

            var pointerPosition = mouse.position.ReadValue();
            if (mouse.middleButton.wasPressedThisFrame)
            {
                _mouseDragging = !IsPointerOverUi();
                _previousMousePosition = pointerPosition;
            }

            if (_mouseDragging && mouse.middleButton.isPressed)
            {
                PanByScreenDelta(pointerPosition - _previousMousePosition);
                _previousMousePosition = pointerPosition;
            }

            if (mouse.middleButton.wasReleasedThisFrame)
            {
                _mouseDragging = false;
            }

            if (!IsPointerOverUi())
            {
                var scroll = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    Zoom(scroll * _mouseZoomSpeed);
                }
            }
        }

        private void HandleTouches()
        {
            var touchscreen = Touchscreen.current;
            if (touchscreen == null)
            {
                return;
            }

            var firstIndex = -1;
            var secondIndex = -1;
            var activeTouchCount = 0;
            for (var i = 0; i < touchscreen.touches.Count; i++)
            {
                if (!touchscreen.touches[i].press.isPressed)
                {
                    continue;
                }

                if (activeTouchCount == 0)
                {
                    firstIndex = i;
                }
                else if (activeTouchCount == 1)
                {
                    secondIndex = i;
                }

                activeTouchCount++;
            }

            if (activeTouchCount < 2)
            {
                _touchGestureActive = false;
                _touchGestureIgnored = false;
                if (activeTouchCount == 0)
                {
                    _blockPointerUntilRelease = false;
                }

                return;
            }

            var firstTouch = touchscreen.touches[firstIndex];
            var secondTouch = touchscreen.touches[secondIndex];
            var firstPosition = firstTouch.position.ReadValue();
            var secondPosition = secondTouch.position.ReadValue();
            if (!_touchGestureActive)
            {
                _touchGestureActive = true;
                _blockPointerUntilRelease = true;
                _touchGestureIgnored = IsPointerOverUi(firstTouch.touchId.ReadValue()) || IsPointerOverUi(secondTouch.touchId.ReadValue());
                _previousFirstTouch = firstPosition;
                _previousSecondTouch = secondPosition;
                return;
            }

            if (!_touchGestureIgnored)
            {
                var previousCenter = (_previousFirstTouch + _previousSecondTouch) * 0.5f;
                var currentCenter = (firstPosition + secondPosition) * 0.5f;
                PanByScreenDelta(currentCenter - previousCenter);

                var previousDistance = Vector2.Distance(_previousFirstTouch, _previousSecondTouch);
                var currentDistance = Vector2.Distance(firstPosition, secondPosition);
                Zoom((currentDistance - previousDistance) * _pinchZoomSpeed);
            }

            _previousFirstTouch = firstPosition;
            _previousSecondTouch = secondPosition;
        }

        private void PanByScreenDelta(Vector2 delta)
        {
            var right = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
            var forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            _targetPosition -= (right * delta.x + forward * delta.y) * _dragSpeed;
        }

        private void Zoom(float amount)
        {
            var direction = transform.forward;
            if (Mathf.Abs(direction.y) < 0.001f)
            {
                return;
            }

            var desiredHeight = Mathf.Clamp(_targetPosition.y + direction.y * amount, _minimumHeight, _maximumHeight);
            var clampedAmount = (desiredHeight - _targetPosition.y) / direction.y;
            _targetPosition += direction * clampedAmount;
        }

        private void ClampTargetPosition()
        {
            _targetPosition.y = Mathf.Clamp(_targetPosition.y, _minimumHeight, _maximumHeight);
            if (_movementBounds == null)
            {
                return;
            }

            var groundHeight = _movementBounds.transform.TransformPoint(_movementBounds.center).y;
            var direction = transform.forward;
            if (Mathf.Abs(direction.y) < 0.001f)
            {
                return;
            }

            var focusDistance = (groundHeight - _targetPosition.y) / direction.y;
            var focus = _targetPosition + direction * focusDistance;
            var localFocus = _movementBounds.transform.InverseTransformPoint(focus);
            var minimum = _movementBounds.center - _movementBounds.size * 0.5f;
            var maximum = _movementBounds.center + _movementBounds.size * 0.5f;
            localFocus.x = Mathf.Clamp(localFocus.x, minimum.x, maximum.x);
            localFocus.z = Mathf.Clamp(localFocus.z, minimum.z, maximum.z);
            var clampedFocus = _movementBounds.transform.TransformPoint(localFocus);
            _targetPosition += Vector3.ProjectOnPlane(clampedFocus - focus, Vector3.up);
        }

        private float GetKeyAxis(KeyControl negative, KeyControl positive)
        {
            return (positive.isPressed ? 1f : 0f) - (negative.isPressed ? 1f : 0f);
        }

        private bool IsPointerOverUi(int pointerId = -1)
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(pointerId);
        }
    }
}
