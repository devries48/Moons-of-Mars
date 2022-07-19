using UnityEngine;
using UnityEngine.Pool;

namespace Game.Astroids
{
    public class PrefabObjectPool
    {
         GameObject _prefab;
         ObjectPool<GameObject> _pool;

        public static PrefabObjectPool Build(GameObject prefab, int initialCapacity, int maxCapacity = 1000)
        {
            var objPool = new PrefabObjectPool
            {
                _prefab = prefab
            };

            objPool._pool = new ObjectPool<GameObject>(
                            objPool.CreatePooledItem,
                            objPool.OnTakeFromPool,
                            objPool.OnReturnedToPool,
                            objPool.OnDestroyPoolObject,
                            defaultCapacity: initialCapacity,
                            maxSize: maxCapacity);

            return objPool;
        }

        public GameObject GetFromPool()
        {
            return _pool.Get();
        }

        public void ReturnToPool(GameObject obj)
        {
            _pool.Release(obj);
        }

        GameObject CreatePooledItem()
        {
            var obj = Object.Instantiate(_prefab);

            return obj;
        }

        void OnTakeFromPool(GameObject obj)
        {
            obj.SetActive(true);
        }

        void OnReturnedToPool(GameObject obj)
        {
            obj.SetActive(false);
        }

        void OnDestroyPoolObject(GameObject obj)
        {
            Debug.LogWarning("Pooled object is requested to be destroyed!");
            obj.SetActive(false);
        }


    }

}