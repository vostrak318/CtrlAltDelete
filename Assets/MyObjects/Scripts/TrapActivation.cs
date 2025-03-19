using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapActivation : MonoBehaviour
{
    private bool isRotating = false;
    private Quaternion targetRotation;
    public float pushForce = 10f;
    public Transform rotationPivot; // Pøidáme pivot pro rotaci
    public Collider triggerCollider; // Pøidáme odkaz na Collider s Trigger

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !isRotating)
        {
            targetRotation = rotationPivot.rotation * Quaternion.Euler(0, 0, 80);
            StartCoroutine(RotateTrap(other));
        }
    }

    private IEnumerator RotateTrap(Collider other)
    {
        isRotating = true;
        float elapsedTime = 0f;
        float duration = 0.3f; // Rychlá rotace bìhem 0.3 sekundy

        Quaternion startingRotation = rotationPivot.rotation;

        while (elapsedTime < duration)
        {
            rotationPivot.rotation = Quaternion.Lerp(startingRotation, targetRotation, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rotationPivot.rotation = targetRotation;
        isRotating = false;

        // Apply velocity to the object that touched the trap in the correct direction
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 pushDirection = (other.transform.position - rotationPivot.position).normalized;
            rb.velocity = pushDirection * pushForce;
        }

        // Set "SuperJump" bool to true in the animator of the object that touched the trap
        Animator animator = other.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetBool("SuperJump", true);
            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
            animator.SetBool("SuperJump", false);
        }

        // Deactivate the trigger collider
        triggerCollider.enabled = false;

        // Return to the original position over 3 seconds
        elapsedTime = 0f;
        float returnDuration = 3f;
        while (elapsedTime < returnDuration)
        {
            rotationPivot.rotation = Quaternion.Lerp(targetRotation, startingRotation, elapsedTime / returnDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rotationPivot.rotation = startingRotation;

        // Reactivate the trigger collider
        triggerCollider.enabled = true;
    }
}




