using UnityEngine;

public class LargePlayerMovement : MonoBehaviour
{
    [Header("Speed")]
    public float incrementSpeed;
    public float maxSpeed;

    [Header("Friction")]
    public float incrementFriction;

    [Header("Slopes")]
    public float slopeCheckDistance;
    public float maxSlopeAngle;

    [Header("Gravity")]
    public float groundDistance;
    public float airbornGroundDistance;
    public float playerGroundOffset;
    public float gravity;

    [Header("Component Reference")]
    private Rigidbody rb;

    [Header("States")]
    private GravityState gravState;

    // Other
    private bool frictionLock;


    // Sets the reference to the player's rigidbody
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gravState = GravityState.GROUND;
    }

    void FixedUpdate()
    {
        Movement();
        if (gravState == GravityState.GROUND) GroundUpdate();
        else if (gravState == GravityState.AIRBORN) AirbornUpdate();

        ClampMovement();
    }

    // Gets the user input and applies it to the player
    private void Movement()
    {
        Vector3 input = Input.GetAxisRaw("Vertical") * transform.forward + Input.GetAxisRaw("Horizontal") * transform.right;
        if (input == Vector3.zero) return;
        frictionLock = false;

        input *= incrementSpeed;
        input.y = 0; // Stops the player from accidentally moving vertically

        rb.AddForce(input, ForceMode.VelocityChange);
    }

    // Adds friction to the user manually
    private void AddFriction()
    {
        Vector3 frictionDamp = -rb.velocity * incrementFriction;
        frictionDamp.y = 0;
        if (Mathf.Abs(frictionDamp.x) < 0.05f && Mathf.Abs(frictionDamp.z) < 0.05f)
        {
            if (new Vector3(rb.velocity.x, 0, rb.velocity.z) != Vector3.zero) rb.velocity = new Vector3(0, 0, 0);
            frictionLock = true;
            return;
        }
        if (frictionDamp.magnitude >= maxSpeed) frictionDamp = -rb.velocity;

        rb.AddForce(frictionDamp, ForceMode.VelocityChange);
    }

    // Prevents the player from moving too fast
    private void ClampMovement()
    {
        float yVel = rb.velocity.y;
        Vector3 vel = rb.velocity;

        vel.y = 0;
        vel = Vector3.ClampMagnitude(vel, maxSpeed);
        vel.y = yVel;

        rb.velocity = vel;
    }

    // Prevents gravity from pulling the player down slopes if the angle is slight
    private bool isSlope()
    {
        RaycastHit hit;

        if (!Physics.Raycast(transform.position, Vector3.down, out hit, slopeCheckDistance)) return false;
        float angle = hit.transform.rotation.eulerAngles.y;
        

        if (angle > maxSlopeAngle) return false;
        return true;
    }

    private void SnapToGround()
    {
        RaycastHit hit;
        if (!Physics.Raycast(transform.position, Vector3.down, out hit, groundDistance, 1 << LayerMask.NameToLayer("Ground")))
        {
            SetGravityState(GravityState.AIRBORN);
            return;
        }
        if (frictionLock) return;
        transform.position = new Vector3(transform.position.x, hit.point.y + playerGroundOffset, transform.position.z);

    }

    private void GroundUpdate()
    {
        AddFriction();
        SnapToGround();
    }

    private void AirbornUpdate()
    {
        CheckLanded();
        ApplyGravity();
    }

    private void CheckLanded()
    {
        RaycastHit hit;
        if (!Physics.Raycast(transform.position, Vector3.down, out hit, airbornGroundDistance, 1 << LayerMask.NameToLayer("Ground"))) return;
        if (Mathf.Abs(rb.velocity.y) >= 0.1f) return;
        SetGravityState(GravityState.GROUND);
    }

    private void ApplyGravity() {
        rb.AddForce(Vector3.down * gravity);
    }

    private void SetGravityState(GravityState state) {
        gravState = state;
        switch (gravState) {
            case GravityState.AIRBORN:
                SetAirbornPhysics();
                break;
            case GravityState.GROUND:
                SetGroundPhysics();
                break;
        }
    }

    private void SetAirbornPhysics() {
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void SetGroundPhysics()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
    }
}
