using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingSaveScript : MonoBehaviour
{
    public GameObject spawnedObject;
    public Collider objectCollider;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Vector3 spawnPosition = objectCollider.transform.position + Vector3.up * 8;

            Instantiate(spawnedObject, spawnPosition, Quaternion.identity);

            StartCoroutine(ReactivateCheckpoint());

            gameObject.SetActive(false);
        }
    }

    private IEnumerator ReactivateCheckpoint()
    {
        // Èekat 8 minut (480 sekund)
        yield return new WaitForSeconds(480);

        // Znovu aktivovat checkpoint
        gameObject.SetActive(true);
    }
}