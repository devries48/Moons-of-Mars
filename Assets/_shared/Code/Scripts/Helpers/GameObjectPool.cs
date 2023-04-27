using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

namespace MoonsOfMars.Shared
{
    public class GameObjectPool
    {
        public const string OBJECTPOOL_SCENE = "Object Pool";

        GameObject _prefab;
        ObjectPool<GameObject> _pool;
        Scene _poolScene;

        public static GameObjectPool Build(GameObject prefab, int initialCapacity, int maxCapacity = 1000, Scene poolScene = default)
        {
            var objPool = new GameObjectPool { _prefab = prefab, _poolScene = poolScene };

            objPool._pool = new ObjectPool<GameObject>(
                                    objPool.CreatePooledItem,
                                    objPool.OnTakeFromPool,
                                    objPool.OnReturnedToPool,
                                    objPool.OnDestroyPoolObject,
                                    defaultCapacity: initialCapacity,
                                    maxSize: maxCapacity);

            return objPool;
        }

        public int CountActive => _pool.CountActive;
        public GameObject GetFromPool() => _pool.Get();

        public GameObject GetFromPool(Vector3 position, Quaternion rotation = default, Vector3 size = default)
        {
            var obj = _pool.Get();
            obj.transform.SetPositionAndRotation(position, rotation);

            if (size != default)
                obj.transform.localScale = size;

            return obj;
        }

        public T GetComponentFromPool<T>(Vector3 position, Quaternion rotation) where T : Poolable_MonoBehaviour
        {
            return GetFromPool(position, rotation).GetComponent<T>();
        }

        public void ReturnToPool(GameObject obj)
        {
            if (obj.activeSelf)
                _pool.Release(obj);
        }

        GameObject CreatePooledItem()
        {
            var obj = Object.Instantiate(_prefab);
            var instance = obj.GetComponentInChildren<IPoolable>();

            if (instance != null)
            {
                instance.SetPool(this);

                if (_poolScene != default)
                    SceneManager.MoveGameObjectToScene(obj, _poolScene);
            }
            else
                Debug.LogError("Unable to find IPoolable interface: " + obj.ToString());

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
        void ReturnToPool(GameObject obj);
        void ReturnToPool();
    }

}
