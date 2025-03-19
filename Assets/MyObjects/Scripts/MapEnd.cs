using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapEnd : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (other.CompareTag("Player"))
        {
            if (SceneManager.sceneCountInBuildSettings > nextSceneIndex)
            {
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                SceneManager.LoadScene(0);
            }
        }
    }
}
