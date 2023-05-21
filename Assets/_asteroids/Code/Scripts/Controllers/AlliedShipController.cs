using MoonsOfMars.Shared;
using System.Collections;
using UnityEngine;

namespace MoonsOfMars.Game.Asteroids
{
    using static GameManager;

    public class AlliedShipController : GameBase
    {
        #region editor fields
        [SerializeField] ThrustController thrustController;
        [SerializeField] AudioSource spawnAudio;
        [SerializeField] AudioClip[] spawnClips;
        [Header("Animation")] //TODO: Separate to RocketAnimationsController
        [SerializeField] bool isPlayer;
        [SerializeField] float duration = 5f;
        [SerializeField] AudioClip endStageClip;
        [SerializeField] GameObject model;
        #endregion

        bool _skipUpdate;
        bool _isShipRemoved;        // Prevent ship remove recursion
        bool _isPackageEjected;     // Prevent ejecing multiple packages

        Vector3 _oldPos;            // used to determine direction
        Vector3 _targetPos;

        void OnEnable()
        {
            ShowModel();

            _skipUpdate = false;
            _isShipRemoved = false;
            _isPackageEjected = false;

            _oldPos = new();
            if (!isPlayer)
            {
                PlaySpawnClip(duration);
                MoveShipIn(duration);
            }
        }

        protected override void FixedUpdate()
        {
            if (_skipUpdate) return;

            Vector3 direction = (transform.position - _oldPos).normalized;

            if (direction != Vector3.zero || _oldPos == Vector3.zero)
                transform.rotation = Quaternion.LookRotation(direction);

            if (isPlayer) return;

            if (!_isPackageEjected && transform.position.z > -.5f && transform.position.z < .5f)
                EjectPackage();

            if (!_isShipRemoved && Vector3.Distance(transform.position, _targetPos) < .1f)
                RemoveShip(duration);
        }

        void LateUpdate() => _oldPos = transform.position;

        public void PlayerShipJumpOut(float duration)
        {
            _skipUpdate = true;
            MoveShipIn(duration);
        }

        public void PlayerShipJumpIn(float duration)
        {
            _skipUpdate = true;
            _oldPos = Vector3.zero;
            var pos = ManagerGame.GetWorldJumpPosition();
            pos.z = -1;

            transform.SetPositionAndRotation(
                new Vector3(pos.x, pos.y, transform.position.z),
                Quaternion.Euler(0, 0, 0));

            LeanTween.move(gameObject, pos, duration)
                .setEaseInQuad()
                .setOnComplete(() => RemoveShip());

            AnimateThrust(duration, false, LeanTweenType.easeInQuint);
            PlaySpawnClip(duration);
        }

        public void PlayerShipStageCompleteAnimation()
        {
            duration = 5;
            transform.localScale = transform.localScale * .1f;
            transform.localPosition = ManagerGame.m_BackgroundCamera.transform.position;

            ManagerGame.SwitchStageCam(StageCamera.end);
            StartCoroutine(FollowPath(duration));
        }

        IEnumerator FollowPath(float duration)
        {
            float t = 0;
            float speedModifier = 1 / duration;
            var path = ManagerLevel.GetStageCompletePath();

            ManagerGame.PlayEffect(EffectsManager.Effect.Teleport, path[0], .1f, Utils.OjectLayer.Background);
            PlaySpawnClip(duration);
            StartCoroutine(IncreaseThrust(.1f));

            while (t < 1)
            {
                t += Time.deltaTime * speedModifier;

                transform.position = Mathf.Pow(1 - t, 3) * path[0] + 3 * Mathf.Pow(1 - t, 2) * t * path[1] + 3 * (1 - t) * Mathf.Pow(t, 2) * path[2] + Mathf.Pow(t, 3) * path[3];
                yield return new WaitForEndOfFrame();
            }

            ManagerGame.SwitchStageCam(StageCamera.start);
            while (!ManagerGame.IsStageStartCameraActive())
                yield return null;

            AnimateThrust(3, false, LeanTweenType.easeInCubic);
            yield return new WaitForSeconds(.5f); // Custom blend time cinemachine brain

            // Move ship from bottom left to upper right (duraion: 2 seconds)
            var p = ManagerGame.m_StageStartCamera.transform.position;
            var startPos = new Vector3(p.x + 4, p.y - 2, p.z + 3);
            var endPos = new Vector3(p.x - 3f, p.y + 1.5f, p.z - 7);

            transform.position = startPos;
            LeanTween.move(gameObject, endPos, 2)
                .setOnComplete(() => StartCoroutine(PlayerShipEndStageComplete(endPos)));
        }

        IEnumerator PlayerShipEndStageComplete(Vector3 pos)
        {
            HideModel();
            PlayStageEndClip();
            ManagerGame.PlayEffect(EffectsManager.Effect.HyperJump, pos, 1, Utils.OjectLayer.Background);
            while (spawnAudio.isPlaying)
                yield return null;

            RemoveFromGame();
            transform.localScale = transform.localScale * 10f;

            // Start loading new stage
            ManagerLevel.LoadNewStage();
        }

        IEnumerator IncreaseThrust(float delay)
        {
            yield return new WaitForSeconds(delay);
            AnimateThrust(1, true);
        }

        void MoveShipIn(float duration)
        {
            LeanTween.move(gameObject, CreatePath(), duration)
                .setEaseOutQuad()
                .setOrientToPath(true);

            AnimateThrust(duration, false);
        }

        void AnimateThrust(float duration, bool increase = true, LeanTweenType type = default)
        {
            if (type == default)
                type = isPlayer ? LeanTweenType.easeInOutQuint : LeanTweenType.easeInQuint;

            float start = increase ? 0 : 1;
            float end = increase ? 1 : 0;

            if (thrustController != null)
            {
                LeanTween.value(gameObject, start, end, duration)
                    .setOnUpdate((float val) => thrustController.SetThrust(val))
                    .setEase(type);
            }
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

        void PlayStageEndClip()
        {
            spawnAudio.volume = .8f;
            spawnAudio.PlayOneShot(endStageClip);
        }

        void RemoveShip(float duration = 0)
        {
            _isShipRemoved = true;
            RemoveFromGame(duration);
        }

        void EjectPackage()
        {
            _isPackageEjected = true;
            ManagerPowerup.SpawnPowerup(transform.position);
        }

        LTBezierPath CreatePath()
        {
            var path = new Vector3[4];

            // first position, spawn
            _oldPos = new Vector3(Random.Range(-50f, 50f), Random.Range(-20f, 20f), 100f);
            transform.position = _oldPos;
            path[0] = _oldPos;

            // second position, bring within game cam view
            float x = Random.Range(-15f, 15f);
            path[1] = new Vector3(x, Random.Range(-8f, 8f), 0);

            // third position
            if (x < 0)
                x += 20;
            else
                x -= 20;

            path[2] = new Vector3(x, Random.Range(-8f, 8f), 0);

            // last position
            _targetPos = new Vector3(Random.Range(-10f, 10f), Random.Range(-6f, 6f), -31f);
            path[3] = _targetPos;

            return new LTBezierPath(path);
        }

        void HideModel() => ShowModel(false);

        void ShowModel(bool show = true)
        {
            if (model != null)
            {
                foreach (var rend in model.GetComponentsInChildren<Renderer>())
                    rend.enabled = show;
            }
        }

    }
}
