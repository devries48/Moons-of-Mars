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
        AsteroidSounds sounds = new();
        #endregion

        #region properties
        Rigidbody Rb
        {
            get
            {
                if (__rb == null)
                    __rb = GetComponent<Rigidbody>();

                return __rb;
            }
        }
        Rigidbody __rb;

        Renderer Renderer
        {
            get
            {
                if (__renderer == null)
                    __renderer = GetComponent<Renderer>();

                return __renderer;
            }
        }
        Renderer __renderer;
     
        public int Generation { get; private set; }
        #endregion

        #region fields
        float _rotationX;
        float _rotationY;
        float _rotationZ;
        readonly float _maxRotation = 25f;

        bool _explosionActive;
        #endregion

        void OnEnable()
        {
            RigidbodyUtil.SetRandomForceAndTorque(Rb, transform);

            _rotationX = Random.Range(-_maxRotation, _maxRotation);
            _rotationY = Random.Range(-_maxRotation, _maxRotation);
            _rotationZ = Random.Range(-_maxRotation, _maxRotation);

            Renderer.enabled = true;
            m_ScreenWrap= true;
        }

        protected override void FixedUpdate()
        {
            transform.Rotate(new Vector3(_rotationX, _rotationY, _rotationZ) * Time.deltaTime);

            Rb.velocity = new Vector2(
                Mathf.Clamp(Rb.velocity.x, -maxSpeed, maxSpeed),
                Mathf.Clamp(Rb.velocity.y, -maxSpeed, maxSpeed));

            base.FixedUpdate();
        }

        void OnCollisionEnter(Collision other)
        {
            if (!Renderer.enabled)
                return;

            var c = other.collider;
            var o = other.gameObject;

            // TODO constant tag names shared static method: CompareTag(other, Tags.Player)
            if (c.CompareTag("Player"))
                GameManager.PlayerDestroyed();
            else if (c.CompareTag("Astroid"))
                HitByAstroid(o);
        }

        void OnTriggerEnter(Collider other)
        {
            if (!Renderer.enabled)
                return;

            var c = other;
            var o = other.gameObject;

            if (c.CompareTag("AlienBullet"))
                HitByAlienBullet(o);

            else if (c.CompareTag("Bullet"))
                HitByBullet(o);
        }

        void HitByAstroid(GameObject astroid)
        {
            if (_explosionActive) return;

            astroid.TryGetComponent<AsteroidController>(out var other);

            // play smallest astroid collision sound
            var minGen = System.Math.Max(Generation, other.Generation);
            PlayAudioClip(AsteroidSounds.Clip.Collide, minGen);
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
                PlayAudioClip(AsteroidSounds.Clip.Explode, 3);
                CreateSmallAsteriods(1, bullet.transform.position);
            }
        }

        IEnumerator ExplodeAstroid()
        {
            _explosionActive = true;
            Renderer.enabled = false;

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
            PlayAudioClip(AsteroidSounds.Clip.Explode, Generation);
            Score(points, gameObject);

            while (Audio.isPlaying)
                yield return null;

            RemoveFromGame();

            _explosionActive = false;
        }

        public void SetGeneration(int generation) => Generation = generation;

        void CreateSmallAsteriods(int asteroidsNum, Vector3 position = default)
        {
            int newGeneration = Generation + 1;

            if (position == default)
                position = transform.position;
            else
                newGeneration = 3;

            GameManager.m_GameManagerData.SpawnAsteroids(asteroidsNum, newGeneration, position);
        }

        void PlayAudioClip(AsteroidSounds.Clip clip, float generation)
        {
            sounds.SetVolume(Audio, generation, clip == AsteroidSounds.Clip.Collide);

            var sound = sounds.GetClip(clip);
            PlaySound(sound);
        }
    }
}