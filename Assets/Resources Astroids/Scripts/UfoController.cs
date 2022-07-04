using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UfoController : MonoBehaviour
{

    [SerializeField]
    float rotationSpeed = 50f;

    Rigidbody _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        transform.Rotate(new Vector3(0, rotationSpeed * Time.fixedDeltaTime, 0));
    }
}