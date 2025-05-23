using UnityEngine;
using System.Collections.Generic;

public class Minecart : MonoBehaviour
{
    List<GameObject> wheels = new List<GameObject>();

    float currentSpeed = 0f;
    float maxSpeed;
    float acceleration = 2f;

    float movingDirection = 0f;
    [SerializeField]
    public bool startMoving = false;
    float startMovingTime = 0f;
    float stopMovingTime = 5f;
    float comeBackTime = 3f;
    Vector3 startPosition;
    float bufferTime = .1f;
    float lastSpeedSign = 1f;
    bool moving = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        maxSpeed = acceleration * stopMovingTime + 1;
        foreach (Transform child in transform)
            if (child.name == "Wheel")
                wheels.Add(child.gameObject);
    }

    public void startMovingForwards()
    {
        if (currentSpeed != 0f)
            return;
        movingDirection = -1f;
        startMovingTime = Time.time;
        moving = true;
        startPosition = transform.position;
    }

    void Update()
    {
        if (startMoving)
        {
            startMoving = false;
            startMovingForwards();
        }

        if (moving && stopMovingTime + startMovingTime > Time.time)
        {
            // rotate the wheels around their bounding box center
            foreach (GameObject wheel in wheels)
                wheel.transform.Rotate(0, 0, -currentSpeed * 360 * Time.deltaTime * .4f);
            currentSpeed += movingDirection * acceleration * Time.deltaTime;
            if (currentSpeed > maxSpeed)
                currentSpeed = maxSpeed;
            else if (currentSpeed < -maxSpeed)
                currentSpeed = -maxSpeed;
            // move this object in the forward direction by the current speed
            transform.position += transform.right * currentSpeed * Time.deltaTime;
        }
        else if (moving && comeBackTime + stopMovingTime + startMovingTime < Time.time)
        {
            // rotate the wheels around their bounding box center
            foreach (GameObject wheel in wheels)
                wheel.transform.Rotate(0, 0, currentSpeed * 360 * Time.deltaTime * .4f);
            currentSpeed -= movingDirection * acceleration * Time.deltaTime;
            if (Mathf.Abs(currentSpeed) < .5f)
                currentSpeed = .5f * movingDirection;
            
            if (currentSpeed > maxSpeed)
                currentSpeed = maxSpeed;
            else if (currentSpeed < -maxSpeed)
                currentSpeed = -maxSpeed;
            // move this object in the backward direction by the current speed
            transform.position -= transform.right * currentSpeed * Time.deltaTime;
            if (Vector3.Distance(transform.position, startPosition) < .01f)
            {
                transform.position = startPosition;
                currentSpeed = 0f;
                moving = false;
            }
        }
    }
}