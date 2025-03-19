using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformMove : MonoBehaviour
{
    public Transform endPoint;
    public float speed = 3f;

    public Vector3 startPoint;
    private bool movingToEnd = true;
    private bool isStopped = false;

    [SerializeField]
    private bool isActive = true;

    void Start()
    {
        startPoint = transform.position;
    }

    void Update()
    {
        if (isActive)
        {
            if (!isStopped)
            {
                MovePlatform();
            }
        }
    }

    void MovePlatform()
    {
        if (movingToEnd)
        {
            transform.position = Vector3.MoveTowards(transform.position, endPoint.position, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, endPoint.position) < 0.1f)
            {
                movingToEnd = false;
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, startPoint, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, startPoint) < 0.1f)
            {
                movingToEnd = true;
            }
        }
    }

    public void StopPlatform()
    {
        isStopped = true;
    }

    public void ResumePlatform()
    {
        isStopped = false;
    }

    // ========================================================================================
    // Attach player to platform
    // ========================================================================================

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.transform.parent = transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.transform.parent = null;
        }
    }
}
