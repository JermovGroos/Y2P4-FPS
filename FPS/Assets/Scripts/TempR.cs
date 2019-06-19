using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempR : MonoBehaviour
{
    float speed;
    public Vector3 rotation;
    void Update()
    {
        transform.Rotate(rotation * Time.deltaTime * speed);
        speed += Time.deltaTime;
    }
}
