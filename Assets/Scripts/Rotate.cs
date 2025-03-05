using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    [SerializeField]
    float speed = 0.2f;
    void Update()
    {
        transform.Rotate(0, speed, 0);
    }
}
