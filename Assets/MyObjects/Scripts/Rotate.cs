using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    [SerializeField]
    float speed = 0.2f;
    void Update()
    {
        if (UISwitcher.instance.infoUI.activeInHierarchy == true || UISwitcher.instance.settings.activeInHierarchy == true || UISwitcher.instance.pauseMenu.activeInHierarchy == true)
        {
            transform.Rotate(0, 0, 0);
        }
        else
        {
            transform.Rotate(0, speed, 0);
        }
    }
}