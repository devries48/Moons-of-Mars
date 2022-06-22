using UnityEngine;

public class Gameplay : MonoBehaviour
{
    [SerializeField, Tooltip("Select a spaceship prefab")]
    GameObject rocket;

    [SerializeField, Tooltip("Select an astroid prefab")]
    GameObject asteroid;

    [SerializeField, Tooltip("Starting number of astroids")]
    int numberAstroids = 2;

    private bool _allAsteroidsOffScreen;
    private int levelAsteroidNum;
    private Camera mainCam;
    private int asteroidLife;

    private void Start()
    {
        asteroid.SetActive(false); // use prefab
        mainCam = Camera.main;
        CreateAsteroids(numberAstroids);
    }

    private void Update()
    {
        if (asteroidLife <= 0)
        {
            asteroidLife = 6;
            CreateAsteroids(1);
        }

        _allAsteroidsOffScreen = true;

    }

    private void CreateAsteroids(float asteroidsNum)
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
        return Camera.main.ScreenToWorldPoint(screenPos);
    }

    public static void RePosition(GameObject obj, float offset, Camera cam)
    {
        if (cam == null)
            return;

        var pos = cam.WorldToScreenPoint(obj.transform.position);
        var newpos = Vector3.zero;

        if (pos.x >= Screen.safeArea.xMax)
            newpos = new(x: Screen.safeArea.xMin - offset, pos.y, pos.z);
        else if (pos.x <= Screen.safeArea.xMin)
            newpos = new(x: Screen.safeArea.xMax + offset, pos.y, pos.z);

        if (newpos != Vector3.zero)
            obj.transform.position = new Vector3(cam.ScreenToWorldPoint(newpos).x, obj.transform.position.y, obj.transform.position.z);

        if (pos.y >= Screen.safeArea.yMax)
            newpos = new(pos.x, Screen.safeArea.yMin - offset, pos.z);
        else if (pos.y <= Screen.safeArea.yMin)
            newpos = new(pos.x, Screen.safeArea.yMax + offset, pos.z);

        if (newpos != Vector3.zero)
            obj.transform.position = new Vector3(obj.transform.position.x, cam.ScreenToWorldPoint(newpos).y, obj.transform.position.z);
    }


    public void RocketFail()
    {
        Cursor.visible = true;
        print("GAME OVER");
    }

    public void asterodDestroyed()
    {
        asteroidLife--;
    }

    public int StartLevelAsteroidsNum
    {
        get { return numberAstroids; }
    }

    public bool allAsteroidsOffScreen
    {
        get { return _allAsteroidsOffScreen; }
    }

}