using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class CameraSwitcher : MonoBehaviour
{
    [SerializeField]
    private CinemachineVirtualCamera vcam;
    [SerializeField]
    private CinemachineVirtualCamera aimcam;
    public Canvas crossHair;

    private bool isAimCameraActive = false;
    private InputAction rightClickAction;

    private void Start()
    {
        // Disable the aim camera at the start
        aimcam.Priority = 0;
        crossHair.enabled = false;
    }

    private void OnEnable()
    {
        // Create and enable the right click action
        rightClickAction = new InputAction(type: InputActionType.Button, binding: "<Mouse>/rightButton");
        rightClickAction.performed += OnRightMouseClick;
        rightClickAction.Enable();
    }

    private void OnDisable()
    {
        // Disable and dispose the right click action
        rightClickAction.performed -= OnRightMouseClick;
        rightClickAction.Disable();
        rightClickAction.Dispose();
    }

    private void OnRightMouseClick(InputAction.CallbackContext context)
    {
        if (context.performed && !IsDeathUIActive())
        {
            isAimCameraActive = !isAimCameraActive;
            if (isAimCameraActive)
            {
                SwitchToAimCamera();
                crossHair.enabled = true;
            }
            else
            {
                SwitchToDefaultCamera();
                crossHair.enabled = false;
            }
        }
    }

    private void SwitchToAimCamera()
    {
        if (vcam && aimcam)
        {
            vcam.Priority = 0;
            aimcam.Priority = 1;
        }
    }

    private void SwitchToDefaultCamera()
    {
        if (vcam && aimcam)
        {
            vcam.Priority = 1;
            aimcam.Priority = 0;
        }
    }

    private bool IsDeathUIActive()
    {
        GameObject deathUI = GameObject.Find("DeathUI");
        return deathUI != null && deathUI.activeInHierarchy;
    }
}

