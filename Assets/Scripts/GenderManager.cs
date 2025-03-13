using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class GenderManager : MonoBehaviour
{
    public static GenderManager instance;

    public GameObject chooseGenderUI;

    public GameObject male;
    public GameObject female;
    public CinemachineVirtualCamera vcam;
    public CinemachineVirtualCamera aimcam;
    public GameObject camFollowMale;
    public GameObject camFollowFemale;

    public bool isMaleActive = false;
    public bool isFemaleActive = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        chooseGenderUI.SetActive(true);
        male.SetActive(false);
        female.SetActive(false);
        isMaleActive = false;
        isFemaleActive = false;
    }

    public void SetMaleActive()
    {
        isMaleActive = true;
        isFemaleActive = false;
        male.SetActive(true);
        female.SetActive(false);
        chooseGenderUI.SetActive(false);
        vcam.Follow = camFollowMale.transform;
        vcam.LookAt = camFollowMale.transform;
        aimcam.Follow = camFollowMale.transform;
        aimcam.LookAt = camFollowMale.transform;
    }

    public void SetFemaleActive()
    {
        isFemaleActive = true;
        isMaleActive = false;
        female.SetActive(true);
        male.SetActive(false);
        chooseGenderUI.SetActive(false);
        vcam.Follow = camFollowFemale.transform;
        vcam.LookAt = camFollowFemale.transform;
        aimcam.Follow = camFollowFemale.transform;
        aimcam.LookAt = camFollowFemale.transform;
    }
}
