using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    float moveSpeed = 3f; // Speed of the player
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

    PlayerInput _playerInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Get the Rigidbody component attached to the player
        _input = GetComponent<Inputs>();
        _playerInput = GetComponent<PlayerInput>();
        AssignAnimationIDs();
        bodyTypes = GetComponent<PlayerChange>().getBodyTypes();
        findBodies();
        _fallTimeoutDelta = FallTimeout;
        // InvokeRepeating("findAnimator", 0f, 0.1f);
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
            if (body != null && body.activeSelf)
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
                bodies.Add(body);
        }
    }

    void Move()
    {
        if (!_playerInput.enabled) // this needs to be deactive at the start for multiplayer. See https://discussions.unity.com/t/player-input-components-not-working-on-spawned-playerobjects/885960/3
            _playerInput.enabled = true;
        float targetSpeed = _input.sprint ? moveSpeed*1.5f : moveSpeed;
        if (_input.move == Vector2.zero) targetSpeed = 0.0f;
        float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;
        float speed;
        
        if (isGrounded || Time.time - lastGroundedTime > 2f)
        {
            speed = targetSpeed * inputMagnitude;

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 right = transform.TransformDirection(Vector3.right);
            inputDirection = (inputDirection.x * right + inputDirection.z * forward).normalized;

            // set velocity
            rb.linearVelocity = new Vector3(inputDirection.x * speed, rb.linearVelocity.y, inputDirection.z * speed);

            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, speed);//_animationBlend);
                if (_input.move.y < 0) // Moving backward
                    inputMagnitude = -inputMagnitude;
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }

            // slow down
            if (inputDirection.x == 0)
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, rb.linearVelocity.z);
            if (inputDirection.z == 0)
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, 0);
        }
    }

    void Jump()
    {
        if (_hasAnimator)
        {
            _animator.SetBool(_animIDGrounded, isGrounded);

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
