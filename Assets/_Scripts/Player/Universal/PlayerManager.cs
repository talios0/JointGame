using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private PlayerPlayState playState;

    [Header("Standard Components")]
    public Transform standard;
    public LargePlayerCamera standardCamera;
    public LargePlayerMovement standardMovement;
    public Vector3 standardCamPos;

    [Header("Drone Componenets")]
    public Transform drone;
    public DroneCamera droneCamera;
    public DroneMovement droneMovement;

    private void Start()
    {
        ChangePlayerPlayState(PlayerPlayState.STANDARD);
    }

    public void Update()
    {
        if (Input.GetButtonDown("Switch")) ShiftPlayerPlayState();
    }

    public void ChangePlayerPlayState(PlayerPlayState state) {
        switch (state) {
            case PlayerPlayState.STANDARD:
                playState = state;
                TransitionToStandard();
                break;
            case PlayerPlayState.DRONE:
                playState = state;
                TransitionToDrone();
                break;
            default:
                Debug.LogWarning("No valid state for: " + state);
                break;
        }
    }

    public void ShiftPlayerPlayState() {
        switch (playState) {
            case PlayerPlayState.STANDARD:
                ChangePlayerPlayState(PlayerPlayState.DRONE);
                break;
            case PlayerPlayState.DRONE:
                ChangePlayerPlayState(PlayerPlayState.STANDARD);
                break;
            default:
                Debug.LogWarning("No valid state shift for: " + playState);
                break;
        }
    }

    private void TransitionToStandard() {

    }

    private void TransitionToDrone() {
    }
}
