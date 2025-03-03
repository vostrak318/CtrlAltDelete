using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpScript : MonoBehaviour
{
    public GameObject player;
    public Transform holdPos;
    public float throwForce = 100f;
    public float pickUpRange = 10f;
    private GameObject heldObj;
    private Rigidbody heldObjRb;
    private bool canDrop = true;
    private bool canPickUp = true;
    private int LayerNumber;
    private PlatformMove platformMove;

    void Start()
    {
        LayerNumber = LayerMask.NameToLayer("holdLayer");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && canPickUp)
        {
            if (heldObj == null)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, pickUpRange))
                {
                    if (hit.transform.gameObject.tag == "Item" || hit.transform.gameObject.tag == "SpeedPotion" || hit.transform.gameObject.tag == "JumpPotion" || hit.transform.gameObject.tag == "TimeSlowPotion" || hit.transform.gameObject.tag == "Save")
                    {
                        PickUpObject(hit.transform.gameObject);
                    }
                    else if (hit.transform.gameObject.tag == "Platform")
                    {
                        platformMove = hit.transform.GetComponent<PlatformMove>();
                        if (platformMove != null)
                        {
                            platformMove.StopPlatform();
                        }
                    }
                }
            }
            else
            {
                if (canDrop == true)
                {
                    StopClipping();
                    DropObject();
                }
            }
        }

        if (heldObj != null && platformMove == null)
        {
            MoveObject();
            if (Input.GetKeyUp(KeyCode.Mouse0) && canDrop == true)
            {
                StopClipping();
                ThrowObject();
            }
            if (Input.GetKeyUp(KeyCode.Mouse1) || Input.GetKeyUp(KeyCode.R) && canDrop == true)
            {
                StopClipping();
                DropObject();
            }
        }

        if (platformMove != null && Input.GetKey(KeyCode.E))
        {
            MovePlatformWithMouse();
        }

        if (Input.GetKeyUp(KeyCode.E) && platformMove != null)
        {
            platformMove.ResumePlatform();
            platformMove = null;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(RagdollTimer());
        }
    }

    void PickUpObject(GameObject pickUpObj)
    {
        if (pickUpObj.GetComponent<Rigidbody>())
        {
            heldObj = pickUpObj;
            heldObjRb = pickUpObj.GetComponent<Rigidbody>();
            heldObjRb.isKinematic = true;
            heldObjRb.transform.parent = holdPos.transform;
            heldObj.layer = LayerNumber;
            Physics.IgnoreCollision(heldObj.GetComponent<Collider>(), player.GetComponent<Collider>(), true);
        }
    }

    void DropObject()
    {
        Physics.IgnoreCollision(heldObj.GetComponent<Collider>(), player.GetComponent<Collider>(), false);
        heldObj.layer = 0;
        heldObjRb.isKinematic = false;
        heldObj.transform.parent = null;
        heldObj = null;
    }

    void MoveObject()
    {
        heldObj.transform.position = holdPos.transform.position;
    }

    void ThrowObject()
    {
        Physics.IgnoreCollision(heldObj.GetComponent<Collider>(), player.GetComponent<Collider>(), false);
        heldObj.layer = 0;
        heldObjRb.isKinematic = false;
        heldObj.transform.parent = null;
        heldObjRb.AddForce(transform.forward * throwForce);
        heldObj = null;
    }

    void StopClipping()
    {
        var clipRange = Vector3.Distance(heldObj.transform.position, transform.position);

        RaycastHit[] hits;
        hits = Physics.RaycastAll(transform.position, transform.TransformDirection(Vector3.forward), clipRange);

        if (hits.Length > 1)
        {
            heldObj.transform.position = transform.position + new Vector3(0f, -0.5f, 0f);
        }
    }

    void MovePlatformWithMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, platformMove.transform.position);
        float distance;
        if (plane.Raycast(ray, out distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            Vector3 direction = (platformMove.endPoint.position - platformMove.startPoint).normalized;
            float length = Vector3.Distance(platformMove.startPoint, platformMove.endPoint.position);
            Vector3 projectedPoint = Vector3.Project(hitPoint - platformMove.startPoint, direction) + platformMove.startPoint;
            float clampedLength = Mathf.Clamp(Vector3.Distance(platformMove.startPoint, projectedPoint), 0, length);
            platformMove.transform.position = platformMove.startPoint + direction * clampedLength;
        }
    }

    private IEnumerator RagdollTimer()
    {
        canPickUp = false;
        yield return new WaitForSeconds(5);
        canPickUp = true;
    }
}


