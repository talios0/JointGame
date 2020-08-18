using UnityEngine;

public class DroneMovement : MonoBehaviour
{
    [Header("Component References")]
    public Rigidbody rb;
    public new DroneCamera camera;
    public PlayerManager manager;

    [Header("Physics")]
    public float gravity = 15;

    [Header("Movement")]
    public float accelerationModifier;
    public float maxSpeed;
    public float incrementFriction;

    [Header("Angular Movement")]
    public float angularAcceleration;
    public float maxAngularSpeed;
    public float angularDragModifier;

    [Header("Jump")]
    public float jumpAcceleration;
    public float forwardsAcceleration;
    public float groundCheckRadius;

    // Input Storage
    private float xInput, yInput, zInput;

    private bool disabled = false;


    // Unity Start/Update methods
    private void Start()
    {
        
    }

    private void Update()
    {
        if (!disabled) UpdateInput();
    }

    private void FixedUpdate()
    {
        if (!disabled) {
            ApplyMovement();
            Jump();
        }
        ApplyGravity();
        ApplyFriction();
        ApplyAngularDrag();
        ClampSpeed();
        ClampAngularSpeed();
    }

    private void LateUpdate()
    {
        if ((transform.eulerAngles.x >= 90 && transform.eulerAngles.x < 180) || (transform.eulerAngles.x <= 270 && transform.eulerAngles.x > 180)) {
            transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, transform.eulerAngles.z);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position + Vector3.down * 0.1f, groundCheckRadius);
    }

    // Input
    private void UpdateInput() {
        xInput = Input.GetAxisRaw("Vertical");
        zInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Jump");
    }

    public void MouseRotationInput(float amount) {
        zInput += amount;
        if (zInput > 1) zInput = 1;
        else if (zInput < -1) zInput = -1;
    }

    // Movement Application
    private void ApplyMovement() {
        rb.AddForce(xInput * camera.GetForward() * accelerationModifier, ForceMode.Acceleration);
        rb.AddForceAtPosition(-zInput * camera.GetForward() * angularAcceleration, transform.right, ForceMode.Acceleration);
        rb.AddForceAtPosition(zInput * camera.GetForward() * angularAcceleration, -transform.right, ForceMode.Acceleration);
    }

    private void ApplyGravity() {
        rb.AddForce(Vector3.down *gravity, ForceMode.Acceleration);
    }

    // Jump + Checks
    private void Jump() {
        if (yInput == 0 || Mathf.Abs(rb.velocity.y) > 0.05f|| !CheckGround()) return;
        rb.AddForce(camera.GetForward() * forwardsAcceleration + Vector3.up * jumpAcceleration, ForceMode.Impulse);
    }

    private bool CheckGround() {
        int layerMask = ~(1 << LayerMask.NameToLayer("Drone"));
        return Physics.CheckSphere(transform.position + Vector3.down * 0.1f, groundCheckRadius, layerMask);
    }

    // Friction and Drag
    private void ApplyFriction() {
        Vector3 frictionDamp = -rb.velocity * incrementFriction;
        frictionDamp.y = 0;
        if (Mathf.Abs(frictionDamp.x) < 0.05f && Mathf.Abs(frictionDamp.z) < 0.05f)
        {
            if (new Vector3(rb.velocity.x, 0, rb.velocity.z) != Vector3.zero) rb.velocity = new Vector3(0, rb.velocity.y, 0);
            return;
        }
        if (frictionDamp.magnitude > maxSpeed) frictionDamp = -rb.velocity;

        rb.AddForce(frictionDamp, ForceMode.VelocityChange);
    }

    private void ApplyAngularDrag() {
        Vector3 drag = -rb.angularVelocity * angularDragModifier;
        drag.x = 0;
        if (Mathf.Abs(drag.y) < 0.05f && Mathf.Abs(drag.z) < 0.05f) {
            if (new Vector3(0, rb.angularVelocity.y, rb.angularVelocity.z) != Vector3.zero) rb.angularVelocity = new Vector3(0, 0, 0);
            return;
        }

        if (drag.magnitude > maxAngularSpeed) drag = -rb.angularVelocity;

        rb.AddTorque(drag, ForceMode.VelocityChange);
    }


    // Velocity Clamps
    private void ClampSpeed() {
        float yVel = rb.velocity.y;
        Vector3 vel = rb.velocity;

        vel.y = 0;
        vel = Vector3.ClampMagnitude(vel, maxSpeed);
        vel.y = yVel;

        rb.velocity = vel;
    }

    private void ClampAngularSpeed() {
        Vector3 vel = rb.angularVelocity;

        vel = Vector3.ClampMagnitude(vel, maxAngularSpeed);
        vel.x = 0;

        rb.angularVelocity = vel;
    }

    public void DisableMovement() {
        disabled = true;
        xInput = 0;
        yInput = 0;
        zInput = 0;
    }

    public void EnableMovement()
    {
        disabled = false;
    }
}
