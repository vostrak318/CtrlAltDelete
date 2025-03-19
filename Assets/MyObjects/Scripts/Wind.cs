using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wind : MonoBehaviour
{
    [SerializeField]
    private float windStrength;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Ball")) // Kontrola tagu
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 windDirection = transform.forward; // Vítr fouká ve smìru natoèení objektu
                rb.AddForce(windDirection.normalized * windStrength, ForceMode.Force);
            }
        }
    }
}
