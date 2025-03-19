using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchToSettingsInMenu : MonoBehaviour
{
    public GameObject settings;
    public GameObject startMenu;
    public GameObject infoMenu;
    void Start()
    {
        settings.SetActive(false);
    }

    public void OpenSettings()
    {
        startMenu.SetActive(false);
        settings.SetActive(true);
    }
    public void CloseSettings()
    {
        settings.SetActive(false);
        startMenu.SetActive(true);
    }
    public void OpenInfo()
    {
        settings.SetActive(false);
        infoMenu.SetActive(true);
    }
    public void CloseInfo()
    {
        infoMenu.SetActive(false);
        settings.SetActive(true);
    }
}
