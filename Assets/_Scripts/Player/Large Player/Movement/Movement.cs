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

    [Header("Component Reference")]
    private Rigidbody rb;


    // Sets the reference to the player's rigidbody
    void Start() {rb = GetComponent<Rigidbody>();}

    void FixedUpdate()
    {
        Movement();
        AddFriction();
        ClampMovement();
    }

    // Gets the user input and applies it to the player
    private void Movement() {
        Vector3 input = Input.GetAxisRaw("Vertical") * transform.forward + Input.GetAxisRaw("Horizontal") * transform.right;
        if (input == Vector3.zero) {
            Slope();
            return;
        }
        
        input *= incrementSpeed;
        input.y = 0; // Stops the player from accidentally moving vertically

        rb.AddForce(input, ForceMode.VelocityChange);
    }

    // Adds friction to the user manually
    private void AddFriction() {
        Vector3 frictionDamp = -rb.velocity * incrementFriction;
        frictionDamp.y = 0;

        if(Mathf.Abs(frictionDamp.x) < 0.05f && Mathf.Abs(frictionDamp.z) < 0.05f) {
            if (new Vector3(rb.velocity.x, 0, rb.velocity.z) != Vector3.zero) rb.velocity = new Vector3(0,rb.velocity.y, 0);
            return;
        }
        if (frictionDamp.magnitude >= maxSpeed) frictionDamp = -rb.velocity;

        rb.AddForce(frictionDamp, ForceMode.VelocityChange);
    }

    // Prevents the player from moving too fast
    private void ClampMovement() {
        float yVel = rb.velocity.y;
        Vector3 vel = rb.velocity;

        vel.y = 0;
        vel = Vector3.ClampMagnitude(vel, maxSpeed);
        vel.y = yVel;

        rb.velocity = vel;
    }

    // Prevents gravity from pulling the player down slopes if the angle is slight
    private void Slope() {
        RaycastHit hit;

        if (!Physics.Raycast(transform.position, Vector3.down, out hit, slopeCheckDistance))  return;
        float angle = hit.transform.rotation.eulerAngles.y;

        //if (angle <= maxSlopeAngle) rb.velocity = new Vector3(0, rb.velocity.y, 0);
    }
}
