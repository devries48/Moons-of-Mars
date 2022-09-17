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
        float smallAstroidScale = .4f;

        [SerializeField, Range(0f, 1f)]
        float mediumAstroidScale = .6f;

        [SerializeField, Range(0f, 1f)]
        float largeAstroidScale = .8f;

        [Header("Astroid Score")]

        [SerializeField, Range(0, 200)]
        int smallAstroidScore = 50;

        [SerializeField, Range(0f, 200)]
        int mediumAstroidScore = 25;

        [SerializeField, Range(0f, 200)]
        int largeAstroidScore = 10;

        [SerializeField]
        AstroidSounds sounds = new();
        #endregion

        #region properties

        public int Generation { get; private set; }

        #endregion

        #region fields
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

            _rb.velocity = new Vector2(
                Mathf.Clamp(_rb.velocity.x, -maxSpeed, maxSpeed),
                Mathf.Clamp(_rb.velocity.y, -maxSpeed, maxSpeed));

            GameManager.ScreenWrapObject(gameObject);
        }

        void OnCollisionEnter(Collision other)
        {
            var c = other.collider;
            var o = other.gameObject;

            // TODO constant tag names shared static method: CompareTag(other, Tags.Player)
            if (c.CompareTag("Player"))
                GameManager.PlayerDestroyed(o);

            else if (c.CompareTag("Bullet"))
                HitByBullet(o);

            else if (c.CompareTag("AlienBullet"))
                HitByAlienBullet(o);

            else if (c.CompareTag("Astroid"))
                HitByAstroid(o);
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
            GameManager.AsterodDestroyed();

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
                PlayEffect(EffectsManager.Effect.dustExplosion, transform.position, smallAstroidScale);
                PlayAudioClip(AstroidSounds.Clip.Explode, 3);
                print("dat is een extra steentje!");
                CreateSmallAsteriods(1, bullet.transform.position);
            }
        }

        IEnumerator ExplodeAstroid()
        {
            _explosionActive = true;
            _render.enabled = false;

            var scale = Generation switch
            {
                1 => largeAstroidScale,
                2 => mediumAstroidScale,
                _ => smallAstroidScale
            };

            var points = Generation switch
            {
                1 => largeAstroidScore,
                2 => mediumAstroidScore,
                _ => smallAstroidScore
            };

            PlayEffect(EffectsManager.Effect.dustExplosion, transform.position, scale);
            PlayAudioClip(AstroidSounds.Clip.Explode, Generation);
            Score(points);

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

            GameManager.SpawnAsteroids(asteroidsNum, newGeneration, position);
        }

        void PlayAudioClip(AstroidSounds.Clip clip, float generation)
        {
            sounds.SetVolume(Audio, generation, clip == AstroidSounds.Clip.Collide);

            var sound = sounds.GetClip(clip);

            PlaySound(sound);
        }
    }
}