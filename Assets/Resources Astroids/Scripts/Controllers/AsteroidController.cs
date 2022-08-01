using System.Collections;
using UnityEngine;

namespace Game.Astroids
{
    [SelectionBase]
    [RequireComponent(typeof(Rigidbody), typeof(Renderer))]
    public class AsteroidController : GameMonoBehaviour
    {
        #region editor fields

        [SerializeField]
        float maxSpeed = 3f;

        [Header("Astroid Explosion Scale")]

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
        readonly float _maxRotation = 25f;

        bool _explosionActive;
        #endregion

        private void Awake()
        {
            _gameManager = AstroidsGameManager.Instance;
            _rb = GetComponent<Rigidbody>();
            _render = GetComponent<Renderer>();
        }

        void OnEnable()
        {
            _rotationX = Random.Range(-_maxRotation, _maxRotation);
            _rotationY = Random.Range(-_maxRotation, _maxRotation);
            _rotationZ = Random.Range(-_maxRotation, _maxRotation);

            _rb.AddForce(transform.right * CreateRandomSpeed());
            _rb.AddForce(transform.up * CreateRandomSpeed());

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
            else if (collisionInfo.collider.CompareTag("AlienBullet"))
                HitByAlienBullet(collisionInfo.gameObject);
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
            RemoveFromGame(bullet);
            StartCoroutine(ExplodeAstroid());

            if (Generation < 3)
                CreateSmallAsteriods(2);
        }

        void HitByAlienBullet(GameObject bullet)
        {
            RemoveFromGame(bullet);

            if (Generation < 3)
            {
                PlayEffect(EffectsManager.Effect.dustExplosion, transform.position, smallAstroid);
                PlayAudioClip(AstroidSounds.Clip.Explode, 3);

                CreateSmallAsteriods(1, bullet.transform.position);
            }
        }

        IEnumerator ExplodeAstroid(float scale = 0)
        {
            _explosionActive = true;
            _render.enabled = false;

            if (scale == 0)
            {
                scale = Generation switch
                {
                    1 => largeAstroid,
                    2 => mediumAstroid,
                    _ => smallAstroid
                };
            }

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

        void CreateSmallAsteriods(int asteroidsNum, Vector3 position = default)
        {
            int newGeneration = Generation + 1;

            if (position == default)
                position = transform.position;
            else
                newGeneration = 3;

            _gameManager.SpawnAsteroids(asteroidsNum, newGeneration, position);
        }

        void PlayAudioClip(AstroidSounds.Clip clip, float generation)
        {
            sounds.SetVolume(Audio, generation);

            var sound = sounds.GetClip(clip);

            PlaySound(sound);
        }
    }
}