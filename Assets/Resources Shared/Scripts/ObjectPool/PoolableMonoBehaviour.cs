using Game.Asteroids;
using UnityEngine;

[DisallowMultipleComponent]
public class PoolableMonoBehaviour : MonoBehaviour, IPoolable
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

    static void RequestDefaultDestruction(GameObject gameObject) => Destroy(gameObject);
    public static void RemoveFromGame(GameObject victim)
    {
        var handler = victim.GetComponent<GameMonoBehaviour>();
      
        if (handler)
            handler.RemoveFromGame();
        else
            RequestDefaultDestruction(victim);
    }

}
