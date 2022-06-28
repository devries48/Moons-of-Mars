using UnityEngine;

public class Gameplay : MonoBehaviour
{
    [SerializeField, Tooltip("Select a spaceship prefab")]
    GameObject rocket;

    [SerializeField, Tooltip("Select an astroid prefab")]
    GameObject asteroid;

    [SerializeField, Tooltip("Starting number of astroids")]
    int numberAstroids = 2;

    [SerializeField]
    Camera gameCamera;


    int _asteroidLife;

    private bool _allAsteroidsOffScreen;
    private int levelAsteroidNum;

    static CamBounds camBounds;

    void Start()
    {
        camBounds = new CamBounds(gameCamera);
        Camera.SetupCurrent(gameCamera);
        asteroid.SetActive(false); // use prefab
        CreateAsteroids(numberAstroids);
    }

    void Update()
    {
        if (_asteroidLife <= 0)
        {
            _asteroidLife = 6;
            CreateAsteroids(1);
        }

        _allAsteroidsOffScreen = true;
    }

    void CreateAsteroids(float asteroidsNum)
    {

        //print("Min-max: " + Screen.safeArea.xMin + " -  " + Screen.safeArea.xMax);
        //var pos = Camera.main.WorldToScreenPoint(rocket.transform.position);
        //print("Rocket: " + pos.x + " - " + pos.y);
        //var pos2 = Camera.main.ScreenToWorldPoint(pos);
        //print("Rocket: " + pos2.x + " - " + pos2.y);

        for (int i = 1; i <= asteroidsNum; i++)
        {
            var AsteroidClone = Instantiate(asteroid, new Vector3(Random.Range(-20, 20), 10f), transform.rotation);
            AsteroidClone.GetComponent<Asteroid>().SetGeneration(1);
            AsteroidClone.SetActive(true);
        }
    }

    public static Vector3 WorldPos(Vector3 screenPos)
    {
        return Camera.current.ScreenToWorldPoint(screenPos);
    }

    public static void RePosition(GameObject obj)
    {
        var pos = obj.transform.position;
        var offset = obj.transform.localScale / 2;

        if (pos.x > camBounds.RightEdge + offset.x)
        {
            obj.transform.position = new Vector2(camBounds.LeftEdge - offset.x, pos.y);
        }
        if (pos.x < camBounds.LeftEdge - offset.x)
        {
            obj.transform.position = new Vector2(camBounds.RightEdge +offset.x, pos.y);
        }
        if (pos.y > camBounds.TopEdge + offset.y)
        {
            obj.transform.position = new Vector2(pos.x, camBounds.BottomEdge - offset.y);
        }
        if (pos.y < camBounds.BottomEdge - offset.y)
        {
            obj.transform.position = new Vector2(pos.x, camBounds.TopEdge + offset.y);
        }
    }

    public void RocketFail()
    {
        Cursor.visible = true;
        print("GAME OVER");
    }

    public void asterodDestroyed()
    {
        _asteroidLife--;
    }

    public int StartLevelAsteroidsNum
    {
        get { return numberAstroids; }
    }

    public bool allAsteroidsOffScreen
    {
        get { return _allAsteroidsOffScreen; }
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
            print("width:" + Width);
            print("height:" + Height);
            print("aspect:" + cam.aspect);
        }

        public readonly float Width;
        public readonly float Height;
        public readonly float RightEdge;
        public readonly float LeftEdge;
        public readonly float TopEdge;
        public readonly float BottomEdge;
    }


}