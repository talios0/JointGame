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
    public float groundRadius;

    [Header("Component Reference")]
    public Collider col;
    private Rigidbody rb;

    [Header("States")]
    private GravityState gravState;

    // Other
    private bool frictionLock;
    private Collider floor;
    private bool isFloor;

    private Vector3 input;
    private Vector3 pos;


    // Sets the reference to the player's rigidbody
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gravState = GravityState.GROUND;
    }

    void FixedUpdate()
    {
        Movement();

        floor = GetClosestFloor();
        isFloor = floor != null ? true : false;

        if (gravState == GravityState.GROUND) GroundUpdate();
        else if (gravState == GravityState.AIRBORN) AirbornUpdate();

        ClampMovement();
    }

    // Gets the user input and applies it to the player
    private void Movement()
    {
        input = Input.GetAxisRaw("Vertical") * transform.forward + Input.GetAxisRaw("Horizontal") * transform.right;
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
        if (!isFloor) return false;
        float angle = floor.transform.rotation.eulerAngles.y;


        if (angle > maxSlopeAngle) return false;
        return true;
    }

    private void SnapToGround()
    {
        if (!isFloor)
        {
            SetGravityState(GravityState.AIRBORN);
            return;
        }
        if (frictionLock) return;
        pos = new Vector3(transform.position.x, GetClosestFloor().ClosestPoint(transform.position).y + playerGroundOffset, transform.position.z);
        transform.position = pos;
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
        AddFriction();
    }

    private void CheckLanded()
    {
        if (!isFloor) return;
        if (Mathf.Abs(rb.velocity.y) >= 0.1f) return;
        SetGravityState(GravityState.GROUND);
    }

    private void ApplyGravity()
    {
        rb.AddForce(Vector3.down * gravity);
    }

    private void SetGravityState(GravityState state)
    {
        gravState = state;
        switch (gravState)
        {
            case GravityState.AIRBORN:
                SetAirbornPhysics();
                break;
            case GravityState.GROUND:
                SetGroundPhysics();
                break;
        }
    }

    private void SetAirbornPhysics()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void SetGroundPhysics()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
    }

    private Collider GetClosestFloor()
    {
        Collider[] col = Physics.OverlapSphere(transform.position + playerGroundOffset * Vector3.down, groundRadius, 1 << LayerMask.NameToLayer("Ground"));
        Collider closest = null;
        float distance = 0;
        for (int i = 0; i < col.Length; i++) {
            float newDistance = Vector3.Distance(col[i].bounds.ClosestPoint(transform.position), transform.position);
            if (closest == null || newDistance < distance) {
                closest = col[i];
                distance = newDistance;
            }
        }
        return closest;
    }

}
