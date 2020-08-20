using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerManager : MonoBehaviour
{
    private PlayerPlayState playState;

    [Header("Standard Components")]
    public Transform standard;
    public LargePlayerCamera standardCamera;
    public Camera attachedStandardCamera;
    public LargePlayerMovement standardMovement;

    [Header("Drone Componenets")]
    public Transform drone;
    public DroneCamera droneCamera;
    public Camera attachedDroneCamera;
    public DroneMovement droneMovement;
    public GameObject droneUI;

    [Header("Transition")]
    public VolumeProfile finalProfile;
    public VolumeProfile startProfile;
    public Volume transitionVolume;
    public Transform transitionTransform;
    public Camera transitionCamera;

    [Header("Bloom")]
    public Vector2 bloom;

    private bool transition = false;

    private void Start()
    {
        ChangePlayerPlayState(PlayerPlayState.STANDARD);
    }

    public void Update()
    {
        if (Input.GetButtonDown("Switch")) ShiftPlayerPlayState();
    }

    public void ChangePlayerPlayState(PlayerPlayState state)
    {
        switch (state)
        {
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

    public void ShiftPlayerPlayState()
    {
        switch (playState)
        {
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

    public PlayerPlayState GetPlayState()
    {
        return playState;
    }

    private void TransitionToStandard()
    {
        if (transition) return;
        transition = true;
        transitionTransform.position = droneCamera.transform.position;
        transitionTransform.rotation = droneCamera.transform.rotation;
        // Disable
        droneMovement.DisableMovement();
        attachedDroneCamera.enabled = false;
        droneCamera.enabled = false;
        droneUI.SetActive(false);

        transitionCamera.enabled = true;
        StartCoroutine(Transition(droneCamera.transform.position, standardCamera.transform.position, attachedDroneCamera, attachedStandardCamera));
    }

    private void TransitionToDrone()
    {
        if (transition) return;
        transition = true;
        transitionTransform.position = standardCamera.transform.position;
        transitionTransform.rotation = standardCamera.transform.rotation;
        transitionVolume.profile = startProfile;
        // Disable
        standardMovement.DisableMovement();
        attachedStandardCamera.enabled = false;
        standardCamera.enabled = true;

        transitionCamera.enabled = true;
        StartCoroutine(Transition(standardCamera.transform.position, droneCamera.transform.position, attachedStandardCamera, attachedDroneCamera));
    }

    private void EndTransition() {
        switch (playState) {
            case PlayerPlayState.STANDARD:
                standardMovement.EnableMovement();
                attachedStandardCamera.enabled = true;
                standardCamera.enabled = true;
                break;
            case PlayerPlayState.DRONE:
                droneMovement.EnableMovement();
                attachedDroneCamera.enabled = true;
                droneCamera.enabled = true;
                droneUI.SetActive(true);
                break;
            default:
                Debug.LogWarning("Unknown End Transition State: " + playState);
                break;
        }

        transitionCamera.enabled = false;
        transitionVolume.enabled = false;
        transition = false;
    }

    IEnumerator Transition(Vector3 startPos, Vector3 finalPos, Camera startCam, Camera endCam)
    {
        transitionVolume.enabled = true;
        Bloom targetBloom = null;
        if (!transitionVolume.profile.TryGet(out targetBloom))
        {
            Debug.LogError("Unable to retrieve Bloom");
            yield break;
        }
        for (float i = 0; i < Mathf.PI; i += 0.1f)
        {
            float weight = Mathf.Cos(i + Mathf.PI) / 2 + 0.5f;
            transitionTransform.position = new Vector3(Blend(startPos.x, finalPos.x, weight), Blend(startPos.y, finalPos.y, weight), Blend(startPos.z, finalPos.z, weight));
            targetBloom.intensity.value = Blend(bloom.x, bloom.y, weight); ;
            transitionCamera.fieldOfView = Blend(startCam.fieldOfView, endCam.fieldOfView, weight);
            transitionCamera.transform.rotation = Quaternion.Lerp(startCam.transform.rotation, endCam.transform.rotation, i / Mathf.PI);
            yield return new WaitForFixedUpdate();
        }
        EndTransition();
        yield break;
    }

    private float Blend(float start, float end, float weight) { 
        return (end * weight) + (start * (1 - weight));
    }
}
