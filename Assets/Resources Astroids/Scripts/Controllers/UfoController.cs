using System.Collections;
using UnityEngine;

namespace Game.Astroids
{
    public class UfoController : SpaceShipMonoBehaviour
    {
        enum SpawnSide
        {
            left,
            right
        }

        #region editor fields

        [Header("UFO")]

        [SerializeField]
        float speed = 10f;

        [SerializeField]
        float rotationSpeed = 50f;

        [SerializeField, Range(0, 200)]
        int destructionScore = 100;

        [SerializeField]
        AudioSource engineAudio;

        [Header("UFO Model")]

        [SerializeField]
        GameObject bodyModel;

        [SerializeField]
        GameObject cockpitModel;

        [SerializeField]
        GameObject lightsModel;

        #endregion

        #region fields

        AstroidsGameManager _gameManager;
        Vector3 _targetPos; // Ufo target position
        bool _remove;
        Renderer _rndBody;
        Renderer _rndCockpit;

        #endregion

        protected override void Awake()
        {
            _gameManager = AstroidsGameManager.Instance;
            m_isEnemy = true;

            base.Awake();
        }

        protected override void OnEnable()
        {
            _remove = false;

            if (engineAudio != null)
                FadeInEngine(.5f);

            SetRandomUfoBehaviour();
            ShowModel();
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            CancelInvoke(nameof(FireRandomDirection));
            base.OnDisable();
        }

        void FixedUpdate()
        {
            if (!m_isAlive) return;

            SpinUfo();
            MoveUfo();
        }

        protected override void HitByBullet(GameObject bullet)
        {
            HideModel();
            base.HitByBullet(bullet);
            Score(destructionScore);
        }

        void SetRandomUfoBehaviour()
        {
            var side = RandomEnumUtil<SpawnSide>.Get();
            var tilt = Random.Range(-15f, 60f);

            Rb.transform.position = SpawnPoint(side == SpawnSide.left);
            Rb.transform.localRotation= Quaternion.AngleAxis(tilt, Vector3.right);
            _targetPos = SpawnPoint(side != SpawnSide.left);

            InvokeRepeating(nameof(FireRandomDirection), fireRate, fireRate);
        }

        void MoveUfo()
        {
            //linear movement
            var step = speed * Time.fixedDeltaTime;

            Rb.transform.position = Vector3.MoveTowards(Rb.transform.position, _targetPos, step);

            if (!_remove && Vector3.Distance(Rb.transform.position, _targetPos) < 0.1f)
                RemoveUfo();
        }

        void SpinUfo()
        {
            Rb.transform.Rotate(new Vector3(0, rotationSpeed * speed * Time.fixedDeltaTime, 0));
        }

        void RemoveUfo()
        {
            _remove = true;
            FadeOutEngine(1f);
        }

        void FireRandomDirection()
        {
            if (AreShieldsUp)
                return;

            weapon.transform.Rotate(0, 0, Random.Range(0, 360));
            FireWeapon();
        }

        void FadeInEngine(float FadeTime)
        {
            StartCoroutine(FadeInCore(FadeTime));
        }

        void FadeOutEngine(float FadeTime)
        {
            StartCoroutine(FadeOutCore(FadeTime));
        }

        IEnumerator FadeInCore(float fadeTime)
        {
            float finalVolume = engineAudio.volume;
            float deltaVolume = 0.1f;

            engineAudio.volume = 0;
            engineAudio.Play();

            while (engineAudio.volume < finalVolume)
            {
                engineAudio.volume += deltaVolume * Time.deltaTime / fadeTime;
                yield return null;
            }
            engineAudio.volume = finalVolume;
        }

        IEnumerator FadeOutCore(float fadeTime)
        {
            float startVolume = engineAudio.volume;

            while (engineAudio.volume > 0f)
            {
                var tmp = engineAudio.volume;
                engineAudio.volume = tmp - (startVolume * Time.deltaTime / fadeTime);
                yield return new WaitForEndOfFrame();
            }

            engineAudio.Stop();
            engineAudio.volume = startVolume;

            _gameManager.UfoDestroyed();

            RemoveFromGame();
        }

        Vector3 SpawnPoint(bool left)
        {
            var xPos = (left)
                 ? _gameManager.m_camBounds.LeftEdge - 1
                 : _gameManager.m_camBounds.RightEdge + 1;

            var yPos = Random.Range(
                _gameManager.m_camBounds.TopEdge - 1,
                _gameManager.m_camBounds.BottomEdge + 1);

            return new Vector3(xPos, yPos);
        }

        void ShowModel(bool show = true)
        {
            if (_rndBody == null && bodyModel != null)
                bodyModel.TryGetComponent(out _rndBody);

            if (_rndCockpit == null && cockpitModel != null)
                cockpitModel.TryGetComponent(out _rndCockpit);

            if (_rndBody != null)
                _rndBody.enabled = show;

            if (_rndCockpit != null)
                _rndCockpit.enabled = show;

            if (lightsModel != null)
                lightsModel.SetActive(show);
        }

        void HideModel() => ShowModel(false);

    }
}