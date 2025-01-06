using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class LaserMaker : MonoBehaviour
{
    public CinemachineVirtualCamera vcam;
    public CinemachineVirtualCamera aimcam;
    public Camera cam;
    public float laserMaxLength = 20f;
    public LayerMask ignoreLayer; // Layer to ignore

    private GameObject attachedItem;

    private void Update()
    {
        if (Input.GetButton("Fire1") && aimcam.Priority > vcam.Priority)
        {
            Vector3 rayOrigin = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
            RaycastHit[] hits = Physics.RaycastAll(rayOrigin, cam.transform.forward, laserMaxLength);

            foreach (RaycastHit hit in hits)
            {
                if ((ignoreLayer.value & (1 << hit.collider.gameObject.layer)) == 0)
                {
                    if (hit.collider.CompareTag("Item"))
                    {
                        AttachItem(hit.collider.gameObject);
                    }
                    break; // Break the loop when a valid hit is found
                }
            }
        }
        else
        {
            DetachItem();
        }
    }

    private void AttachItem(GameObject item)
    {
        if (attachedItem != item)
        {
            DetachItem();

            attachedItem = item;
            attachedItem.transform.SetParent(transform);
            attachedItem.transform.localPosition = Vector3.zero;
        }
    }

    private void DetachItem()
    {
        if (attachedItem != null)
        {
            attachedItem.transform.SetParent(null);
            attachedItem = null;
        }
    }
}