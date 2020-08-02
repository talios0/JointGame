using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneCamera : MonoBehaviour
{
    public Transform drone;

    [Header("Camera Properties")]
    public float lookSens;
    public float highClamp, lowClamp;

    private float input;
    private float vertRot;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        UpdateInput();
    }

    private void FixedUpdate()
    {
        Look();
    }

    private void LateUpdate()
    {
        transform.position = drone.position;
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, drone.eulerAngles.y, drone.eulerAngles.z);
    }

    private float UpdateInput() {
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
}
