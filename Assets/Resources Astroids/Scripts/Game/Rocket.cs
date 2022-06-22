using System.Collections;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    [Header("Weapon & bullet")]
    [SerializeField, Tooltip("Select a gun prefab")]
    GameObject gun;

    [SerializeField, Tooltip("Select a bullet prefab")]
    GameObject bullet;

    [SerializeField, Tooltip("Lifetime in seconds")]
    float bulletLifetime = 10f;

    [SerializeField, Tooltip("Fire rate in seconds")]
    float fireRate = 0.5f;

    [SerializeField, Tooltip("Fire rate in seconds")]
    float fireForce = 350f;

    private float thrust = 6f;
    private float rotationSpeed = 180f;
    private float MaxSpeed = 4.5f;
    private Camera mainCam;
    private Rigidbody rb;
    bool _canShoot = true;

    private void Start()
    {
        mainCam = Camera.main;
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        ControlRocket();
        CheckPosition();
    }

    private void Update()
    {
        if (!_canShoot)
            return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            StartCoroutine(Shoot());
    }

    private void ControlRocket()
    {
        transform.Rotate(0, 0, Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime);
        rb.AddForce(Input.GetAxis("Vertical") * thrust * transform.up);
        rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -MaxSpeed, MaxSpeed), Mathf.Clamp(rb.velocity.y, -MaxSpeed, MaxSpeed));
    }

    private void CheckPosition()
    {

        float sceneWidth = Screen.width;
        float sceneHeight = Screen.height;

        float sceneRightEdge = sceneWidth / 2;
        float sceneLeftEdge = sceneRightEdge * -1;
        float sceneTopEdge = sceneHeight / 2;
        float sceneBottomEdge = sceneTopEdge * -1;

        if (transform.position.x > sceneRightEdge)
        {
            transform.position = new Vector2(sceneLeftEdge, transform.position.y);
        }

        if (transform.position.x < sceneLeftEdge)
        {
            transform.position = new Vector2(sceneRightEdge, transform.position.y);
        }
        if (transform.position.y > sceneTopEdge)
        {
            transform.position = new Vector2(transform.position.x, sceneBottomEdge);
        }
        if (transform.position.y < sceneBottomEdge)
        {
            transform.position = new Vector2(transform.position.x, sceneTopEdge);
        }
    }

    public void ResetRocket()
    {
        transform.position = new Vector2(0f, 0f);
        transform.eulerAngles = new Vector3(0, 180f, 0);
        rb.velocity = new Vector3(0f, 0f, 0f);
        rb.angularVelocity = new Vector3(0f, 0f, 0f);
    }

    IEnumerator Shoot()
    {
        _canShoot = false;

        var bullet_obj = Instantiate(bullet, gun.transform.position, gun.transform.rotation) as GameObject;
        var bullet_rb = bullet_obj.GetComponent<Rigidbody>();
        bullet_rb.AddForce(transform.up * fireForce);

        Destroy(bullet_obj, bulletLifetime);

        yield return new WaitForSeconds(fireRate);

        _canShoot = true;
    }
}
