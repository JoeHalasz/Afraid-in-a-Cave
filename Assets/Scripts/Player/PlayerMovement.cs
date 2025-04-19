using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    float speed = 4.5f; // Speed of the player
    float jumpForce = 300f; // Jump force of the player
    float acceleration = 10f; // Acceleration of the player
    float deceleration = 8f; // Deceleration when no input is given
    Rigidbody rb; // Reference to the Rigidbody component
    bool isGrounded; // Check if the player is on the ground
    float lastGroundedTime = 0f;
    float lastJumpTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Get the Rigidbody component attached to the player
    }

    void Update()
    {
        if (!IsOwner || !IsSpawned) return;
        Move(); // Call the Move function every frame
        Jump(); // Call the Jump function every frame
    }

    void Move()
    {
        if (isGrounded || Time.time - lastGroundedTime > 2f)
        {
            float horizontal = 0;
            float vertical = 0;

            if (Input.GetKey(KeyCode.W))
                vertical = 1; // Move forward
            else if (Input.GetKey(KeyCode.S))
                vertical = -1; // Move backward

            if (Input.GetKey(KeyCode.A))
                horizontal = -1; // Move left
            else if (Input.GetKey(KeyCode.D))
                horizontal = 1; // Move right

            Vector3 moveDirection = transform.forward * vertical + transform.right * horizontal;

            if (moveDirection.magnitude > 1f)
                moveDirection.Normalize();

            // Apply acceleration force
            Vector3 targetVelocity = moveDirection * speed;
            Vector3 velocityChange = targetVelocity - rb.linearVelocity;
            velocityChange.y = 0; // Ignore vertical changes
            rb.AddForce(velocityChange * acceleration, ForceMode.Acceleration);

            // Apply deceleration when no input is given
            if (moveDirection.magnitude == 0)
            {
                Vector3 decelerationForce = -rb.linearVelocity * deceleration;
                decelerationForce.y = 0; // Ignore vertical changes
                rb.AddForce(decelerationForce, ForceMode.Acceleration);
            }
        }
    }

    void Jump()
    {
        if (isGrounded && Input.GetButtonDown("Jump") && Time.time - lastJumpTime > 1f) // Check if the player is grounded and the jump button is pressed
        {
            lastJumpTime = Time.time;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); // Apply an upward force to make the player jump
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground")) // Check if the player collides with an object tagged as "Ground"
            isGrounded = true; // Set isGrounded to true when touching the ground
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground")) // Check if the player is still colliding with an object tagged as "Ground"
            isGrounded = true; // Keep isGrounded true while touching the ground
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground")) // Check if the player exits collision with an object tagged as "Ground"
        {
            isGrounded = false; // Set isGrounded to false when leaving the ground
            lastGroundedTime = Time.time;
        }
    }
}
