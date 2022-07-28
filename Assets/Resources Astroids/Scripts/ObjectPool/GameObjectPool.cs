using UnityEngine;
using UnityEngine.Pool;

namespace Game.Astroids
{
    public class GameObjectPool
    {
        GameObject _prefab;
        ObjectPool<GameObject> _pool;

        public static GameObjectPool Build(GameObject prefab, int initialCapacity, int maxCapacity = 1000)
        {
            var objPool = new GameObjectPool { _prefab = prefab };

            objPool._pool = new ObjectPool<GameObject>(
                                    objPool.CreatePooledItem,
                                    objPool.OnTakeFromPool,
                                    objPool.OnReturnedToPool,
                                    objPool.OnDestroyPoolObject,
                                    defaultCapacity: initialCapacity,
                                    maxSize: maxCapacity);

            return objPool;
        }

        public GameObject GetFromPool() => _pool.Get();

        public GameObject GetFromPool(Vector3 position, Quaternion rotation = default, float scale = 0f)
        {
            var obj = _pool.Get();
            obj.transform.SetPositionAndRotation(position, rotation);

            if (scale != 0f)
                obj.transform.localScale *= scale;

            return obj;
        }

        public T GetComponentFromPool<T>(Vector3 position, Quaternion rotation) where T : GameMonoBehaviour
        {
            return GetFromPool(position, rotation).GetComponent<T>();
        }

        public void ReturnToPool(GameObject obj) => _pool.Release(obj);

        GameObject CreatePooledItem()
        {
            var obj = Object.Instantiate(_prefab);
            obj.TryGetComponent(out IPoolable instance);

            if (instance != null)
                instance.SetPool(this);

            return obj;
        }

        void OnTakeFromPool(GameObject obj) => obj.SetActive(true);

        void OnReturnedToPool(GameObject obj) => obj.SetActive(false);

        void OnDestroyPoolObject(GameObject obj) => obj.SetActive(false);

    }

    interface IPoolable
    {
        bool IsPooled { get; }
        void SetPool(GameObjectPool pool);
        void ReturnToPool();
    }
}