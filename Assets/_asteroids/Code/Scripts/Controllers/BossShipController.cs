using MoonsOfMars.Shared;
using System.Collections;
using UnityEngine;

namespace MoonsOfMars.Game.Asteroids
{
    public class BossShipController : SpaceShipMonoBehaviour
    {
        [Header("Boss")]
        [SerializeField] Transform _ship;
        [SerializeField] int _minSpinSpeed = 50;
        [SerializeField] int _maxSpinSpeed = 200;

        int _spinSpeed;

        protected override void OnEnable()
        {
            _spinSpeed = Random.Range(_minSpinSpeed, _maxSpinSpeed + 1);
            MoveShipIn(12f);
        }

        protected override void FixedUpdate()
        {
            _ship.Rotate(new Vector3(0, 0, _spinSpeed * Time.fixedDeltaTime));
        }

        void MoveShipIn(float duration)
        {
            var reverse = Random.value > 0.5f;
            var curve = reverse ? LeanTweenType.easeInCirc : LeanTweenType.easeOutQuad;
            var path = CreatePath(reverse);
            var tilt = Random.Range(-110f, -75f);

            transform.localRotation = Quaternion.Euler(tilt, 0, 0);
            base.OnEnable();
            LeanTween.move(gameObject, path, duration)
                .setEase(curve)
                .setOrientToPath(true)
                .setOnComplete(() =>
                {
                    if (reverse)
                    {
                        PlayAudioClip(SpaceShipSounds.Clip.warp);
                        GameManager.GmManager.PlayEffect(EffectsManager.Effect.HyperJump, transform.position, 3f, Utils.OjectLayer.Default);
                    }
                    if (gameObject.activeSelf)
                        StartCoroutine(RemoveBossShip());
                });
        }
        LTBezierPath CreatePath(bool isReverse)
        {
            var path = new Vector3[4];

            // first position, spawn
            var _oldPos = new Vector3(Random.Range(-50f, 50f), Random.Range(-20f, 20f), 100f);
            transform.position = _oldPos;
            path[0] = _oldPos;

            // second position, bring within game cam view
            float x = Random.Range(-14f, 14f);
            path[1] = new Vector3(x, Random.Range(-8f, 8f), 0);

            // third position
            if (x < 0)
                x += 20;
            else
                x -= 20;

            path[2] = new Vector3(x, Random.Range(-8f, 8f), 0);

            // last position
            var _targetPos = new Vector3(Random.Range(-9f, 9f), Random.Range(-5.5f, 5.5f), -29f);
            path[3] = _targetPos;

            if (isReverse)
                System.Array.Reverse(path);

            return new LTBezierPath(path);
        }

        IEnumerator RemoveBossShip()
        {
            while (sounds.IsAudioPlaying)
                yield return null;

            HideModel();
            transform.position = new Vector3(0, 0, -50);
            RemoveFromGame();
        }

    }
}