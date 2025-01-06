using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ragdoll : MonoBehaviour
{
    private Rigidbody[] ragdollBodies;
    private Collider[] ragdollColliders;
    private Animator animator;
    private Rigidbody mainRigidbody;
    private Collider mainCollider;

    void Start()
    {
        // Získání všech Rigidbody a Collider komponent v potomcích, kromì hlavního Rigidbody a Collider
        ragdollBodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();
        animator = GetComponent<Animator>();
        mainRigidbody = GetComponent<Rigidbody>();
        mainCollider = GetComponent<Collider>();

        // Deaktivace ragdoll na zaèátku
        SetRagdollState(false);
    }

    void Update()
    {
        // Aktivace ragdoll po stisku tlaèítka R
        if (Input.GetKeyDown(KeyCode.R))
        {
            SetRagdollState(true);
            StartCoroutine(DisableRagdollAfterTime(5f));
        }
    }

    private void SetRagdollState(bool state)
    {
        if (state)
        {
            // Uložení aktuální rychlosti a úhlové rychlosti hlavního Rigidbody
            Vector3 savedVelocity = mainRigidbody.velocity;
            Vector3 savedAngularVelocity = mainRigidbody.angularVelocity;

            // Aktivace ragdollu
            foreach (Rigidbody rb in ragdollBodies)
            {
                if (rb != mainRigidbody)
                {
                    rb.isKinematic = false;
                    rb.velocity = savedVelocity;
                    rb.angularVelocity = savedAngularVelocity;
                }
            }

            foreach (Collider col in ragdollColliders)
            {
                if (col != mainCollider)
                {
                    col.enabled = true;
                }
            }

            // Deaktivace animátoru
            animator.enabled = false;

            // Deaktivace hlavního Rigidbody a Collider
            mainRigidbody.isKinematic = true;
            mainCollider.enabled = false;
        }
        else
        {
            // Deaktivace ragdollu
            foreach (Rigidbody rb in ragdollBodies)
            {
                if (rb != mainRigidbody)
                {
                    rb.isKinematic = true;
                }
            }

            foreach (Collider col in ragdollColliders)
            {
                if (col != mainCollider)
                {
                    col.enabled = false;
                }
            }

            // Aktivace animátoru
            animator.enabled = true;

            // Aktivace hlavního Rigidbody a Collider
            mainRigidbody.isKinematic = false;
            mainCollider.enabled = true;
        }
    }

    private IEnumerator DisableRagdollAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        SetRagdollState(false);
    }
}




