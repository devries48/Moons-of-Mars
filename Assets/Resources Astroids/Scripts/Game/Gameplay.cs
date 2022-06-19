using UnityEngine;

public class Gameplay : MonoBehaviour
{

    public GameObject asteroid;
    public GameObject rocket;

    [SerializeField]
    private int startNumberAstroids = 1;

    private bool _allAsteroidsOffScreen;
    private int levelAsteroidNum;
    private Camera mainCam;
    private int asteroidLife;

    private void Start()
    {
        asteroid.SetActive(false); // use prefab
        mainCam = Camera.main;
        CreateAsteroids(startNumberAstroids);
    }

    private void Update()
    {
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * 0.8f);

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

    public void RocketFail()
    {
        Cursor.visible = true;
        print("GAME OVER");
    }

    public void asterodDestroyed()
    {
        asteroidLife--;
    }

    public int startLevelAsteroidsNum
    {
        get { return startNumberAstroids; }
    }

    public bool allAsteroidsOffScreen
    {
        get { return _allAsteroidsOffScreen; }
    }

}