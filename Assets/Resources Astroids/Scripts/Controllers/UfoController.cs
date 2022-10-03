using System.Collections;
using UnityEngine;
using static Game.Astroids.UfoManager;

namespace Game.Astroids
{
    public class UfoController : SpaceShipMonoBehaviour
    {
        #region editor fields
        [Header("UFO")]
        [SerializeField] GameObject pivot;
        [SerializeField] float speed = 10f;
        [SerializeField] float rotationSpeed = 50f;
        [SerializeField] AudioSource engineAudio;

        [Header("UFO Lights")]
        public GameObject m_LightsModel;
        #endregion

        #region fields
        internal UfoType m_ufoType = UfoType.green;

        Vector3 _targetPos;  // Ufo target position
        bool _isShipRemoved; // Prevent ship remove recursion
        #endregion

        enum SpawnSide { left, right }

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
            LeanTween.cancel(pivot, true);
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
            print("ufo hit by bullet: "+ obj.tag);
            HideLights();
            base.HitByBullet(obj);
            Score(GameManager.m_ufoManager.GetDestructionScore(m_ufoType));
        }

        protected override void HideModel()
        {
            if (engineAudio)
                engineAudio.Stop();

            LeanTween.cancel(pivot, true);
            base.HideModel();
        }

        void SetRandomShipBehaviour()
        {
            m_ufoType = RandomEnumUtil<UfoType>.Get();
            m_shipType = m_ufoType == UfoType.green ? ShipType.ufoGreen : ShipType.ufoRed;
           
            GameManager.m_ufoManager.SetUfoMaterials(this);
            pivot.transform.localRotation = Quaternion.identity;

            var side = RandomEnumUtil<SpawnSide>.Get();
            var maxPivot = m_ufoType == UfoType.green ? 10f : 3f;
            
            LeanTween.rotateX(pivot, -maxPivot, 1f).setFrom(maxPivot).setLoopPingPong();

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

            print("vuur van ufo");
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
            {
                Debug.Log("GameManager == null");
                return Vector3.zero;
            }

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
            if (m_LightsModel != null)
                m_LightsModel.SetActive(show);
        }

        void HideLights() => ShowLights(false);
    }
}