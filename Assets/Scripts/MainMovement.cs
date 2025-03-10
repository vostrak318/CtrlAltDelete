using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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
    private Vector3 savedPosition;
    private bool haveSpawn = false;
    public Rigidbody bodypart;
    public GameObject spawnPoint;

    private List<Vector3> savePoints = new List<Vector3>();


    

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

        startingPosition = spawnPoint.transform.position;
        savedPosition = startingPosition;
        Debug.Log("Initial spawn point set to: " + startingPosition);
    }

    void Update()
    {
        // Check if the character is on the ground
        isGrounded = Physics.CheckSphere(transform.position, 0.5f, LayerMask.GetMask("Ground"));

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
                if (GenderManager.instance.isFemaleActive == true)
                {
                    Debug.Log("Playing female sound");
                    SoundFXManager.instance.PlaySoundFXClip(SoundFXManager.instance.femaleJumpClip, transform, 1f);
                }
                else if (GenderManager.instance.isMaleActive == true)
                {
                    Debug.Log("Playing male sound");
                    SoundFXManager.instance.PlaySoundFXClip(SoundFXManager.instance.maleJumpClip, transform, 1f);
                }
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
            //StartCoroutine(DisableRagdollAfterTime(ragdollRespawnTime));
            if (!haveSpawn)
            {
                savedPosition = startingPosition;
            }
        }

        if (isRagdollActive && bodypart.IsSleeping() && !DeathUI.activeInHierarchy)
        {
            Debug.Log("Ragdoll is not moving");
            SetRagdollState(false);
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
        else if (collision.gameObject.CompareTag("Trap"))
        {
            SetRagdollState(true);
            Debug.Log("Collided with: " + collision.gameObject.name);
        }
        else if (collision.gameObject.CompareTag("Save"))
        {
            savedPosition = collision.gameObject.transform.position;
            savePoints.Add(savedPosition);
            haveSpawn = true;
            Destroy(collision.gameObject);
            Debug.Log("Save point collected. New saved position: " + savedPosition);
        }
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

            /*
            // Check for ground
            if (isGrounded)
            {
                // Set position to the hit point
                transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
            }*/


            // Cast a ray downwards to check for ground


            transform.position = bodypart.transform.position;
        }
    }

    public IEnumerator DisableRagdollAfterTime(float time)
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
        jumpForce *= 2.5f;
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
    public void UpdateSpawnPosition(Vector3 newSpawnPosition)
    {
        spawnPoint.transform.position = newSpawnPosition;
        Debug.Log("Spawn point updated to: " + newSpawnPosition);
    }
    public void UpdateDeathUI()
    {
        DeathUI.SetActive(true);
    }
    public void Respawn()
    {
        if (savePoints.Count > 0)
        {
            // Vezmi poslední save point
            Vector3 lastSavePoint = savePoints[savePoints.Count - 1];
            gameObject.transform.position = new Vector3(lastSavePoint.x, lastSavePoint.y + 3f, lastSavePoint.z);

            // Odeber tento save point ze seznamu
            savePoints.RemoveAt(savePoints.Count - 1);

            Debug.Log("Respawning at last save point: " + lastSavePoint);
        }
        else
        {
            // Pokud nejsou žádné save pointy, vrátíme se na základní spawn point
            gameObject.transform.position = new Vector3(spawnPoint.transform.position.x, spawnPoint.transform.position.y + 3f, spawnPoint.transform.position.z);
            Debug.Log("Respawning at initial spawn point: " + spawnPoint.transform.position);
        }

        // Reset UI a ragdoll
        DeathUI.SetActive(false);
        haveSpawn = false;
        savedPosition = startingPosition;
        SetRagdollState(false);
        Cursor.lockState = CursorLockMode.Locked;

        // Pøehrát animaci IdleAnim
        animator.Play("IdleAnim");
    }
}