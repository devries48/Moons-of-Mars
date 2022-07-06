using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UfoController : BaseSpaceShipController
{
    [Header("UFO")]
    [SerializeField]
    float rotationSpeed = 50f;
    
    void FixedUpdate()
    {
        transform.Rotate(new Vector3(0, rotationSpeed * Time.fixedDeltaTime, 0));
    }
}