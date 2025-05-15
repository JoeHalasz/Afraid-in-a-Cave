using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class PlayerMovement : NetworkBehaviour
{
    float moveSpeed = 4.5f; // Speed of the player
    float jumpForce = 300f; // Jump force of the player
    float acceleration = 10f; // Acceleration of the player
    float deceleration = 8f; // Deceleration when no input is given
    Rigidbody rb; // Reference to the Rigidbody component
    bool isGrounded; // Check if the player is on the ground
    float lastGroundedTime = 0f;
    float lastJumpTime = 0f;
    float SpeedChangeRate = 10f;
    float FallTimeout = 0.15f;

    // animation stuff
    private float _animationBlend;
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;
    private Animator _animator;
    private bool _hasAnimator;

    List<string> bodyTypes;
    List<GameObject> bodies = new List<GameObject>();

    private Inputs _input;
    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    void Start()
    {
        if (!HasAuthority || !IsSpawned) return;
        rb = GetComponent<Rigidbody>(); // Get the Rigidbody component attached to the player
        _input = GetComponent<Inputs>();
        AssignAnimationIDs();
        bodyTypes = GetComponent<PlayerChange>().getBodyTypes();
        findBodies();
        _fallTimeoutDelta = FallTimeout;
    }

    void Update()
    {
        if (!HasAuthority || !IsSpawned) return;
        findAnimator();
        Move();
        Jump();
    }

    void findAnimator()
    {
        foreach (GameObject body in bodies)
        {
            if (body != null)
            {
                _hasAnimator = body.TryGetComponent(out _animator);
                if (_hasAnimator)
                    break;
            }
        }
    }

    void findBodies()
    {
        bodies.Clear();
        foreach (string bodyType in bodyTypes)
        {
            GameObject body = transform.Find(bodyType).gameObject;
            if (body != null)
                if (body.activeSelf)
                    bodies.Add(body);
        }
    }

    void Move()
    {
        float targetSpeed = _input.sprint ? moveSpeed*2 : moveSpeed;
        if (_input.move == Vector2.zero) targetSpeed = 0.0f;
        float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;
        float currentHorizontalSpeed = rb.linearVelocity.magnitude;
        float speedOffset = 0.1f;
        float speed;
        
        if (isGrounded || Time.time - lastGroundedTime > 2f)
        {
            // if (currentHorizontalSpeed < targetSpeed - speedOffset ||
            //     currentHorizontalSpeed > targetSpeed + speedOffset)
            // {
            //     speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
            //         Time.deltaTime * SpeedChangeRate);

            //     // round speed to 3 decimal places
            //     speed = Mathf.Round(speed * 1000f) / 1000f;
            // }
            // else
                speed = targetSpeed * inputMagnitude;
            Debug.Log("Speed: " + speed);

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // move
            rb.AddForce(inputDirection * speed * acceleration, ForceMode.Acceleration);

            // dont go over max speed
            if (rb.linearVelocity.magnitude > targetSpeed)
            {
                Vector3 force = rb.linearVelocity.normalized * targetSpeed;
                force.y = rb.linearVelocity.y; // keep vertical speed
                rb.linearVelocity = force;
            }

            // Update animator if using character
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, 1.0f);//_animationBlend);

                if (_input.move.y < 0) // Moving backward
                    inputMagnitude = -inputMagnitude;

                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        // Apply deceleration when no input is given
        if (targetSpeed == 0.0f)
        {
            Vector3 decelerationForce = -rb.linearVelocity * (deceleration/100);
            decelerationForce.y = 0; // Ignore vertical changes
            // rb.AddForce(decelerationForce, ForceMode.Acceleration);
        }

    }

    void Jump()
    {
        if (_hasAnimator)
        {
            if(isGrounded)
            {
                _fallTimeoutDelta = FallTimeout;
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, false);
            }
            else
            {
                if (_fallTimeoutDelta >= 0.0f)
                    _fallTimeoutDelta -= Time.deltaTime;
                else
                    _animator.SetBool(_animIDFreeFall, true);
            }
        }
        if (isGrounded && Input.GetButtonDown("Jump") && Time.time - lastJumpTime > 1f) // Check if the player is grounded and the jump button is pressed
        {
            // update animator if using character
            if (_hasAnimator)
                _animator.SetBool(_animIDJump, true);
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

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }
}
