using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UISwitcher : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject deathUI;
    public GameObject settings;
    public GameObject genderUI;

    private void Start()
    {
        pauseMenu.SetActive(false);
        settings.SetActive(false);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && pauseMenu.activeInHierarchy == false && deathUI.activeInHierarchy == false && settings.activeInHierarchy == false && genderUI.activeInHierarchy == false)
        {
            pauseMenu.SetActive(true);
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && pauseMenu.activeInHierarchy == true)
        {
            pauseMenu.SetActive(false);
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && pauseMenu.activeInHierarchy == false && deathUI.activeInHierarchy == false && settings.activeInHierarchy == true)
        {
            settings.SetActive(false);
            pauseMenu.SetActive(true);
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void ExitToMainMenu()
    {
        SceneManager.LoadScene(0);
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.None;
    }
    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void OpenSettings()
    {
        pauseMenu.SetActive(false);
        settings.SetActive(true);
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
    }
    public void StopGame()
    {
        pauseMenu.SetActive(true);
        settings.SetActive(false);
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
    }
}