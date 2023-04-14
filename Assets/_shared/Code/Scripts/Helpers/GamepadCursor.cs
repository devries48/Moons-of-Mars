using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;

namespace MoonsOfMars.Shared
{
    public class GamepadCursor : MonoBehaviour
    {

        [SerializeField] PlayerInput _playerInput;
        [SerializeField] RectTransform _cursorTransform;
        [SerializeField] RectTransform _canvasTransform;
        [SerializeField] Canvas _canvas;
        [SerializeField] float _cursorSpeed = 10f;
        [SerializeField] float _padding = 35f;
        [SerializeField] Camera _uiCamera;

        Mouse _virtualMouse;
        Mouse _currentMouse;
        Camera _camera;
        bool _prevMouseState;
        string _prevControlSchema = "";

        const string GamepadScheme = "Gamepad";
        const string mouseScheme = "Keyboard&Mouse";

        void OnEnable()
        {
            //_camera = Camera.main;
            _currentMouse = Mouse.current;

            var activeVirtualMouse = InputSystem.GetDevice("VirtualMouse");

            if (_virtualMouse == null)
                _virtualMouse = (Mouse)InputSystem.AddDevice("VirtualMouse");
            else if (!_virtualMouse.added)
                InputSystem.AddDevice(_virtualMouse);
            else
                _virtualMouse = (Mouse)activeVirtualMouse;

            // Pair the device to the user to use the PlayerInput component with the Event System & virtual Mouse.
            InputUser.PerformPairingWithDevice(_virtualMouse, _playerInput.user);

            if (_cursorTransform != null)
            {
                var position = _cursorTransform.anchoredPosition;
                InputState.Change(_virtualMouse.position, position);
            }

            InputSystem.onAfterUpdate += UpdateMotion;
            _playerInput.onControlsChanged += OnControlsChanged;
        }

        void OnDisable()
        {
            InputSystem.RemoveDevice(_virtualMouse);
            InputSystem.onAfterUpdate -= UpdateMotion;

            _playerInput.onControlsChanged -= OnControlsChanged;
        }

        void UpdateMotion()
        {
            if (_virtualMouse == null || Gamepad.current == null)
                return;

            // Delta
            var deltaValue = Gamepad.current.leftStick.ReadValue();
            deltaValue *= _cursorSpeed * Time.unscaledDeltaTime;

            var currentPosition = _virtualMouse.position.ReadValue();
            var newPosition = currentPosition + deltaValue;

            newPosition.x = Mathf.Clamp(newPosition.x, _padding, Screen.width - _padding);
            newPosition.y = Mathf.Clamp(newPosition.y, _padding, Screen.height - _padding);

            InputState.Change(_virtualMouse.position, newPosition);
            InputState.Change(_virtualMouse.delta, deltaValue);

            var aButtonPressed = Gamepad.current.aButton.isPressed;
            if (_prevMouseState != aButtonPressed)
            {
                _virtualMouse.CopyState<MouseState>(out var mouseState);
                mouseState.WithButton(MouseButton.Left, aButtonPressed);
                InputState.Change(_virtualMouse, mouseState);
                _prevMouseState = aButtonPressed;
            }
            AnchorCursor(_virtualMouse.position.ReadValue());
        }

        void AnchorCursor(Vector2 position)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasTransform, position,
                _canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : _uiCamera, out Vector2 anchoredPosition);

            _cursorTransform.anchoredPosition = anchoredPosition;
        }

        void OnControlsChanged(PlayerInput input)
        {
            if(_virtualMouse == null)
                return;

            if (_playerInput.currentControlScheme == mouseScheme && _prevControlSchema != mouseScheme)
            {
                _cursorTransform.gameObject.SetActive(false);
                Cursor.visible = true;
                _currentMouse.WarpCursorPosition(_virtualMouse.position.ReadValue());
                _prevControlSchema = mouseScheme;
            }
            else if (_playerInput.currentControlScheme == GamepadScheme && _prevControlSchema != GamepadScheme)
            {
                _cursorTransform.gameObject.SetActive(true);
                Cursor.visible = false;
                InputState.Change(_virtualMouse.position, _currentMouse.position.ReadValue());
                AnchorCursor(_currentMouse.position.ReadValue());
                _prevControlSchema = GamepadScheme;

            }
        }
    }
}