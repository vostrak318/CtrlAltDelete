using UnityEngine;
using System.Collections;

public class MainMovement : MonoBehaviour
{
    public float walkSpeed = 1.5f; // Snížená rychlost chùze
    public float sprintSpeed = 3f; // Snížená rychlost bìhu
    public float jumpForce = 6f;
    public Animator animator;

    private Rigidbody rb;
    private Vector3 movement;
    private Vector3 airMovement;

    public float turnSpeed = 8;
    private bool isGrounded;
    private bool canJump = true;
    public float jumpCooldown = 1.32f;
    public float jumpAnimationTime = 0.5f;
    Camera mainCamera;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Kontrola, zda je postava na zemi
        isGrounded = Physics.CheckSphere(transform.position, 0.1f, LayerMask.GetMask("Ground"));

        // Získání vstupu z klávesnice pouze pokud je na zemi
        float moveHorizontal = 0f;
        float moveVertical = 0f;

        if (isGrounded)
        {
            moveHorizontal = Input.GetAxis("Horizontal");
            moveVertical = Input.GetAxis("Vertical");

            // Pohyb ve smìru kamery
            Vector3 forward = mainCamera.transform.forward;
            Vector3 right = mainCamera.transform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            movement = forward * moveVertical + right * moveHorizontal;

            // Nastavení animací
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

            // Skok
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded && canJump)
            {
                rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
                StartCoroutine(JumpAnimationTimer());
                StartCoroutine(JumpCooldown());
            }

            // Otáèení postavy podle smìru pohybu
            if (isMoving)
            {
                Quaternion targetRotation = Quaternion.LookRotation(movement);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }

            // Uložení smìru pohybu pro použití ve vzduchu
            airMovement = movement;
        }
    }

    void FixedUpdate()
    {
        // Pohyb postavy
        if (isGrounded)
        {
            Vector3 newPosition = rb.position + movement * Time.fixedDeltaTime;
            rb.MovePosition(newPosition);
        }
        else
        {
            // Pokraèování v pohybu ve vzduchu
            Vector3 newPosition = rb.position + airMovement * Time.fixedDeltaTime;
            rb.MovePosition(newPosition);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        // Kontrola, zda se dotýkáme vrstvy Ground
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        // Pokud opustíme kolizi s vrstvou Ground, nejsme na zemi
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = false;
        }
    }

    IEnumerator JumpAnimationTimer()
    {
        animator.SetBool("Jump", true);
        yield return new WaitForSeconds(jumpAnimationTime);
        animator.SetBool("Jump", false);
    }

    IEnumerator JumpCooldown()
    {
        canJump = false;
        yield return new WaitForSeconds(jumpCooldown);
        canJump = true;
    }
}



