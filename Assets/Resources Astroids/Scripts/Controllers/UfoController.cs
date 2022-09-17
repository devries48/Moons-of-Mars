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
        GameObject pivot;

        [SerializeField]
        float speed = 10f;

        [SerializeField]
        float rotationSpeed = 50f;

        [SerializeField, Range(0, 200)]
        int destructionScore = 100;

        [SerializeField]
        AudioSource engineAudio;

        [Header("UFO Lights")]

        [SerializeField]
        GameObject lightsModel;

        #endregion

        #region fields

        Vector3 _targetPos;  // Ufo target position
        bool _isShipRemoved; // Prevent ship remove recursion

        #endregion

        protected override void Awake()
        {
            m_isEnemy = true;

            base.Awake();
        }

        protected override void OnEnable()
        {
            _isShipRemoved = false;

            if (engineAudio != null)
                FadeInEngine(.5f);

            SetRandomShipBehaviour();
            ShowLights();

            base.OnEnable();
        }

        protected override void OnDisable()
        {
            CancelInvoke(nameof(FireRandomDirection));
            LeanTween.cancel(pivot);
            base.OnDisable();
        }

        void FixedUpdate()
        {
            if (!m_isAlive || _isShipRemoved) return;
            
            SpinUfo();
            MoveUfo();
        }

        protected override void HitByBullet(GameObject obj)
        {
            HideLights();
            base.HitByBullet(obj);
            Score(destructionScore);
        }

        void SetRandomShipBehaviour()
        {
            var side = RandomEnumUtil<SpawnSide>.Get();

            LeanTween.rotateX(pivot, -10f, 1f).setFrom(10f).setLoopPingPong(5);

            Rb.transform.position = SpawnPoint(side == SpawnSide.left);
            _targetPos = SpawnPoint(side != SpawnSide.left);

            InvokeRepeating(nameof(FireRandomDirection), fireRate, fireRate);
        }

        void MoveUfo()
        {
            //linear movement
            var step = speed * Time.fixedDeltaTime;

            Rb.transform.position = Vector3.MoveTowards(Rb.transform.position, _targetPos, step);

            if (Vector3.Distance(Rb.transform.position, _targetPos) < 0.1f)
                RemoveShip();
        }

        void SpinUfo()
        {
            Rb.transform.Rotate(new Vector3(0, rotationSpeed * speed * Time.fixedDeltaTime, 0));
        }

        void RemoveShip()
        {
            _isShipRemoved = true;
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

            GameManager.UfoDestroyed();
            RemoveFromGame();
        }

        Vector3 SpawnPoint(bool left)
        {
            if (GameManager == null)
                return Vector3.zero;

            var xPos = (left)
                 ? GameManager.m_camBounds.LeftEdge - 1
                 : GameManager.m_camBounds.RightEdge + 1;

            var yPos = Random.Range(
                GameManager.m_camBounds.TopEdge - 1,
                GameManager.m_camBounds.BottomEdge + 1);

            return new Vector3(xPos, yPos);
        }

        void ShowLights(bool show = true)
        {
            if (lightsModel != null)
                lightsModel.SetActive(show);
        }

        void HideLights() => ShowLights(false);
    }
}