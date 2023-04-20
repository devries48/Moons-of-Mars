using UnityEngine;

namespace MoonsOfMars.Shared
{
    public class Singleton_MonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        [SerializeField] bool _dontDestroyOnLoad;
        static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance != null) return _instance;

                _instance = FindAnyObjectByType<T>();
                if (_instance != null) return _instance;

                var singleton = new GameObject(typeof(T).Name);
                _instance = singleton.AddComponent<T>();

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                if (_instance == this && _dontDestroyOnLoad)
                    DontDestroyOnLoad(transform.root.gameObject);
            }
            if (_instance == this) return;

            Destroy(gameObject);
        }
    }
}