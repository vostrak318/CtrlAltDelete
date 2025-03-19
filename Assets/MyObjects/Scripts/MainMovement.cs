using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class MainMovement : MonoBehaviour
{
    public static MainMovement instance;

    public float walkSpeed = 1.5f;
    public float sprintSpeed = 3f;
    public float jumpForce = 6f;
    public Animator maleAnimator;
    public Animator femaleAnimator;

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
    public Vector3 bodypartInitialPosition;
    public GameObject spawnPoint;

    private List<Vector3> savePoints = new List<Vector3>();

    private AudioSource movementAudioSource; // Nová promìnná pro pøehrávání zvuku pohybu

    private bool hasPlayedDeathSound = false;

    private bool isRespawned = false;

    void Start()
    {
        instance = this;

        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        mainCamera = Camera.main;

        ragdollBodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();
        mainCollider = GetComponent<Collider>();

        bodypartInitialPosition = bodypart.transform.localPosition;

        SetRagdollState(false);

        startingPosition = spawnPoint.transform.position;
        savedPosition = startingPosition;

        movementAudioSource = gameObject.AddComponent<AudioSource>();
        movementAudioSource.loop = true;
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
            // Walk
            bool isMoving = movement.sqrMagnitude > 0;
            GetActiveAnimator().SetBool("Move", isMoving);

            // Sprint
            bool isSprinting = Input.GetKey(KeyCode.LeftShift);
            GetActiveAnimator().SetBool("Sprint", isSprinting);

            if (isSprinting)
            {
                movement *= sprintSpeed;
                if (!SoundFXManager.instance.IsPlaying(SoundFXManager.instance.runClip) && isGrounded && GetActiveAnimator().GetBool("Move") == true)
                {
                    SoundFXManager.instance.PlayLoopingSoundFX(SoundFXManager.instance.runClip, transform, 1.5f);
                }
                else if (GetActiveAnimator().GetBool("Move") == false)
                {
                    SoundFXManager.instance.StopLoopingSoundFX();
                }
            }
            else if (isMoving) // Pokud se pohybuje, ale nesprintuje
            {
                movement *= walkSpeed;
                if (!SoundFXManager.instance.IsPlaying(SoundFXManager.instance.walkClip) && isGrounded)
                {
                    SoundFXManager.instance.PlayLoopingSoundFX(SoundFXManager.instance.walkClip, transform, 1.5f);
                }
            }
            else
            {
                SoundFXManager.instance.StopLoopingSoundFX();
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
        else
        {
            // Pokud není na zemi, zastavte pøehrávání zvuku chùze
            SoundFXManager.instance.StopLoopingSoundFX();
        }

        // Activate ragdoll on pressing R
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (isRagdollActive == false)
            {
                SoundFXManager.instance.PlaySoundFXClip(SoundFXManager.instance.ragdollClip, transform, 0.3f);
            }
            SetRagdollState(true);
        }

        if (isRagdollActive && bodypart.IsSleeping() && !DeathUI.activeInHierarchy)
        {
            Debug.Log("Ragdoll is not moving");
            SetRagdollState(false);
        }

        if (DeathUI.activeInHierarchy == true && !hasPlayedDeathSound)
        {
            Cursor.lockState = CursorLockMode.None;
            hasPlayedDeathSound = true;
        }

        if (isRespawned)
        {
            AfterRespawn();
            isRespawned = false;
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
            GetActiveAnimator().SetBool("Sprint", false);
            GetActiveAnimator().SetBool("Jump", false);
            GetActiveAnimator().SetBool("Move", false);
            GetActiveAnimator().SetBool("SuperJump", true);
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
            GetActiveAnimator().SetBool("Jump", true);
            yield return new WaitForSeconds(jumpAnimationTime);
            GetActiveAnimator().SetBool("Jump", false);
        }
        else
        {
            GetActiveAnimator().SetBool("SuperJump", true);
            yield return new WaitForSeconds(jumpAnimationTime);
            GetActiveAnimator().SetBool("SuperJump", false);
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
            GetActiveAnimator().enabled = false;

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
            GetActiveAnimator().enabled = true;

            // Activate main Rigidbody and Collider
            if (rb != null)
            {
                rb.isKinematic = false;
                mainCollider.enabled = true;
            }

            // Pøehrát animaci IdleAnim nebo IdleWoman podle pohlaví
            if (GenderManager.instance.isFemaleActive)
            {
                GetActiveAnimator().SetBool("Move", false);
                GetActiveAnimator().SetBool("Sprint", false);
                GetActiveAnimator().SetBool("Jump", false);
                GetActiveAnimator().SetBool("SuperJump", false);
                femaleAnimator.Play("IdleWoman");
            }
            else if (GenderManager.instance.isMaleActive)
            {
                GetActiveAnimator().SetBool("Move", false);
                GetActiveAnimator().SetBool("Sprint", false);
                GetActiveAnimator().SetBool("Jump", false);
                GetActiveAnimator().SetBool("SuperJump", false);
                maleAnimator.Play("IdleAnim");
            }

            transform.position = bodypart.transform.position;
        }
    }

    public IEnumerator DisableRagdollAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        SetRagdollState(false);
    }

    public Animator GetActiveAnimator()
    {
        if (GenderManager.instance.isFemaleActive)
        {
            return femaleAnimator;
        }
        else if (GenderManager.instance.isMaleActive)
        {
            return maleAnimator;
        }
        return null;
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
        Cursor.lockState = CursorLockMode.None;
        if (!hasPlayedDeathSound)
        {
            PlayDeathSound();
            hasPlayedDeathSound = true;
        }
    }
    public void Respawn()
    {
        if (isRespawned) return; // Zajistí, že se metoda spustí pouze jednou

        if (savePoints.Count > 0)
        {
            // Vezmi poslední save point
            Vector3 lastSavePoint = savePoints[savePoints.Count - 1];
            savedPosition = new Vector3(lastSavePoint.x, lastSavePoint.y, lastSavePoint.z);

            // Odeber tento save point ze seznamu
            savePoints.RemoveAt(savePoints.Count - 1);
        }
        else
        {
            // Pokud nejsou žádné save pointy, vrátíme se na základní spawn point
            savedPosition = new Vector3(spawnPoint.transform.position.x, spawnPoint.transform.position.y, spawnPoint.transform.position.z);
        }

        // Nastav pozici postavy na respawn pozici
        Debug.Log("Should respawn at: " + savedPosition);
        Debug.Log("Respawned on position: " + transform.position);

        // Reset death sound flag
        hasPlayedDeathSound = false;

        isRespawned = true;
    }

    private void AfterRespawn()
    {
        DeathUI.SetActive(false);
        haveSpawn = false;
        SetRagdollState(false);
        Cursor.lockState = CursorLockMode.Locked;
        rb.velocity = Vector3.zero;

        foreach (Rigidbody ragdollBody in ragdollBodies)
        {
            ragdollBody.isKinematic = false;
            ragdollBody.velocity = Vector3.zero;
        }

        // Pøehrát animaci IdleAnim nebo IdleWoman podle pohlaví
        if (GenderManager.instance.isFemaleActive)
        {
            GetActiveAnimator().SetBool("Move", false);
            GetActiveAnimator().SetBool("Sprint", false);
            GetActiveAnimator().SetBool("Jump", false);
            GetActiveAnimator().SetBool("SuperJump", false);
            femaleAnimator.Play("IdleWoman");
        }
        else if (GenderManager.instance.isMaleActive)
        {
            GetActiveAnimator().SetBool("Move", false);
            GetActiveAnimator().SetBool("Sprint", false);
            GetActiveAnimator().SetBool("Jump", false);
            GetActiveAnimator().SetBool("SuperJump", false);
            maleAnimator.Play("IdleAnim");
        }

        transform.position = savedPosition;
        bodypart.isKinematic = true;

        if (savePoints.Count > 0)
        {
            savedPosition = savePoints[savePoints.Count - 1];
            savePoints.RemoveAt(savePoints.Count - 1);
        }
        else if (savePoints.Count == 0)
        {
            savedPosition = startingPosition;
        }
    }

    private void PlayDeathSound()
    {
        if (GenderManager.instance.isFemaleActive == true)
        {
            SoundFXManager.instance.PlaySoundFXClip(SoundFXManager.instance.femaleDeathClip, transform, 1f);
        }
        else if (GenderManager.instance.isMaleActive == true)
        {
            SoundFXManager.instance.PlaySoundFXClip(SoundFXManager.instance.maleDeathClip, transform, 1f);
        }
    }
}
