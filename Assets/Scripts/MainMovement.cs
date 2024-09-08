using UnityEngine;

public class MainMovement : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpForce = 5f;
    public Animator animator;

    private Rigidbody rb;
    private Vector3 movement;

    public float turnSpeed = 5;
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
        // Z�sk�n� vstupu z kl�vesnice
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        // Pohyb ve sm�ru kamery
        Vector3 forward = mainCamera.transform.forward;
        Vector3 right = mainCamera.transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        movement = forward * moveVertical + right * moveHorizontal;

        // Nastaven� animac�
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
        if (Input.GetKeyDown(KeyCode.Space) && Mathf.Abs(rb.velocity.y) < 0.001f)
        {
            rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
            animator.SetBool("Jump", true);
        }
        else
        {
            animator.SetBool("Jump", false);
        }

        // Ot��en� postavy podle sm�ru pohybu
        if (isMoving)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        // Pohyb postavy
        Vector3 newPosition = rb.position + movement * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }
}
