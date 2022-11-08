using System.Collections;
using UnityEngine;

namespace Game.Astroids
{
    public class GameMonoBehaviour : PoolableMonoBehaviour
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

        protected AsteroidsGameManager GameManager
        {
            get
            {
                if (__gameManager == null)
                    __gameManager = AsteroidsGameManager.Instance;

                if (__gameManager == null)
                    Debug.LogWarning("GameManager is null");

                return __gameManager;
            }
        }
        AsteroidsGameManager __gameManager;

        #endregion

        protected void Score(int score, GameObject target) => Astroids.Score.Earn(score, target);

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
            => GameManager.PlayEffect(effect, position, scale);

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