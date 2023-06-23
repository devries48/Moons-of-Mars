using MoonsOfMars.Shared;
using UnityEngine;
using static MoonsOfMars.Shared.EffectsData;

namespace MoonsOfMars.Game.Asteroids
{
    //using static GameManager;

    /// <summary>
    /// Base class for poolable gameobjects.
    /// </summary>
    /// <remarks>
    /// - GameManager access: GameManager property
    /// - Screen wrapping: M_ScreenWrap field
    /// - Play audio clips: PlaySound()
    /// - Play effect: PlayEffect()
    /// - Add score: Score()
    /// </remarks>
    public class GameBase : PoolableBase
    {
        #region properties

        protected GameManager ManagerGame => GameManager.GmManager;
        protected LevelManager ManagerLevel => ManagerGame.m_LevelManager;
        protected InputManager ManagerInput => ManagerGame.InputManager;
        protected PowerupManagerData ManagerPowerup => ManagerGame.PowerupManager;
        protected HudManager ManagerHud => ManagerGame.m_HudManager;

        protected AudioSource Audio
        {
            get
            {
                if (__audio == null)
                    TryGetComponent(out __audio);

                return __audio;
            }
        }
        AudioSource __audio;
        #endregion

        internal bool m_ScreenWrap;

        protected virtual void OnDisable() => CancelInvokeRemoveFromGame();

        protected virtual void FixedUpdate()
        {
            if (!m_ScreenWrap)
                return;

            var pos = transform.position;
            var offset = transform.localScale / 2;
            var bounds = ManagerGame.m_camBounds;

            if (pos.x > bounds.RightEdge + offset.x)
                transform.position = new Vector2(bounds.LeftEdge - offset.x, pos.y);

            if (pos.x < bounds.LeftEdge - offset.x)
                transform.position = new Vector2(bounds.RightEdge + offset.x, pos.y);

            if (pos.y > bounds.TopEdge + offset.y)
                transform.position = new Vector2(pos.x, bounds.BottomEdge - offset.y);

            if (pos.y < bounds.BottomEdge - offset.y)
                transform.position = new Vector2(pos.x, bounds.TopEdge + offset.y);
        }

        protected void Score(int score, GameObject target) => Asteroids.Score.Earn(score, target);

        protected void PlaySound(AudioClip clip, AudioSource audioSource = null)
        {
            if (clip == null)
                return;

            if (audioSource != null)
                audioSource.PlayOneShot(clip);
            else if (Audio != null)
                Audio.PlayOneShot(clip);
        }

        protected void PlayEffect(Effect effect, Vector3 position, float scale = 1f) => ManagerGame.PlayEffect(effect, position, scale);

    }
}