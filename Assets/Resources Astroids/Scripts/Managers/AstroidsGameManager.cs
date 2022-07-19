using UnityEngine;

namespace Game.Astroids
{
    [RequireComponent(typeof(EffectsManager))]
    public class AstroidsGameManager : MonoBehaviour
    {
        /// <summary>
        /// Singleton GameManager
        /// </summary>
        public static AstroidsGameManager Instance => __instance;
        static AstroidsGameManager __instance;

        [SerializeField, Tooltip("Select a spaceship prefab")]
        GameObject rocket;

        [SerializeField, Tooltip("Select an astroid prefab")]
        GameObject asteroid;

        [SerializeField, Tooltip("Starting number of astroids")]
        int numberAstroids = 2;

        [SerializeField]
        Camera gameCamera;

        int _asteroidLife;
        CamBounds _camBounds;
        EffectsManager _effects;

        void Awake()
        {
            SingletonInstanceGuard();

            _camBounds = new CamBounds(gameCamera);
            TryGetComponent(out _effects);
        }
        private void Start()
        {
            Camera.SetupCurrent(gameCamera);
            CreateAsteroids(numberAstroids);
        }

        void OnEnable() 
        { 
            __instance = this; 
        }

        void Update()
        {
            if (_asteroidLife <= 0)
            {
                _asteroidLife = 6;
                CreateAsteroids(1);
            }
        }

        void SingletonInstanceGuard()
        {
            if (__instance == null)
            {
                __instance = this;
                DontDestroyOnLoad(transform.root.gameObject);
            }
            else
            {
                Destroy(gameObject);
                throw new System.Exception("Only one instance is allowed");
            }
        }

        void CreateAsteroids(float asteroidsNum)
        {
            for (int i = 1; i <= asteroidsNum; i++)
            {
                var AsteroidClone = Instantiate(asteroid, new Vector3(Random.Range(-20, 20), 10f), transform.rotation);
                AsteroidClone.GetComponent<AsteroidController>().SetGeneration(1);
                AsteroidClone.SetActive(true);
            }
        }

        //public static Vector3 WorldPos(Vector3 screenPos)
        //{
        //    return Camera.current.ScreenToWorldPoint(screenPos);
        //}

        public void RePosition(GameObject obj)
        {
            var pos = obj.transform.position;
            var offset = obj.transform.localScale / 2;

            if (pos.x > _camBounds.RightEdge + offset.x)
                obj.transform.position = new Vector2(_camBounds.LeftEdge - offset.x, pos.y);

            if (pos.x < _camBounds.LeftEdge - offset.x)
                obj.transform.position = new Vector2(_camBounds.RightEdge + offset.x, pos.y);

            if (pos.y > _camBounds.TopEdge + offset.y)
                obj.transform.position = new Vector2(pos.x, _camBounds.BottomEdge - offset.y);

            if (pos.y < _camBounds.BottomEdge - offset.y)
                obj.transform.position = new Vector2(pos.x, _camBounds.TopEdge + offset.y);
        }

        public void PlayEffect(EffectsManager.Effect effect, Vector3 position, float scale = 1f)
        {
            _effects.StartEffect(effect, position, scale);
        }

        public void RocketFail()
        {
            Cursor.visible = true;
            print("GAME OVER");
        }

        public void AsterodDestroyed()
        {
            _asteroidLife--;
        }

        struct CamBounds
        {
            public CamBounds(Camera cam)
            {
                Width = cam.orthographicSize * 2 * cam.aspect;
                Height = cam.orthographicSize * 2;

                RightEdge = Width / 2;
                LeftEdge = RightEdge * -1;
                TopEdge = Height / 2;
                BottomEdge = TopEdge * -1;
            }

            public readonly float Width;
            public readonly float Height;
            public readonly float RightEdge;
            public readonly float LeftEdge;
            public readonly float TopEdge;
            public readonly float BottomEdge;
        }

    }
}