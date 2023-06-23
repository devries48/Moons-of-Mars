using Cinemachine;
using MoonsOfMars.Shared;
using UnityEngine;
using UnityEngine.InputSystem;
using static MoonsOfMars.Shared.Utils;

namespace Game.SpaceShooter
{
    public class GameManager : GameManagerBase<GameManager>
    {

        public enum PlayerCamera { panels, cockpit, follow }

        [SerializeField] CinemachineVirtualCamera _menuCamera;
        [SerializeField] CinemachineVirtualCamera[] _playerCameras;

        int _activeCameraIndex;
        bool _moveCamIn;

        protected override void Awake()
        {
            base.Awake();
            ActivateMenuCamera(true);
            //Cursor.lockState = CursorLockMode.Confined;
            //Cursor.visible = false;
        }

        void ActivateMenuCamera(bool activate)
        {
            _menuCamera.Priority = activate ? 1 : 10;
        }

      //  public void PlayEffect(Effect effect, Vector3 position, Quaternion rotation, float scale = 1f, ObjectLayer layer = ObjectLayer.Effects)
       //=> EffectsManager.StartEffect(effect, position, rotation, scale, layer);

        #region Input Methods

        public void OnSwitchCamera(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            if (_moveCamIn)
            {
                _activeCameraIndex--;
                if (_activeCameraIndex == -1)
                {
                    _activeCameraIndex = 1;
                    _moveCamIn = false;
                }

            }
            else
            {
                _activeCameraIndex++;
                if (_activeCameraIndex == _playerCameras.Length)
                {
                    _activeCameraIndex = _playerCameras.Length - 2;
                    _moveCamIn = true;
                }
            }

            for (int i = 0; i < _playerCameras.Length; i++)
            {
                var cam = _playerCameras[i];
                cam.Priority = i == _activeCameraIndex ? 10 : 2;
            }
        }

        public void OnQuit(InputAction.CallbackContext context)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
        }
        #endregion
    }
}