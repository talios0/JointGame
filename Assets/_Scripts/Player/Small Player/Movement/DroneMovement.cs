using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneMovement : MonoBehaviour
{
    [Header("Speed")]
    public float incrementSpeed;
    public float maxSpeed;

    [Header("Friction")]
    public float frictionMultiplier;

    [Header("Gravity")]
    public float gravity;
    private float groundDistance = 1;

    [Header("Component Reference")]
    private Rigidbody rb;

    [Header("States")]
    private GravityState gravState;
    private DroneMovementState moveState;

    [Header("Input Storage")]
    private float xInput, yInput, zInput;

    // Unity Start, Update, and Fixed Update functions

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        gravState = GravityState.GROUND;
        moveState = DroneMovementState.IDLE;
    }

    private void Update()
    {
        MoveStateUpdater();
    }

    private void FixedUpdate()
    {
        FixedMoveStateUpdater();
    }

    // Input
    private void UpdateInput() {
        xInput = Input.GetAxisRaw("Vertical");
        zInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Jump");
    }

    // Apply Movement
    private void ApplyMovement() {
        if (xInput == 0 && zInput == 0) return;
        rb.AddForce((xInput * transform.forward + zInput * transform.right) * incrementSpeed, ForceMode.Acceleration);
    }

    // Clamp Movement
    private void ClampMovement() {
        Vector3 vel = rb.velocity;
        float yVel = vel.y;

        vel.y = 0;
        if (vel.magnitude < maxSpeed) return;
        vel = Vector3.ClampMagnitude(vel, maxSpeed);
        vel.y = yVel;

        rb.velocity = vel;
    }

    // Friction
    private void AddFriction() {
        Vector3 frictionDamp = -rb.velocity * frictionMultiplier;
        frictionDamp.y = 0;
        if (Mathf.Abs(frictionDamp.x) < 0.05f && Mathf.Abs(frictionDamp.z) < 0.05f)
        {
            if (new Vector3(rb.velocity.x, 0, rb.velocity.z) != Vector3.zero) rb.velocity = new Vector3(0, 0, 0);
            return;
        }

        if (frictionDamp.magnitude > maxSpeed) frictionDamp = -rb.velocity;
        rb.AddForce(frictionDamp, ForceMode.VelocityChange);
    }

    // Gravity
    private void ApplyGravity() {
        rb.AddForce(Vector3.down * gravity);
    }

    // State Handlers
    private void MoveStateUpdater() {
        switch (moveState) {
            case DroneMovementState.IDLE:
                UpdateInput();
                break;
            case DroneMovementState.MOVING:
                UpdateInput();
                break;
            case DroneMovementState.NOTCONTROLLED:
                break;
            default:
                Debug.LogWarning("[DroneMovement.cs] DroneMovementState: <b>" + moveState + "</b> is not recognized");
                break;
        }
    }

    private void FixedMoveStateUpdater()
    {
        switch (moveState)
        {
            case DroneMovementState.IDLE:
                ApplyMovement();
                ClampMovement();
                AddFriction();
                ApplyGravity();
                break;
            case DroneMovementState.MOVING:
                ApplyMovement();
                ClampMovement();
                AddFriction();
                ApplyGravity();
                break;
            case DroneMovementState.NOTCONTROLLED:
                AddFriction();
                ApplyGravity();
                break;
            default:
                Debug.LogWarning("[DroneMovement.cs] DroneMovementState: <b>" + moveState + "</b> is not recognized");
                break;
        }
    }
}
