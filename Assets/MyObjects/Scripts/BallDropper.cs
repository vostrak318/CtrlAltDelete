using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallDropper : MonoBehaviour
{
    public GameObject ballPrefab;
    public float spawnInterval = 4f;
    public Vector3 spawnAreaSize = new Vector3(5f, 0f, 5f); // Velikost oblasti pro spawnování koulí

    void Start()
    {
        StartCoroutine(SpawnBalls());
    }

    private IEnumerator SpawnBalls()
    {
        while (true)
        {
            SpawnBall();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnBall()
    {
        Vector3 randomPosition = GetRandomPosition();
        GameObject ball = Instantiate(ballPrefab, randomPosition, Quaternion.identity);
        ball.AddComponent<BallCollisionHandler>();
    }

    private Vector3 GetRandomPosition()
    {
        Vector3 randomOffset = new Vector3(
            Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
            Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2),
            Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
        );
        return transform.position + randomOffset;
    }
}

public class BallCollisionHandler : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (MainMovement.instance != null)
            {
                MainMovement.instance.SetRagdollState(true);
            }
            else
            {
                Debug.LogError("MainMovement instance is not set.");
            }
        }
    }
}

