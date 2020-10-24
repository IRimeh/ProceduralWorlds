using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightRotator : MonoBehaviour
{
    [SerializeField]
    private float rotationSpeed = 10.0f;

    void Update()
    {
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, rotationSpeed * Time.deltaTime, 0));
    }
}
