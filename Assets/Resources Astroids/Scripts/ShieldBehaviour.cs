using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class ShieldBehaviour : MonoBehaviour
{
    [SerializeField]
    int magnitude = 2000;

    [SerializeField]
    float shieldVisibleTimer = 2f;
    MeshRenderer MeshRend
    {
        get
        {
            if (__meshRend == null)
                TryGetComponent(out __meshRend);

            return __meshRend;
        }
    }
    MeshRenderer __meshRend;

    float _visibleTimer;

    void Start()
    {
        MeshRend.enabled = false;
    }

    void Update()
    {
        if (_visibleTimer > 0f)
        {
            _visibleTimer -= Time.deltaTime;

            if (_visibleTimer <= 0f)
                ShieldsDown();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Astroid"))
        {
            ShieldsUp();
            var force = transform.position - other.transform.position;

            other.TryGetComponent<Rigidbody>(out var rb);
            if (rb == null)
                return;

            force.Normalize();
            rb.AddForce(-force * magnitude);
        }
    }

    void ShieldsUp()
    {
        MeshRend.enabled = true;
        _visibleTimer = shieldVisibleTimer;
    }

    void ShieldsDown()
    {
        MeshRend.enabled = false;
    }
}
