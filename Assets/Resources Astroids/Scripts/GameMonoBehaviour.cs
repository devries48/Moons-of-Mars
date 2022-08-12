using UnityEngine;

namespace Game.Astroids
{
    [DisallowMultipleComponent]
    public class GameMonoBehaviour : MonoBehaviour, IPoolable
    {
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

        public bool IsPooled => _pool != null;

        GameObjectPool _pool;

        protected void Score(int score)
        {
            Astroids.Score.Earn(score);
        }

        protected void PlaySound(AudioClip clip)
        {
            if (clip == null || Audio == null)
                return;

            Audio.PlayOneShot(clip);
        }

        protected void PlayEffect(EffectsManager.Effect effect, Vector3 position, float scale = 1f)
        {
            AstroidsGameManager.Instance.PlayEffect(effect, position, scale);
        }

        protected virtual void OnDisable() => CancelInvokeRemoveFromGame();

        protected virtual void RequestDestruction() => RequestDefaultDestruction(gameObject);

        static void RequestDefaultDestruction(GameObject gameObject) => Destroy(gameObject);

        public void InvokeRemoveFromGame(float time) => Invoke(nameof(RemoveFromGame), time);

        public void CancelInvokeRemoveFromGame() => CancelInvoke(nameof(RemoveFromGame));

        public void RemoveFromGame()
        {
            if (IsPooled)
                _pool.ReturnToPool(gameObject);
            else
                RequestDestruction();
        }

        public static void RemoveFromGame(GameObject victim)
        {
            GameMonoBehaviour handler = victim.GetComponent<GameMonoBehaviour>();

            if (handler)
                handler.RemoveFromGame();
            else
                RequestDefaultDestruction(victim);
        }

        public void SetPool(GameObjectPool pool) => _pool = pool;

        public void ReturnToPool() => _pool.ReturnToPool(gameObject);
    }
}