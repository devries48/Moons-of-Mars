using System.Collections;
using UnityEngine;

namespace Game.Astroids
{
    [SelectionBase]
    [RequireComponent(typeof(Rigidbody))]
    public class AsteroidController : GameMonoBehaviour
    {
        #region editor fields

        [SerializeField]
        float maxSpeed = 3f;

        [Header("Astroid Explosion Size")]

        [SerializeField, Range(0f, 1f)]
        float smallAstroid = .25f;

        [SerializeField, Range(0f, 1f)]
        float mediumAstroid = .5f;

        [SerializeField, Range(0f, 1f)]
        float largeAstroid = 1f;

        [SerializeField]
        AstroidSounds sounds = new();

        #endregion

        #region properties

        public int Generation { get; private set; }

        #endregion

        #region fields
        AstroidsGameManager _gameManager;
        Rigidbody _rb;
        Renderer _render;

        float _rotationX;
        float _rotationY;
        float _rotationZ;

        bool _explosionActive;

        #endregion

        private void Awake() => _gameManager = AstroidsGameManager.Instance;

        void Start()
        {
            var maxRotation = 25f;

            _rotationX = Random.Range(-maxRotation, maxRotation);
            _rotationY = Random.Range(-maxRotation, maxRotation);
            _rotationZ = Random.Range(-maxRotation, maxRotation);

            _rb = GetComponent<Rigidbody>();
            _rb.AddForce(transform.right * CreateRandomSpeed());
            _rb.AddForce(transform.up * CreateRandomSpeed());

            _render = GetComponent<Renderer>();
            _render.enabled = true;
        }

        void Update()
        {
            transform.Rotate(new Vector3(_rotationX, _rotationY, _rotationZ) * Time.deltaTime);

            _rb.velocity = new Vector2(Mathf.Clamp(_rb.velocity.x, -maxSpeed, maxSpeed), Mathf.Clamp(_rb.velocity.y, -maxSpeed, maxSpeed));
            _gameManager.ScreenWrapObject(gameObject);
        }

        void OnCollisionEnter(Collision collisionInfo)
        {
            // TODO constant tag names
            if (collisionInfo.collider.name == "Rocket")
                _gameManager.RocketFail();
            else if (collisionInfo.collider.CompareTag("Bullet"))
                HitByBullet(collisionInfo.gameObject);
            else if (collisionInfo.collider.CompareTag("Astroid"))
                HitByAstroid(collisionInfo.gameObject);
        }

        void HitByAstroid(GameObject astroid)
        {
            astroid.TryGetComponent<AsteroidController>(out var other);

            if (other == null)
            {
                Debug.LogWarning("AsteroidController on collision is NULL");
                return;
            }

            if (_explosionActive) return;

            // play smallest astroid collision sound
            var minGen = System.Math.Max(Generation, other.Generation);

            PlayAudioClip(AstroidSounds.Clip.Collide, minGen);
        }

        void HitByBullet(GameObject bullet)
        {
            StartCoroutine(ExplodeAstroid());

            if (Generation < 3)
                CreateSmallAsteriods(2);

            RemoveFromGame(bullet);
        }

        IEnumerator ExplodeAstroid()
        {
            _explosionActive = true;
            _render.enabled = false;

            var scale = Generation switch
            {
                1 => largeAstroid,
                2 => mediumAstroid,
                _ => smallAstroid
            };

            PlayEffect(EffectsManager.Effect.dustExplosion, transform.position, scale);
            PlayAudioClip(AstroidSounds.Clip.Explode, Generation);

            while (Audio.isPlaying)
            {
                yield return null;
            }

            RemoveFromGame();

            _explosionActive = false;
        }

        public void SetGeneration(int generation) => Generation = generation;

        float CreateRandomSpeed()
        {
            var speed = Random.Range(200f, 800f);
            var selector = Random.Range(0, 2);
            var dir = selector == 1 ? -1 : 1;

            return speed * dir;
        }

        void CreateSmallAsteriods(int asteroidsNum)
        {
            int newGeneration = Generation + 1;
            var scaleSize = 0.5f;

            for (int i = 1; i <= asteroidsNum; i++)
            {
                var AsteroidClone = Instantiate(gameObject, new Vector3(transform.position.x, transform.position.y, 0f), transform.rotation);

                AsteroidClone.transform.localScale = new Vector3(AsteroidClone.transform.localScale.x * scaleSize, AsteroidClone.transform.localScale.y * scaleSize, AsteroidClone.transform.localScale.z * scaleSize);
                AsteroidClone.GetComponent<AsteroidController>().SetGeneration(newGeneration);
            }
        }

        void PlayAudioClip(AstroidSounds.Clip clip, float generation)
        {
            sounds.SetVolume(Audio, generation);

            var sound = sounds.GetClip(clip);

            PlaySound(sound);
        }
    }
}