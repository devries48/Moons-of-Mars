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

        protected AstroidsGameManager GameManager
        {
            get
            {
                if (__gameManager == null)
                    __gameManager = AstroidsGameManager.Instance;

                return __gameManager;
            }
        }
        AstroidsGameManager __gameManager;

        #endregion


        protected void Score(int score) => Astroids.Score.Earn(score);

        protected void PlaySound(AudioClip clip)
        {
            if (clip == null || Audio == null)
                return;

            Audio.PlayOneShot(clip);
        }

        protected void PlayEffect(EffectsManager.Effect effect, Vector3 position, float scale = 1f)
        {
            GameManager.PlayEffect(effect, position, scale);
        }

        protected virtual void OnDisable() => CancelInvokeRemoveFromGame();

        protected virtual void RequestDestruction() => RequestDefaultDestruction(gameObject);

        static void RequestDefaultDestruction(GameObject gameObject) => Destroy(gameObject);

        public void InvokeRemoveFromGame(float time) => Invoke(nameof(RemoveFromGame), time);

        public void CancelInvokeRemoveFromGame() => CancelInvoke(nameof(RemoveFromGame));

        public void RemoveFromGame()
        {
            if (IsPooled)
                ReturnToPool();
            else
                RequestDestruction();
        }

        public void RemoveFromGame(float t) => StartCoroutine(RemoveFromGameCore(t));

        IEnumerator RemoveFromGameCore(float t)
        {
            yield return new WaitForSeconds(t);

            RemoveFromGame();
        }

        public static void RemoveFromGame(GameObject victim)
        {
            GameMonoBehaviour handler = victim.GetComponent<GameMonoBehaviour>();

            if (handler)
                handler.RemoveFromGame();
            else
                RequestDefaultDestruction(victim);
        }

    }
}