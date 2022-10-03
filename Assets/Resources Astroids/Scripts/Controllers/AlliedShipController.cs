using UnityEngine;

namespace Game.Astroids
{
    public class AlliedShipController : SpaceShipMonoBehaviour
    {
        #region editor fields
        [Header("Allied Ship")]

        [SerializeField]
        ThrustController _thrustController;

        [SerializeField]
        AudioSource spawnAudio;

        #endregion

        bool _isShipRemoved;        // Prevent ship remove recursion
        bool _isPackageEjected;     // Prevent ejecing multiple packages
                                    
        Vector3 _oldPos;            // used to determine direction
        Vector3 _targetPos;

        protected override void Awake()
        {
            m_shipType = ShipType.allied;   
            base.Awake();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _isShipRemoved = false;
            _isPackageEjected = false;

            _oldPos = new();
            MoveShip();
        }

        void Update()
        {
            Vector3 direction = (transform.position - _oldPos).normalized;

            if (direction != Vector3.zero || _oldPos == Vector3.zero)
                transform.rotation = Quaternion.LookRotation(direction);

            if (!_isPackageEjected && transform.position.z > -.5f && transform.position.z < .5f)
                EjectPackage();

            if (!_isShipRemoved && Vector3.Distance(transform.position, _targetPos) < .1f)
                RemoveShip();
        }

        void LateUpdate() => _oldPos = transform.position;

        void MoveShip()
        {
            var duration = 5f;

            LeanTween.move(gameObject, CreatePath(), duration)
                .setEaseOutQuad()
                .setOrientToPath(true);

            if (_thrustController != null)
            {
                LeanTween.value(gameObject, 1f, 0f, duration).setOnUpdate((float val) =>
                    {
                        _thrustController.SetThrust(val);
                    })
                    .setEaseOutCubic();
            }
            spawnAudio.volume = 1f;
            spawnAudio.Play();
            GameManager.AudioFadeOut(duration - .5f, 0f, 1f);
            //StartCoroutine(FadeMixerGroup.StartFade(spawnAudio.outputAudioMixerGroup.audioMixer, "Vol1", duration - .5f, 0f, 1f));
        }

        void RemoveShip()
        {
            _isShipRemoved = true;
            RemoveFromGame(5f);
        }

        void EjectPackage()
        {
            _isPackageEjected = true;
            GameManager.m_powerupManager.SpawnPowerup(transform.position);
        }

        LTBezierPath CreatePath(int increments = 4)
        {
            var path = new Vector3[increments];

            // first position, spawn
            _oldPos = new Vector3(Random.Range(-50f, 50f), Random.Range(-20f, 20f), 100f);
            transform.position = _oldPos;
            path[0] = _oldPos;

            // second position, bring within game cam view
            float x = Random.Range(-15f, 15f);
            path[1] = new Vector3(x, Random.Range(-8f, -8f), 0);

            // third position
            if (x < 0)
                x += 20;
            else
                x -= 20;

            path[2] = new Vector3(x, Random.Range(-8f, 8f), 0);

            // last position
            _targetPos = new Vector3(Random.Range(-10f, 10f), Random.Range(-8f, 8f), -31f);
            path[3] = _targetPos;

            return new LTBezierPath(path);
        }
    }
}
