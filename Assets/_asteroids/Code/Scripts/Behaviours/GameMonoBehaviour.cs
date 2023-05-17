using MoonsOfMars.Shared;
using System.Collections;
using UnityEngine;


namespace MoonsOfMars.Game.Asteroids
{
    using static AsteroidsGameManager;

    public class GameMonoBehaviour : PoolableBase
    {
        #region properties

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

        protected virtual void FixedUpdate()
        {
            if (!m_ScreenWrap)
                return;

            var pos = transform.position;
            var offset = transform.localScale / 2;
            var bounds = GmManager.m_camBounds;

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

        protected void PlayEffect(EffectsManager.Effect effect, Vector3 position, float scale = 1f)
            => GmManager.PlayEffect(effect, position, scale);

        protected virtual void OnDisable() => CancelInvokeRemoveFromGame();

        public void InvokeRemoveFromGame(float time) => Invoke(nameof(RemoveFromGame), time);

        public void CancelInvokeRemoveFromGame() => CancelInvoke(nameof(RemoveFromGame));

        public void RemoveFromGame(float t) => StartCoroutine(RemoveFromGameCore(t));

        IEnumerator RemoveFromGameCore(float t)
        {
            yield return new WaitForSeconds(t);

            RemoveFromGame();
        }
    }
}