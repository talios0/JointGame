using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LargePlayerCamera : MonoBehaviour
{
    public Transform player;

    [Header("Camera Properties")]
    public float lookSens;
    public float highClamp, lowClamp;


    private Vector2 input;
    private float vertRot;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        input = GetInput();
    }

    private void FixedUpdate()
    {
        Look(input);
    }

    private Vector2 GetInput() {
        Vector2 input = new Vector2(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"));
        input*=lookSens;
        if (vertRot-input.y > highClamp) {
            input.y = -(highClamp - vertRot);
        }
        else if (vertRot-input.y < lowClamp)
        {
            input.y = -(lowClamp - vertRot);
        }
        vertRot += -input.y;
        return input;
    }

    private void Look(Vector2 input)
    {
        transform.Rotate(new Vector3(1, 0, 0), input.y);
        player.transform.Rotate(new Vector3(0, 1, 0), input.x);
    }

}
