using UnityEngine;

using static Game.Astroids.UfoManager;

namespace Game.Astroids
{
    [SelectionBase]
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

        #region properties
        protected UfoManager UfoManager
        {
            get
            {
                if (__ufoManager == null)
                    __ufoManager = GameManager.m_UfoManager;

                return __ufoManager;
            }
        }
        UfoManager __ufoManager;
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

            StartCoroutine(AudioUtil.FadeIn(engineAudio, .5f));
            SetRandomShipBehaviour();
            ShowLights();

            base.OnEnable();
        }

        protected override void OnDisable()
        {
            CancelFire();
            LeanTween.cancel(pivot, true);
            base.OnDisable();
        }

        protected override void FixedUpdate()
        {
            if (!m_isAlive || _isShipRemoved) return;

            SpinUfo();
            MoveUfo();
            base.FixedUpdate();
        }

        protected override void HitByBullet(GameObject obj)
        {
            HideLights();
            CancelFire();
            base.HitByBullet(obj);
            Score(UfoManager.GetDestructionScore(m_ufoType), gameObject);
        }

        protected override void HideModel()
        {
            if (engineAudio)
                engineAudio.Stop();

            LeanTween.cancel(pivot, true);
            base.HideModel();
        }

        protected override void RaisePowerUpShield()
        {
            m_Shield.AutoShieldUp(m_pwrShieldTime);
        }

        void SetRandomShipBehaviour()
        {
            m_ufoType = RandomEnumUtil<UfoType>.Get();
            m_shipType = m_ufoType == UfoType.green
                ? ShipType.ufoGreen
                : ShipType.ufoRed;

            engineAudio.clip = m_ufoType == UfoType.green
                ? UfoManager.m_GreenUfo.engineSound
                : UfoManager.m_RedUfo.engineSound;

            engineAudio.Play();

            UfoManager.SetUfoMaterials(this);
            pivot.transform.localRotation = Quaternion.identity;

            var side = RandomEnumUtil<SpawnSide>.Get();
            var maxPivot = m_ufoType == UfoType.green ? 10f : 3f;

            LeanTween.rotateX(pivot, -maxPivot, 1f).setFrom(maxPivot).setLoopPingPong();

            transform.position = SpawnPoint(side == SpawnSide.left);
            _targetPos = SpawnPoint(side != SpawnSide.left);

            InvokeRepeating(nameof(FireRandomDirection), fireRate, fireRate);
        }

        void MoveUfo()
        {
            if (GameManager.m_playerShip == null)
                return;

            if (m_ufoType == UfoType.red)
                _targetPos = GameManager.m_playerShip.transform.position;

            var step = speed * Time.fixedDeltaTime;
            transform.position = Vector3.MoveTowards(transform.position, _targetPos, step);

            if (m_ufoType == UfoType.green)
            {
                if (Vector3.Distance(transform.position, _targetPos) < 0.1f)
                    RemoveShip();
            }
        }

        void SpinUfo()
        {
            transform.Rotate(new Vector3(0, rotationSpeed * speed * Time.fixedDeltaTime, 0));
        }

        void RemoveShip()
        {
            CancelFire();
            GameManager.UfoDestroyed();
            _isShipRemoved = true;
            StartCoroutine(AudioUtil.FadeOut(engineAudio, 1, () => { RemoveFromGame(); }));
        }

        void FireRandomDirection()
        {
            if (AreShieldsUp)
                return;

            weapon.transform.Rotate(0, 0, Random.Range(0, 360));
            FireWeapon();
        }

        void CancelFire()
        {
            CancelInvoke(nameof(FireRandomDirection));
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