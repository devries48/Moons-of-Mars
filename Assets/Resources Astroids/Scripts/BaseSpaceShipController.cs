using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseSpaceShipController : MonoBehaviour
{
    #region editor fields
    [Header("Spaceship")]

    [Header("Weapon & bullet")]
    [SerializeField, Tooltip("Select a weapon prefab")]
    protected GameObject weapon;

    [SerializeField, Tooltip("Select a bullet prefab")]
    protected GameObject bullet;

    [SerializeField, Tooltip("Lifetime in seconds")]
    protected float bulletLifetime = 10f;

    [SerializeField, Tooltip("Fire rate in seconds")]
    protected float fireRate = 0.5f;

    [SerializeField]
    protected float fireForce = 350f;

    [SerializeField]
    protected SpaceShipSounds sounds = new();

    #endregion
    protected bool m_canShoot = true;
    protected Rigidbody m_rb;

    void Start()
    {
        m_rb = GetComponent<Rigidbody>();
    }

    protected IEnumerator Shoot()
    {
        m_canShoot = false;

        var bullet_obj = Instantiate(bullet, weapon.transform.position, weapon.transform.rotation) as GameObject;
        var bullet_rb = bullet_obj.GetComponent<Rigidbody>();

        bullet_rb.AddForce(transform.up * fireForce);
        Destroy(bullet_obj, bulletLifetime);
        yield return new WaitForSeconds(fireRate);

        m_canShoot = true;
    }

    public void ResetRocket()
    {
        transform.position = new Vector2(0f, 0f);
        transform.eulerAngles = new Vector3(0, 180f, 0);

        m_rb.velocity = new Vector3(0f, 0f, 0f);
        m_rb.angularVelocity = new Vector3(0f, 0f, 0f);
    }


}
