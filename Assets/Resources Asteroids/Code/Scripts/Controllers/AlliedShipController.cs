using UnityEngine;

namespace Game.Astroids
{
    public class AlliedShipController : GameMonoBehaviour
    {
        #region editor fields
        [SerializeField] bool isPlayer;
        [SerializeField] float duration = 5f;
        [SerializeField] ThrustController thrustController;
        [SerializeField] AudioSource spawnAudio;
        [SerializeField] AudioClip[] spawnClips;
        #endregion

        bool _isShipRemoved;        // Prevent ship remove recursion
        bool _isPackageEjected;     // Prevent ejecing multiple packages

        Vector3 _oldPos;            // used to determine direction
        Vector3 _targetPos;

        void OnEnable()
        {
            _isShipRemoved = false;
            _isPackageEjected = false;

            _oldPos = new();
            if (!isPlayer) MoveShipIn(duration);
        }

        protected override void FixedUpdate()
        {

            Vector3 direction = (transform.position - _oldPos).normalized;

            if (direction != Vector3.zero || _oldPos == Vector3.zero)
            {
                print(direction);
                transform.rotation = Quaternion.LookRotation(direction);
            }

            if (isPlayer) return;

            if (!_isPackageEjected && transform.position.z > -.5f && transform.position.z < .5f)
                EjectPackage();

            if (!_isShipRemoved && Vector3.Distance(transform.position, _targetPos) < .1f)
                RemoveShip(duration);
        }

        void LateUpdate() => _oldPos = transform.position;

        public void PlayerShipJumpOut(float duration) => MoveShipIn(duration);

        public void PlayerShipJumpIn(float duration)
        {
            _oldPos = Vector3.zero;
            var pos = GameManager.GetWorldJumpPosition();
            pos.z = -1;
            transform.position = new Vector3(pos.x, pos.y, transform.position.z);

            LeanTween.move(gameObject, pos, duration)
                .setEaseInQuad()
                .setOnComplete(() => RemoveShip());

            AnimateThrust(duration, LeanTweenType.easeInQuint);
        }

        void MoveShipIn(float duration)
        {
            LeanTween.move(gameObject, CreatePath(), duration)
                .setEaseOutQuad()
                .setOrientToPath(true);

            AnimateThrust(duration);
        }

        void AnimateThrust(float duration, LeanTweenType type = default)
        {
            if (type == default)
                type = isPlayer ? LeanTweenType.easeInOutQuint : LeanTweenType.easeInQuint;

            if (thrustController != null)
            {
                LeanTween.value(gameObject, 1f, 0f, duration)
                    .setOnUpdate((float val) => thrustController.SetThrust(val))
                    .setEase(type);
            }
            PlaySpawnClip(duration);
        }

        void PlaySpawnClip(float duration)
        {
            if (spawnAudio == null || spawnClips == null || spawnClips.Length == 0)
                return;

            var clip = spawnClips[Random.Range(0, spawnClips.Length)];
            spawnAudio.volume = 1f;
            spawnAudio.PlayOneShot(clip);
            StartCoroutine(AudioUtil.FadeOut(spawnAudio, duration - .5f));
        }

        void RemoveShip(float duration = 0)
        {
            _isShipRemoved = true;
            RemoveFromGame(duration);
        }

        void EjectPackage()
        {
            _isPackageEjected = true;
            GameManager.m_PowerupManager.SpawnPowerup(transform.position);
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
