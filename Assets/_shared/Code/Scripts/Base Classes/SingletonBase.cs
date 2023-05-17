using UnityEngine;

namespace MoonsOfMars.Shared
{
    public class SingletonBase<T> : MonoBehaviour where T : MonoBehaviour
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

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (Instance != null)
            {
                if (Instance == this && _dontDestroyOnLoad)
                    DontDestroyOnLoad(transform.root.gameObject);
            }
            if (Instance == this) return;

            Debug.LogError("Singleton Error");
            Destroy(gameObject);
        }
    }
}