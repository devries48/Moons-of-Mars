using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.XInput;

namespace MoonsOfMars.Shared
{
    /// <summary>
    /// Component to sit next to Player Input Component and Input System UI Input Module.
    /// The Player Input Behavior must be set to 'Send Messages'.
    /// </summary>
    [SuppressMessage("", "IDE0051", Justification = "Methods used by Player Input SendMessages")]
    [RequireComponent(typeof(PlayerInput), typeof(InputSystemUIInputModule))]
    public class InputManagerBase : SingletonBase<InputManagerBase>
    {
        const string GamepadScheme = "Gamepad";
        const string MouseScheme = "Keyboard&Mouse";

        enum GamepadType { init, none, generic, playstation, xbox }

        [SerializeField] RectTransform _cursorTransform;
        [SerializeField] Canvas _canvas;
        [SerializeField] float _cursorSpeed = 500f;
        [SerializeField] float _padding = 35f;
        [SerializeField] Camera _uiCamera;

        public bool HasGamepad => _curGamepad > GamepadType.none;

        PlayerInput _playerInput;
        Mouse _virtualMouse, _currentMouse;
        bool _prevMouseState;
        string _prevControlSchema = "";
        GamepadType _curGamepad = GamepadType.init;

        protected override void Awake()
        {
            base.Awake();

            // See https://answers.unity.com/questions/1919658/multiple-actions-bind-to-the-same-keyboard-key-do.html
            InputSystem.settings.SetInternalFeatureFlag("DISABLE_SHORTCUT_SUPPORT", true);
            InputSystem.onDeviceChange += (_, _) => CheckGamepads();

            CheckGamepads();
        }

        void OnEnable()
        {
            _currentMouse = Mouse.current;
            _playerInput = GetComponent<PlayerInput>();

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
            //_playerInput.onControlsChanged += OnControlsChanged;
        }

        void OnDisable()
        {
            InputSystem.RemoveDevice(_virtualMouse);
            InputSystem.onAfterUpdate -= UpdateMotion;

            //_playerInput.onControlsChanged -= OnControlsChanged;
        }

        /// <summary>
        /// Override this method to handle Gamepad changes.
        /// </summary>
        protected virtual void OnGamepadChanged() { }

        void CheckGamepads()
        {
            var result = GamepadType.none;

            for (var i = 0; i < InputSystem.devices.Count - 1; i++)
            {
                var device = InputSystem.devices[i];

                if (device is Gamepad)
                {
                    result = GamepadType.generic;
                    if (device is DualShockGamepad)
                    {
                        print("Playstation gamepad");
                        result = GamepadType.playstation;
                    }
                    else if (device is XInputController)
                    {
                        print("Xbox gamepad");
                        result = GamepadType.xbox;
                    }
                }
                else
                {
                    print(device.ToString());
                }
            }

            if (result != _curGamepad)
            {
                _curGamepad = result;
                OnGamepadChanged();
            }
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
            _canvas.gameObject.TryGetComponent(out RectTransform rect);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, position,
                _canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : _uiCamera, out Vector2 anchoredPosition);

            _cursorTransform.anchoredPosition = anchoredPosition;
        }

        /// <summary>
        /// Player Input SendMessages
        /// </summary>
        void OnControlsChanged(PlayerInput input)
        {
            if (_virtualMouse == null)
                return;

            if (_playerInput.currentControlScheme == MouseScheme && _prevControlSchema != MouseScheme)
            {
                _cursorTransform.gameObject.SetActive(false);
                Cursor.visible = true;
                _currentMouse.WarpCursorPosition(_virtualMouse.position.ReadValue());
                _prevControlSchema = MouseScheme;
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