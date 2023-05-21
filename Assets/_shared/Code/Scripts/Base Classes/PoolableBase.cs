using System.Collections;
using UnityEngine;

namespace MoonsOfMars.Shared
{
    [DisallowMultipleComponent]
    public class PoolableBase : MonoBehaviour, IPoolable
    {
        public bool IsPooled => _pool != null;

        GameObjectPool _pool;

        public void ReturnToPool() => _pool.ReturnToPool(gameObject);
        public void ReturnToPool(GameObject obj) => _pool.ReturnToPool(obj);
        public void SetPool(GameObjectPool pool) => _pool = pool;

        public void RemoveFromGame()
        {
            if (IsPooled)
                ReturnToPool();
            else
                RequestDestruction();
        }

        protected virtual void RequestDestruction() => RequestDefaultDestruction(gameObject);

        public void InvokeRemoveFromGame(float time) => Invoke(nameof(RemoveFromGame), time);

        public void CancelInvokeRemoveFromGame() => CancelInvoke(nameof(RemoveFromGame));

        public void RemoveFromGame(float t) => StartCoroutine(RemoveFromGameCore(t));

        IEnumerator RemoveFromGameCore(float t)
        {
            yield return new WaitForSeconds(t);

            RemoveFromGame();
        }

        static void RequestDefaultDestruction(GameObject gameObject) => Destroy(gameObject);
        public static void RemoveFromGame(GameObject victim)
        {
            var handler = victim.GetComponent<PoolableBase>();

            if (handler)
                handler.RemoveFromGame();
            else
                RequestDefaultDestruction(victim);
        }

    }
}