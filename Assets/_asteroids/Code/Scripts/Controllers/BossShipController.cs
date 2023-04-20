using MoonsOfMars.Game.Asteroids;
using MoonsOfMars.Shared;
using UnityEngine;

public class BossShipController : GameMonoBehaviour
{
    [SerializeField] Transform _ship;
    [SerializeField] float _spinSpeed = 100f;

    void OnEnable()
    {
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

        LeanTween.move(gameObject, path, duration)
            .setEase(curve)
            .setOrientToPath(true)
            .setOnComplete(() =>
            {
                if (reverse)
                    AsteroidsGameManager.GmManager.PlayEffect(EffectsManager.Effect.hit2, transform.position, 3f, Utils.OjectLayer.Default);
               
                RemoveFromGame();
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
        var _targetPos = new Vector3(Random.Range(-10f, 10f), Random.Range(-6f, 6f), -31f);
        path[3] = _targetPos;

        if (isReverse)
            System.Array.Reverse(path);

        return new LTBezierPath(path);
    }


}
