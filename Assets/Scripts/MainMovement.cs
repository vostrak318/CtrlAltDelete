using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class MainMovement : MonoBehaviour
{
    public float walkSpeed = 1.5f;
    public float sprintSpeed = 3f;
    public float jumpForce = 6f;
    public Animator animator;

    private Rigidbody rb;
    private Vector3 movement;
    private Vector3 airMovement;

    public float turnSpeed = 8;
    private bool isGrounded;
    private bool canJump = true;
    private bool isRagdollActive = false;
    public float jumpCooldown = 1.32f;
    public float jumpAnimationTime = 0.5f;
    Camera mainCamera;
    public bool hasSuperJump = false;

    private Rigidbody[] ragdollBodies;
    private Collider[] ragdollColliders;
    private Collider mainCollider;
    private Vector3 savedVelocity;

    public int ragdollPower = 50;
    public float ragdollRespawnTime = 5f;

    public GameObject DeathUI;
    private Vector3 startingPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        mainCamera = Camera.main;

        // Initialize ragdoll components
        ragdollBodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();
        mainCollider = GetComponent<Collider>();

        // Deactivate ragdoll at the start
        SetRagdollState(false);

        startingPosition = gameObject.transform.position;
    }

    void Update()
    {
        // Check if the character is on the ground
        isGrounded = Physics.CheckSphere(transform.position, 0.1f, LayerMask.GetMask("Ground"));

        // Get keyboard input only if on the ground and ragdoll is not active
        float moveHorizontal = 0f;
        float moveVertical = 0f;

        if (isGrounded && !isRagdollActive)
        {
            moveHorizontal = Input.GetAxis("Horizontal");
            moveVertical = Input.GetAxis("Vertical");

            // Move in the direction of the camera
            Vector3 forward = mainCamera.transform.forward;
            Vector3 right = mainCamera.transform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            movement = forward * moveVertical + right * moveHorizontal;

            // Set animations
            bool isMoving = movement.sqrMagnitude > 0;
            animator.SetBool("Move", isMoving);

            // Sprint
            bool isSprinting = Input.GetKey(KeyCode.LeftShift);
            animator.SetBool("Sprint", isSprinting);

            if (isSprinting)
            {
                movement *= sprintSpeed;
            }
            else
            {
                movement *= walkSpeed;
            }

            // Jump
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded && canJump)
            {
                rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
                StartCoroutine(JumpAnimationTimer());
                StartCoroutine(JumpCooldown());
            }

            // Rotate character in the direction of movement
            if (isMoving)
            {
                Quaternion targetRotation = Quaternion.LookRotation(movement);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }

            // Save movement direction for use in the air
            airMovement = movement;
        }

        // Activate ragdoll on pressing R
        if (Input.GetKeyDown(KeyCode.R))
        {
            SetRagdollState(true);
            StartCoroutine(DisableRagdollAfterTime(ragdollRespawnTime));
        }



        if (DeathUI.activeInHierarchy == true)
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    void FixedUpdate()
    {
        // Move character
        if (isGrounded && !isRagdollActive)
        {
            Vector3 newPosition = rb.position + movement * Time.fixedDeltaTime;
            rb.MovePosition(newPosition);
        }
        else if (!isGrounded && !isRagdollActive)
        {
            Vector3 newPosition = rb.position + airMovement * Time.fixedDeltaTime; // Continue moving in the air

            savedVelocity = (newPosition - rb.position) / Time.deltaTime;
            rb.MovePosition(newPosition);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        // Check if touching the Ground layer
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        // If leaving collision with the Ground layer, not on the ground
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("SpeedPotion"))
        {
            Destroy(collision.gameObject);
            StartCoroutine(SpeedPotionEffect());
        }
        else if (collision.gameObject.CompareTag("JumpPotion"))
        {
            Destroy(collision.gameObject);
            StartCoroutine(JumpPotionEffect());
        }
        else if (collision.gameObject.CompareTag("TimeSlowPotion"))
        {
            Destroy(collision.gameObject);
            StartCoroutine(TimeSlowPotionEffect());
        }
        else if (collision.gameObject.CompareTag("Trap") || HasChildWithTag(collision.gameObject, "Trap"))
        {
            SetRagdollState(true);
            StartCoroutine(DisableRagdollAfterTime(ragdollRespawnTime));
        }
        
    }
    /*void OnTriggerEnter(Collider other)
    {
        foreach (Rigidbody ragdollBody in ragdollBodies)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
            {
                SetRagdollState(true);
                DeathUI.SetActive(true);
            }
        }
    }*/

    private bool HasChildWithTag(GameObject obj, string tag)
    {
        foreach (Transform child in obj.transform)
        {
            if (child.CompareTag(tag))
            {
                return true;
            }
        }
        return false;
    }

    IEnumerator JumpAnimationTimer()
    {
        if (!hasSuperJump)
        {
            animator.SetBool("Jump", true);
            yield return new WaitForSeconds(jumpAnimationTime);
            animator.SetBool("Jump", false);
        }
        else
        {
            animator.SetBool("SuperJump", true);
            yield return new WaitForSeconds(jumpAnimationTime);
            animator.SetBool("SuperJump", false);
        }
    }

    IEnumerator JumpCooldown()
    {
        canJump = false;
        yield return new WaitForSeconds(jumpCooldown);
        canJump = true;
    }

    // ========================================================================================
    // Ragdoll functions
    // ========================================================================================

    public void SetRagdollState(bool state)
    {
        isRagdollActive = state;

        if (state)
        {
            // Activate ragdoll
            foreach (Rigidbody ragdollBody in ragdollBodies)
            {
                if (ragdollBody != rb)
                {
                    ragdollBody.isKinematic = false;
                    ragdollBody.velocity = rb.velocity;
                    ragdollBody.AddForce(savedVelocity, ForceMode.Impulse);
                }
            }

            foreach (Collider ragdollCollider in ragdollColliders)
            {
                if (ragdollCollider != mainCollider)
                {
                    ragdollCollider.enabled = true;
                }
            }

            // Deactivate animator
            animator.enabled = false;

            // Deactivate main Rigidbody and Collider
            rb.isKinematic = true;
            mainCollider.enabled = false;
        }
        else
        {
            if (ragdollBodies != null)
            {
                foreach (Rigidbody ragdollBody in ragdollBodies)
                {
                    if (ragdollBody != rb)
                    {
                        ragdollBody.isKinematic = true;
                    }
                }
            }
            // Deactivate ragdoll

            if (ragdollColliders != null)
            {
                foreach (Collider ragdollCollider in ragdollColliders)
                {
                    if (ragdollCollider != mainCollider)
                    {
                        ragdollCollider.enabled = false;
                    }
                }
            }


            // Activate animator
            animator.enabled = true;
            animator.Play("IdleAnim");

            // Activate main Rigidbody and Collider
            if (rb != null)
            {
                rb.isKinematic = false;
                mainCollider.enabled = true;
            }

            // Cast a ray downwards to check for ground
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
            {
                // Set position to the hit point
                transform.position = hit.point;
            }
        }
    }

    private IEnumerator DisableRagdollAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        SetRagdollState(false);
    }

    // ========================================================================================
    // Potion functions
    // ========================================================================================

    private IEnumerator SpeedPotionEffect()
    {
        walkSpeed *= 2;
        sprintSpeed *= 2;
        yield return new WaitForSeconds(5);
        walkSpeed /= 2;
        sprintSpeed /= 2;
    }

    private IEnumerator JumpPotionEffect()
    {
        jumpForce *= 3;
        hasSuperJump = true;
        yield return new WaitForSeconds(5);
        jumpForce /= 3;
        hasSuperJump = false;
    }

    private IEnumerator TimeSlowPotionEffect()
    {
        Time.timeScale = 0.5f;
        yield return new WaitForSecondsRealtime(5);
        Time.timeScale = 1f;
    }

    //========================================================================================
    //Respawn function
    //========================================================================================

    public void UpdateDeathUI()
    {
        DeathUI.SetActive(true);
    }
    public void Respawn()
    {
        gameObject.transform.position = startingPosition;
        DeathUI.SetActive(false);
        SetRagdollState(false);
        Cursor.lockState = CursorLockMode.Locked;
    }
}

