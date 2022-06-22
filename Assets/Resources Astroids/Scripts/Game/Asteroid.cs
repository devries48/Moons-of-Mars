using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Asteroid : MonoBehaviour
{

    [SerializeField]
    Gameplay gameplay;

    [SerializeField]
    float offset = 40f;

    [SerializeField]
    float maxSpeed = 3f;

    Renderer _renderer;
    Rigidbody _rb;
    float _rotationX;
    float _rotationY;
    float _rotationZ;
    int _generation;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
        _rb = GetComponent<Rigidbody>();

        var maxRotation = 25f;
        _rotationX = Random.Range(-maxRotation, maxRotation);
        _rotationY = Random.Range(-maxRotation, maxRotation);
        _rotationZ = Random.Range(-maxRotation, maxRotation);

        _rb.AddForce(transform.right * CreateRandomSpeed());
        _rb.AddForce(transform.up * CreateRandomSpeed());
    }

    void Update()
    {
        transform.Rotate(new Vector3(_rotationX, _rotationY, _rotationZ) * Time.deltaTime);

        _rb.velocity = new Vector2(Mathf.Clamp(_rb.velocity.x, -maxSpeed, maxSpeed), Mathf.Clamp(_rb.velocity.y, -maxSpeed, maxSpeed));
    }

    void OnCollisionEnter(Collision collisionInfo)
    {
        if (collisionInfo.collider.tag == "Bullet")
        {
            if (_generation < 3)
                CreateSmallAsteriods(2);

            // Destroy astroid & bullet
            Destroy();
            Destroy(collisionInfo.gameObject);
        }

        if (collisionInfo.collider.name == "Rocket")
            gameplay.RocketFail();
    }

    private void OnBecameInvisible()
    {
         Gameplay.RePosition(gameObject, offset, Camera.main);
    }

    public void SetGeneration(int generation)
    {
        _generation = generation;
    }

    float CreateRandomSpeed()
    {
        var speed = Random.Range(200f, 800f);
        var selector = Random.Range(0, 2);
        var dir = selector == 1 ? -1 : 1;

        return speed * dir;
    }

    void CreateSmallAsteriods(int asteroidsNum)
    {
        int newGeneration = _generation + 1;
        var scaleSize = 0.5f;

        for (int i = 1; i <= asteroidsNum; i++)
        {
            var AsteroidClone = Instantiate(gameObject, new Vector3(transform.position.x, transform.position.y, 0f), transform.rotation);

            AsteroidClone.transform.localScale = new Vector3(AsteroidClone.transform.localScale.x * scaleSize, AsteroidClone.transform.localScale.y * scaleSize, AsteroidClone.transform.localScale.z * scaleSize);
            AsteroidClone.GetComponent<Asteroid>().SetGeneration(newGeneration);
            AsteroidClone.SetActive(true);
        }
    }

    public void Destroy()
    {
        gameplay.asterodDestroyed();
        Destroy(gameObject, 0.01f);
    }

    public void DestroySilent()
    {
        Destroy(gameObject, 0.00f);
    }

}