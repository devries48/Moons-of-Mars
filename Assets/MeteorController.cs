using MoonsOfMars.Shared;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoonsOfMars.Game.Asteroids
{
    public class MeteorController : PoolableBase
    {
        public enum SpawnMeteor { Horizontal, BackGround, RightToBack }

        [Header("Meteor")]
        [SerializeField] float _speed;
        [SerializeField] GameObject _model;
        [SerializeField] List<GameObject> _trails;

        [Header("Audio")]
        [SerializeField] AudioSource _meteorAudio;
        [SerializeField] AudioSource _impactAudio;

        [Header("Test")]
        [SerializeField] bool _isTest;
        [SerializeField] SpawnMeteor _testSpawn;
        public Vector3 testStart;
        public Vector3 testEnd;

        float _curSpeed;
        Vector3 _startPos, _endPos, _outPos;
        bool _isModelVisible, _isRemoving;

        void OnEnable()
        {
            _isRemoving = false;
            _curSpeed = _speed;
            transform.localScale = Vector3.one;

            StartCoroutine(AudioUtil.FadeIn(_meteorAudio, 1.5f));
            SetRandomMeteorBehaviour();
            ShowModel();

            var direction = -(_startPos - _endPos).normalized;
            _outPos = _endPos + direction * 500f;
        }

        void Update()
        {
            if (_speed == 0 || !_isModelVisible)
                return;

            var step = _curSpeed * Time.deltaTime;
            var pos = _isRemoving ? _outPos : _endPos;

            transform.position = Vector3.MoveTowards(transform.position, pos, step);

            var targetDirection = (pos - transform.position).normalized;
            var direction = Vector3.RotateTowards(transform.forward, targetDirection, step, 0.0f);

            transform.rotation = Quaternion.LookRotation(direction);

            if (!_isRemoving && transform.position.z > 75)
            {
                _isRemoving = true;
                StartCoroutine(KeepMeteorAlive());
            }

            if (!_isRemoving && Vector3.Distance(transform.position, pos) < .1f)
                StartCoroutine(RemoveMeteor());
        }

        void ShowModel(bool show = true)
        {
            if (_model != null)
            {
                _isModelVisible = show;

                foreach (var rend in _model.GetComponentsInChildren<Renderer>())
                    rend.enabled = show;
            }
        }

        void SetRandomMeteorBehaviour()
        {
            if (_isTest && testStart != Vector3.zero && testEnd != Vector3.zero)
            {
                _startPos = testStart;
                _endPos = testEnd;
            }
            else
            {
                var spawn = RandomEnumUtil<SpawnMeteor>.Get();
                Utils.SetGameObjectLayer(gameObject, spawn == SpawnMeteor.BackGround
                    ? Utils.OjectLayer.Background
                    : Utils.OjectLayer.Default);

                switch (spawn)
                {
                    case SpawnMeteor.Horizontal:
                        SpawnHorizontal(out _startPos, out _endPos);
                        break;
                    case SpawnMeteor.RightToBack:
                        SpawnRightToBack(out _startPos, out _endPos);
                        break;
                    case SpawnMeteor.BackGround:
                    default:
                        SpawnBackground(out _startPos, out _endPos);
                        break;
                }
            }
            transform.position = _startPos;
        }

        void SpawnRightToBack(out Vector3 start, out Vector3 end)
        {
            float xPos = Random.Range(10f, 20f);
            float yPos = Random.Range(-4f, 8f);
            float zPos = -30f;
            start = new Vector3(xPos, yPos, zPos);

            xPos = Random.Range(-40f, 10f);
            yPos = Random.Range(-15f, 15f);
            zPos = 80f;
            end = new Vector3(xPos, yPos, zPos);
            print("RIGHT");

        }

        void SpawnHorizontal(out Vector3 start, out Vector3 end)
        {
            float xPos = -80f;
            float yPos = Random.Range(-4f, 8f);
            float zPos = -30f;
            start = new Vector3(xPos, yPos, zPos);

            xPos = 80f;
            yPos = Random.Range(-15f, 15f);
            zPos = 80f;
            end = new Vector3(xPos, yPos, zPos);

            if (Random.value > 0.5f)
            {
                SwapVectors(ref start, ref end);

                print("HORIZONTAL Reverse");
            }
            else
                print("HORIZONTAL");

        }

        void SpawnBackground(out Vector3 start, out Vector3 end)
        {
            float xPos = -60;
            float yPos = Random.Range(-10f, 12f);
            float zPos = Random.Range(10f, 50f);
            start = new Vector3(xPos, yPos, zPos);

            xPos = 60f;
            yPos = Random.Range(-10f, 12f);
            zPos = Random.Range(10f, 50f);
            end = new Vector3(xPos, yPos, zPos);

            if (Random.value > 0.5f)
            {
                print("BACKGROUND Reverse");
                SwapVectors(ref start, ref end);
            }
            else
                print("BACKGROUND");

        }

        void SwapVectors(ref Vector3 v1, ref Vector3 v2) => (v2, v1) = (v1, v2);

        void OnCollisionEnter(Collision collision)
        {
            if (collision == null) return;
            if (!collision.collider.name.Contains("detonator", System.StringComparison.InvariantCultureIgnoreCase))
                return;

            _curSpeed = 0;
            _meteorAudio.Stop();

            ShowModel(false);

            var contact = collision.contacts[0];
            var pos = contact.point;

            print("boem: " + collision.collider.name + " " + pos);
            AsteroidsGameManager.GmManager.PlayEffect(EffectsManager.Effect.ExplosionSmall, pos, 1, Utils.OjectLayer.Default);
            _impactAudio.Play();

            StartCoroutine(RemoveMeteor());
        }

        IEnumerator KeepMeteorAlive()
        {
            _curSpeed = 50f;
            yield return new WaitForSeconds(5f);
            StartCoroutine(RemoveMeteor());
        }

        IEnumerator RemoveMeteor()
        {
            if (_trails.Count > 0)
            {
                for (int i = 0; i < _trails.Count; i++)
                {
                    if (_trails[i].TryGetComponent<ParticleSystem>(out var ps))
                        ps.Stop();

                    yield return null;
                }
            }

            while (_impactAudio.isPlaying)
                yield return null;

            RemoveFromGame();
        }
    }
}