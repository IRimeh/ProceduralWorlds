using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldRotator : MonoBehaviour
{
    [SerializeField]
    private float rotationSpeed = 10.0f;

    void Update()
    {
        transform.Rotate(-rotationSpeed * Time.deltaTime, -rotationSpeed * Time.deltaTime, 0);
    }
}
