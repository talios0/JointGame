using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneCamera : MonoBehaviour
{
    [Header("Components")]
    public Transform drone;
    private DroneMovement droneMovement;

    [Header("Camera Properties")]
    public float lookSens;
    public float highClamp, lowClamp;
    [Range(0,1)]
    public float turningModifier = 0.05f;

    // Input Storage
    private float input;
    private float vertRot;

    private bool disabled = false;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        droneMovement = drone.GetComponent<DroneMovement>();
    }

    private void Update()
    {
        if (disabled) return;
        UpdateInput();
    }

    private void FixedUpdate()
    {
        if (disabled) return;
        Look();
    }

    private void LateUpdate()
    {
        transform.position = drone.position;
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, drone.eulerAngles.y, drone.eulerAngles.z);
    }

    private float UpdateInput() {
        Debug.Log(Input.GetAxis("Mouse X"));
        droneMovement.MouseRotationInput(Input.GetAxis("Mouse X") * lookSens*turningModifier);
        input = -Input.GetAxis("Mouse Y");
        input *= lookSens;
        if (vertRot + input > highClamp) input = highClamp - vertRot;
        else if (vertRot + input < lowClamp) input = lowClamp - vertRot;
        return input;
    }

    private void Look() {
        if (input == 0) return;
        transform.Rotate(new Vector3(1, 0, 0), input);
        vertRot = transform.eulerAngles.x;
        if (vertRot > 180) vertRot -= 360;
    }

    // Public methods
    public Vector3 GetForward() {
        Quaternion rot = Quaternion.Euler(0, transform.eulerAngles.y, transform.eulerAngles.z);
        return rot * Vector3.forward;
    }

    public Vector3 GetRight() {
        Quaternion rot = Quaternion.Euler(0, transform.eulerAngles.y, transform.eulerAngles.z);
        return rot * Vector3.right;
    }

    public void Disable()
    {
        disabled = true;
    }

    public void Enable()
    {
        disabled = false;
    }
}
