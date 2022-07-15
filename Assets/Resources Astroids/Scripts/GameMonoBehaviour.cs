using UnityEngine;

namespace Game.Astroids
{
    public class GameMonoBehaviour : MonoBehaviour, IPoolableAware, IRecyclable
    {
        Poolable poolable;

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

        void IPoolableAware.PoolableAwoke(Poolable p) 
        {
            poolable = p; 
        }

        void IRecyclable.Recycle() 
        {
            RemoveFromGame(); 
        }

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


        protected virtual void OnDisable() 
        { 
            CancelInvokeRemoveFromGame();
        }

        public void InvokeRemoveFromGame(float time) 
        { 
            Invoke(nameof(RemoveFromGame), time);
        }

        public void CancelInvokeRemoveFromGame() 
        { 
            CancelInvoke(nameof(RemoveFromGame)); 
        }

        public void RemoveFromGame()
        {
            if (poolable)
                poolable.Recycle();
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

        protected virtual void RequestDestruction() 
        {
            RequestDefaultDestruction(gameObject); 
        }

        static void RequestDefaultDestruction(GameObject gameObject) 
        {
            Destroy(gameObject); 
        }
    }
}